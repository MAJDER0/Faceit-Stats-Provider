using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Services
{
    public class PlayerStatisticsService : IPlayerStatistics
    {
        private readonly HttpClient _client;
        private readonly GetOrAddToCache _cacheHelper;
        private readonly HandleHttpRequest _handleHttpRequestHelper;

        public PlayerStatisticsService(IHttpClientFactory clientFactory, IMemoryCache memoryCache)
        {
            _client = clientFactory.CreateClient("Faceit");
            _cacheHelper = new GetOrAddToCache(memoryCache);
            _handleHttpRequestHelper = new HandleHttpRequest();
        }

        public async Task<(List<(string playerId, AnalyzerMatchStats.Rootobject)> playerMatchStats, List<AnalyzerPlayerStatsCombined.Rootobject> combinedPlayerStats)> ProcessPlayerStatisticsAsync(
            List<(string playerId, AnalyzerMatchHistory.Rootobject)> playerMatchHistory,
            List<AnalyzerPlayerStats.Rootobject> playerStats,
            List<AnalyzerPlayerStatsForCsgo.Rootobject> playerStatsForCsGo)
        {
            var getPlayerMatchStatsTasks = new List<(string playerId, Task<AnalyzerMatchStats.Rootobject>)>();

            foreach (var (playerId, playerHistory) in playerMatchHistory)
            {
                foreach (var matchItem in playerHistory.items)
                {
                    getPlayerMatchStatsTasks.Add((playerId, _cacheHelper.GetOrAddToCacheAsync($"match_stats_{matchItem.match_id}", () => _client.GetFromJsonAsync<AnalyzerMatchStats.Rootobject>($"v4/matches/{matchItem.match_id}/stats"))));
                }
            }

            var playerMatchStatsResults = await Task.WhenAll(getPlayerMatchStatsTasks.Select(task => _handleHttpRequestHelper.HandleHttpRequestAsync(task.Item2).ContinueWith(t => (task.playerId, Result: t.Result))));
            var playerMatchStats = playerMatchStatsResults.Where(result => result.Result != null).ToList();

            var combinedPlayerStats = playerStats.Zip(playerStatsForCsGo, (cs2, csgo) =>
            {
                var combinedSegments = new Dictionary<string, AnalyzerPlayerStatsCombined.Segment>();

                if (cs2?.segments != null)
                {
                    foreach (var cs2Segment in cs2.segments)
                    {
                        if (!cs2Segment.mode.ToLower().Contains("wingman"))
                        {
                            var normalizedLabel = UtilityForAnalyzer.NormalizeLabel(cs2Segment.label);
                            combinedSegments[normalizedLabel] = Converters.ConvertToCombinedSegment(cs2Segment);
                        }
                    }
                }

                if (csgo?.segments != null)
                {
                    var csgoProperMaps = csgo.segments.Where(mode => mode.mode == "5v5").ToList();

                    foreach (var csgoSegment in csgoProperMaps)
                    {
                        if (!csgoSegment.mode.ToLower().Contains("wingman"))
                        {
                            var normalizedLabel = UtilityForAnalyzer.NormalizeLabel(csgoSegment.label);
                            if (combinedSegments.TryGetValue(normalizedLabel, out var existingSegment))
                            {
                                combinedSegments[normalizedLabel] = SegmentCombiner.CombineSegments(existingSegment, Converters.ConvertToCombinedSegment(csgoSegment));
                            }
                            else
                            {
                                combinedSegments[normalizedLabel] = Converters.ConvertToCombinedSegment(csgoSegment);
                            }
                        }
                    }
                }

                return new AnalyzerPlayerStatsCombined.Rootobject
                {
                    player_id = cs2?.player_id ?? csgo?.player_id,
                    game_id = cs2?.game_id ?? csgo?.game_id,
                    lifetime = new AnalyzerPlayerStatsCombined.Lifetime
                    {
                        Wins = cs2?.lifetime?.Wins ?? csgo?.lifetime?.Wins,
                        TotalHeadshots = cs2?.lifetime?.TotalHeadshots ?? csgo?.lifetime?.TotalHeadshots,
                        LongestWinStreak = cs2?.lifetime?.LongestWinStreak ?? csgo?.lifetime?.LongestWinStreak,
                        KDRatio = cs2?.lifetime?.KDRatio ?? csgo?.lifetime?.KDRatio,
                        Matches = cs2?.lifetime?.Matches ?? csgo?.lifetime?.Matches,
                        AverageHeadshots = cs2?.lifetime?.AverageHeadshots ?? csgo?.lifetime?.AverageHeadshots,
                        AverageKDRatio = cs2?.lifetime?.AverageKDRatio ?? csgo?.lifetime?.AverageKDRatio,
                        WinRate = cs2?.lifetime?.WinRate ?? csgo?.lifetime?.WinRate,
                        ExtensionData = cs2?.lifetime?.ExtensionData ?? csgo?.lifetime?.ExtensionData
                    },
                    segments = combinedSegments.Values.ToArray()
                };
            }).ToList();

            return (playerMatchStats, combinedPlayerStats);
        }            
       
    }
}
