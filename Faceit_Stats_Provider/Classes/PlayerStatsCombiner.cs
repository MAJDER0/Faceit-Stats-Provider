using Faceit_Stats_Provider.Models;
using static Faceit_Stats_Provider.Models.OverallPlayerStatsCombined;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace Faceit_Stats_Provider.Classes
{
    public class PlayerStatsCombiner
    {
        public static CombinedPlayerStats CombinePlayerStats(
            OverallPlayerStats.Rootobject cs2stats,
            OverallPlayerStatsCsGo.Rootobject csgostats)
        {
            var combinedStats = new CombinedPlayerStats
            {
                PlayerId = cs2stats?.player_id,
                Lifetime = new CombinedLifetime(),
                Segments = new List<CombinedSegment>()
            };

            // Helper methods for parsing
            double ParseDouble(string input)
            {
                if (string.IsNullOrEmpty(input)) return 0;
                input = input.Replace("%", "").Replace(",", ".");
                if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                {
                    return result;
                }
                return 0;
            }

            int ParseInt(string input)
            {
                if (string.IsNullOrEmpty(input)) return 0;
                if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
                {
                    return result;
                }
                return 0;
            }

            // Initialize totals for weighted averages (only for map segments)
            int totalMatchesForAverages = 0;
            double totalWeightedAverageKills = 0, totalWeightedAverageDeaths = 0, totalWeightedAverageAssists = 0;
            double totalWeightedAverageHeadshots = 0, totalWeightedAverageMVPs = 0;
            double totalWeightedAverageTripleKills = 0, totalWeightedAverageQuadroKills = 0, totalWeightedAveragePentaKills = 0;
            double totalWeightedAverageKDRatio = 0, totalWeightedAverageKRRatio = 0, totalWeightedWinRate = 0;
            double totalWeightedHeadshotsPerMatch = 0;  // Track HeadshotsPerMatch

            // Initialize variables for summing other lifetime stats (if needed)
            int totalWins = 0;
            int totalMatches = 0;
            int totalCurrentWinStreak = 0;
            int totalLongestWinStreak = 0;
            List<string> recentResults = new List<string>();

            // Create dictionaries to hold averages from CS2 and CS:GO for each map
            var mapAverages = new Dictionary<string, List<MapAverage>>();

            // Process CS2 segments and lifetime stats
            if (cs2stats != null)
            {
                int matches = ParseInt(cs2stats.lifetime?.Matches);
                if (matches > 0)
                {
                    double averageKDRatio = ParseDouble(cs2stats.lifetime?.AverageKDRatio);
                    double winRate = ParseDouble(cs2stats.lifetime?.WinRate);

                    totalMatchesForAverages += matches;
                    totalMatches += matches;
                    totalWeightedAverageKDRatio += averageKDRatio * matches;
                    totalWeightedWinRate += winRate * matches;

                    totalWins += ParseInt(cs2stats.lifetime?.Wins);
                    totalCurrentWinStreak += ParseInt(cs2stats.lifetime?.CurrentWinStreak);
                    totalLongestWinStreak = Math.Max(totalLongestWinStreak, ParseInt(cs2stats.lifetime?.LongestWinStreak));

                    if (cs2stats.lifetime?.RecentResults != null)
                        recentResults.AddRange(cs2stats.lifetime.RecentResults);
                }

                if (cs2stats.segments != null)
                {
                    foreach (var s in cs2stats.segments)
                    {
                        if (s.mode != "5v5") continue;

                        var normalizedLabel = NormalizeLabel(s.label);
                        var stats = s.stats;

                        int matchesSegment = ParseInt(stats.Matches);
                        if (matchesSegment == 0) continue;

                        double averageKills = ParseDouble(stats.AverageKills);
                        double averageDeaths = ParseDouble(stats.AverageDeaths);
                        double averageAssists = ParseDouble(stats.AverageAssists);
                        double averageHeadshots = ParseDouble(stats.AverageHeadshots);
                        double headshotsPerMatch = ParseDouble(stats.HeadshotsperMatch);  // Added HeadshotsPerMatch
                        double averageTripleKills = ParseDouble(stats.AverageTripleKills);
                        double averageQuadroKills = ParseDouble(stats.AverageQuadroKills);
                        double averagePentaKills = ParseDouble(stats.AveragePentaKills);
                        double averageKDRatio = ParseDouble(stats.AverageKDRatio);
                        double averageKRRatio = ParseDouble(stats.AverageKRRatio);
                        double winRate = ParseDouble(stats.WinRate);

                        if (!mapAverages.ContainsKey(normalizedLabel))
                        {
                            mapAverages[normalizedLabel] = new List<MapAverage>();
                        }

                        mapAverages[normalizedLabel].Add(new MapAverage
                        {
                            GameType = "CS2",
                            Matches = matchesSegment,
                            AverageKills = averageKills,
                            AverageDeaths = averageDeaths,
                            AverageAssists = averageAssists,
                            AverageHeadshots = averageHeadshots,
                            HeadshotsPerMatch = headshotsPerMatch,  // Included HeadshotsPerMatch
                            AverageTripleKills = averageTripleKills,
                            AverageQuadroKills = averageQuadroKills,
                            AveragePentaKills = averagePentaKills,
                            AverageKDRatio = averageKDRatio,
                            AverageKRRatio = averageKRRatio,
                            WinRate = winRate,
                            ImgSmall = s.img_small,
                            ImgRegular = s.img_regular,
                            Type = s.type,
                            Mode = s.mode
                        });
                    }
                }
            }

            // Similar processing for CS:GO
            if (csgostats != null)
            {
                int matches = ParseInt(csgostats.lifetime?.Matches);
                if (matches > 0)
                {
                    double averageHeadshots = ParseDouble(csgostats.lifetime?.AverageHeadshots);
                    double averageKDRatio = ParseDouble(csgostats.lifetime?.AverageKDRatio);
                    double winRate = ParseDouble(csgostats.lifetime?.WinRate);

                    totalMatchesForAverages += matches;
                    totalMatches += matches;
                    totalWeightedAverageHeadshots += averageHeadshots * matches;
                    totalWeightedAverageKDRatio += averageKDRatio * matches;
                    totalWeightedWinRate += winRate * matches;

                    totalWins += ParseInt(csgostats.lifetime?.Wins);
                    totalCurrentWinStreak += ParseInt(csgostats.lifetime?.CurrentWinStreak);
                    totalLongestWinStreak = Math.Max(totalLongestWinStreak, ParseInt(csgostats.lifetime?.LongestWinStreak));

                    if (csgostats.lifetime?.RecentResults != null)
                        recentResults.AddRange(csgostats.lifetime.RecentResults);
                }

                if (csgostats.segments != null)
                {
                    foreach (var s in csgostats.segments)
                    {
                        if (s.mode != "5v5") continue;

                        var normalizedLabel = NormalizeLabel(s.label);
                        var stats = s.stats;

                        int matchesSegment = ParseInt(stats.Matches);
                        if (matchesSegment == 0) continue;

                        double averageKills = ParseDouble(stats.AverageKills);
                        double averageDeaths = ParseDouble(stats.AverageDeaths);
                        double averageAssists = ParseDouble(stats.AverageAssists);
                        double averageHeadshots = ParseDouble(stats.AverageHeadshots);
                        double headshotsPerMatch = ParseDouble(stats.HeadshotsperMatch);  // Included HeadshotsPerMatch
                        double averageTripleKills = ParseDouble(stats.AverageTripleKills);
                        double averageQuadroKills = ParseDouble(stats.AverageQuadroKills);
                        double averagePentaKills = ParseDouble(stats.AveragePentaKills);
                        double averageKDRatio = ParseDouble(stats.AverageKDRatio);
                        double averageKRRatio = ParseDouble(stats.AverageKRRatio);
                        double winRate = ParseDouble(stats.WinRate);

                        if (!mapAverages.ContainsKey(normalizedLabel))
                        {
                            mapAverages[normalizedLabel] = new List<MapAverage>();
                        }

                        mapAverages[normalizedLabel].Add(new MapAverage
                        {
                            GameType = "CSGO",
                            Matches = matchesSegment,
                            AverageKills = averageKills,
                            AverageDeaths = averageDeaths,
                            AverageAssists = averageAssists,
                            AverageHeadshots = averageHeadshots,
                            HeadshotsPerMatch = headshotsPerMatch,  // Included HeadshotsPerMatch
                            AverageTripleKills = averageTripleKills,
                            AverageQuadroKills = averageQuadroKills,
                            AveragePentaKills = averagePentaKills,
                            AverageKDRatio = averageKDRatio,
                            AverageKRRatio = averageKRRatio,
                            WinRate = winRate,
                            ImgSmall = s.img_small,
                            ImgRegular = s.img_regular,
                            Type = s.type,
                            Mode = s.mode
                        });
                    }
                }
            }

            // Initialize combinedSegmentList
            var combinedSegmentList = new List<CombinedSegment>();

            // Process combined segments (same as before but now includes HeadshotsPerMatch)
            foreach (var kvp in mapAverages)
            {
                string mapLabel = kvp.Key;
                var averagesList = kvp.Value;

                var firstSegment = averagesList.First();

                var combinedSegment = new CombinedSegment
                {
                    Label = CapitalizeFirstLetter(mapLabel),
                    img_small = firstSegment.ImgSmall,
                    ImgRegular = firstSegment.ImgRegular,
                    Type = firstSegment.Type,
                    Mode = firstSegment.Mode,
                    Stats = new CombinedStats()
                };

                int totalMapMatches = averagesList.Sum(a => a.Matches);

                combinedSegment.Stats.AverageKills = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageKills * a.Matches) / totalMapMatches;
                combinedSegment.Stats.AverageDeaths = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageDeaths * a.Matches) / totalMapMatches;
                combinedSegment.Stats.AverageAssists = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageAssists * a.Matches) / totalMapMatches;
                combinedSegment.Stats.AverageHeadshots = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageHeadshots * a.Matches) / totalMapMatches;
                combinedSegment.Stats.HeadshotsPerMatch = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.HeadshotsPerMatch * a.Matches) / totalMapMatches;  // Added HeadshotsPerMatch
                combinedSegment.Stats.AverageMVPs = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageMVPs * a.Matches) / totalMapMatches;
                combinedSegment.Stats.AverageTripleKills = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageTripleKills * a.Matches) / totalMapMatches;
                combinedSegment.Stats.AverageQuadroKills = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageQuadroKills * a.Matches) / totalMapMatches;
                combinedSegment.Stats.AveragePentaKills = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AveragePentaKills * a.Matches) / totalMapMatches;
                combinedSegment.Stats.AverageKDRatio = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageKDRatio * a.Matches) / totalMapMatches;
                combinedSegment.Stats.AverageKRRatio = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.AverageKRRatio * a.Matches) / totalMapMatches;
                combinedSegment.Stats.WinRate = totalMapMatches == 0 ? 0 : averagesList.Sum(a => a.WinRate * a.Matches) / totalMapMatches;

                combinedSegment.Stats.Matches = totalMapMatches;
                combinedSegment.Stats.KDRatio = combinedSegment.Stats.AverageKDRatio;
                combinedSegment.Stats.KRRatio = combinedSegment.Stats.AverageKRRatio;

                combinedSegmentList.Add(combinedSegment);
            }

            combinedStats.Segments = combinedSegmentList;

            // Lifetime stats calculations (same as before)

            return combinedStats;

            string CapitalizeFirstLetter(string label)
            {
                if (string.IsNullOrEmpty(label)) return label;
                return char.ToUpper(label[0]) + label.Substring(1).ToLower();
            }

            string NormalizeLabel(string label)
            {
                if (label.StartsWith("de_", StringComparison.OrdinalIgnoreCase))
                {
                    label = label.Substring(3);
                }
                return label.ToLower();
            }
        }

        private class MapAverage
        {
            public string GameType { get; set; }
            public int Matches { get; set; }
            public double AverageKills { get; set; }
            public double AverageDeaths { get; set; }
            public double AverageAssists { get; set; }
            public double AverageHeadshots { get; set; }
            public double HeadshotsPerMatch { get; set; }  // Added HeadshotsPerMatch field
            public double AverageMVPs { get; set; }
            public double AverageTripleKills { get; set; }
            public double AverageQuadroKills { get; set; }
            public double AveragePentaKills { get; set; }
            public double AverageKDRatio { get; set; }
            public double AverageKRRatio { get; set; }
            public double WinRate { get; set; }
            public string ImgSmall { get; set; }
            public string ImgRegular { get; set; }
            public string Type { get; set; }
            public string Mode { get; set; }
        }
    }
}
