// File: Services/GetMatchDetailsService.cs
using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Faceit_Stats_Provider.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;

namespace Faceit_Stats_Provider.Services
{
    public class GetMatchDetailsService : IGetMatchDetails
    {
        private readonly HttpClient client;
        private readonly GetOrAddToCache _cacheHelper;

        public GetMatchDetailsService(IHttpClientFactory clientFactory, IMemoryCache memoryCache)
        {
            this.client = clientFactory.CreateClient("Faceit");
            _cacheHelper = new GetOrAddToCache(memoryCache);
        }

        public async Task<GetMatchDetailsResult> GetMatchDetailsAsync(string RoomID)
        {
            var getPlayerStatsTasks = new List<Task<AnalyzerPlayerStats.Rootobject>>();
            var getPlayerStatsForCsGoTasks = new List<Task<AnalyzerPlayerStatsForCsgo.Rootobject>>();
            var getPlayerMatchHistoryTasks = new List<(string playerId, Task<AnalyzerMatchHistory.Rootobject>)>();

            AnalyzerMatchPlayers.Rootobject players;

            players = await _cacheHelper.GetOrAddToCacheAsync($"players_{RoomID}", () => client.GetFromJsonAsync<AnalyzerMatchPlayers.Rootobject>($"v4/matches/{RoomID}"));

            if (players.teams.faction1.roster is null || players.teams.faction2.roster is null)
            {
                AnalyzerMatchPlayersOldMatch.Rootobject oldMatch = await client.GetFromJsonAsync<AnalyzerMatchPlayersOldMatch.Rootobject>($"v4/matches/{RoomID}");

                players = CovertOldPlatformMatchForAnalyzer.ConvertOldMatchToNew(oldMatch);
            }

            foreach (var item in players.teams.faction1.roster)
            {
                try
                {
                    var cs2stats = await client.GetFromJsonAsync<AnalyzerPlayerStats.Rootobject>($"v4/players/{item.player_id}/stats/cs2");
                    if (cs2stats != null)
                    {
                        getPlayerStatsTasks.Add(Task.FromResult(cs2stats));
                    }
                }
                catch
                {
                    getPlayerStatsTasks.Add(
                        Task.FromResult(new AnalyzerPlayerStats.Rootobject
                        {
                            player_id = item.player_id,
                            lifetime = null,
                            segments = null,
                            game_id = item.game_player_id,
                        })
                    );
                }
                try
                {
                    getPlayerStatsForCsGoTasks.Add(_cacheHelper.GetOrAddToCacheAsync($"stats_csgo_{item.player_id}", () => client.GetFromJsonAsync<AnalyzerPlayerStatsForCsgo.Rootobject>($"v4/players/{item.player_id}/stats/csgo")));
                }
                catch (Exception ex)
                {
                    // Log the error and handle it by adding a default task
                    Console.WriteLine($"Error fetching stats for player {item.player_id}: {ex.Message}");

                    // Add a default task with a fallback value to the task list
                    getPlayerStatsForCsGoTasks.Add(Task.FromResult(new AnalyzerPlayerStatsForCsgo.Rootobject
                    {
                        player_id = item.player_id,
                        game_id = item.game_player_id,
                        lifetime = null,
                        segments = null
                    }));
                }

                try
                {
                    var cs2GameHistory = await client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=cs2&from=120&offset=0&limit=20");

                    if (cs2GameHistory.items.Count() == 0)
                    {
                        throw new Exception();
                    }

                    if (cs2GameHistory != null && cs2GameHistory.items != null)
                    {
                        getPlayerMatchHistoryTasks.Add((item.player_id, Task.FromResult(cs2GameHistory)));
                    }

                }
                catch
                {
                    getPlayerMatchHistoryTasks.Add((item.player_id, _cacheHelper.GetOrAddToCacheAsync($"history_cs2_{item.player_id}", () => client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=csgo&from=120&offset=0&limit=20"))));

                }
            }

            foreach (var item in players.teams.faction2.roster)
            {
                try
                {
                    var cs2stats = await client.GetFromJsonAsync<AnalyzerPlayerStats.Rootobject>($"v4/players/{item.player_id}/stats/cs2");
                    if (cs2stats != null)
                    {
                        getPlayerStatsTasks.Add(Task.FromResult(cs2stats));
                    }
                }
                catch
                {
                    getPlayerStatsTasks.Add(
                        Task.FromResult(new AnalyzerPlayerStats.Rootobject
                        {
                            player_id = item.player_id,
                            lifetime = null,
                            segments = null,
                            game_id = item.game_player_id,
                        })
                    );
                }

                try
                {
                    getPlayerStatsForCsGoTasks.Add(_cacheHelper.GetOrAddToCacheAsync($"stats_csgo_{item.player_id}", () => client.GetFromJsonAsync<AnalyzerPlayerStatsForCsgo.Rootobject>($"v4/players/{item.player_id}/stats/csgo")));
                }
                catch (Exception ex)
                {
                    // Log the error and handle it by adding a default task
                    Console.WriteLine($"Error fetching stats for player {item.player_id}: {ex.Message}");

                    // Add a default task with a fallback value to the task list
                    getPlayerStatsForCsGoTasks.Add(Task.FromResult(new AnalyzerPlayerStatsForCsgo.Rootobject
                    {
                        player_id = item.player_id,
                        game_id = item.game_player_id,
                        lifetime = null,
                        segments = null
                    }));
                }

                try
                {
                    var cs2GameHistory = await client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=cs2&from=120&offset=0&limit=20");

                    if (cs2GameHistory.items.Count() == 0)
                    {
                        throw new Exception();
                    }

                    if (cs2GameHistory != null && cs2GameHistory.items != null)
                    {
                        getPlayerMatchHistoryTasks.Add((item.player_id, Task.FromResult(cs2GameHistory)));
                    }

                }
                catch
                {
                    getPlayerMatchHistoryTasks.Add((item.player_id, _cacheHelper.GetOrAddToCacheAsync($"history_cs2_{item.player_id}", () => client.GetFromJsonAsync<AnalyzerMatchHistory.Rootobject>($"v4/players/{item.player_id}/history?game=csgo&from=120&offset=0&limit=20"))));

                }

            }

            return new GetMatchDetailsResult
            {
                Players = players,
                GetPlayerStatsTasks = getPlayerStatsTasks,
                GetPlayerStatsForCsGoTasks = getPlayerStatsForCsGoTasks,
                GetPlayerMatchHistoryTasks = getPlayerMatchHistoryTasks
            };
        }
    }
}
