using Faceit_Stats_Provider.ModelsForAnalyzer;
using System.Globalization;
using static Faceit_Stats_Provider.ModelsForAnalyzer.AnalyzerMatchStats;
using static Faceit_Stats_Provider.Classes.ScoreFormula;

namespace Faceit_Stats_Provider.Classes
{
    public static class StatsHelper
    {




        public static (List<string>, List<string>, List<string>, string?, string?, AnalyzerMatchPlayers.Roster[], AnalyzerMatchPlayers.Roster[], List<AnalyzerPlayerStats.Rootobject>,
           List<AnalyzerPlayerStats.Rootobject>, Dictionary<string, List<string>>, Dictionary<string, List<string>>,
           Dictionary<string, List<string>>, Dictionary<string, List<string>>, Dictionary<string, List<string>>, Dictionary<string, List<string>>, Dictionary<string, List<string>>, Dictionary<string, List<string>>, List<(string, double)>, List<(string, double)>, List<(double, bool, double, string)>, List<(double, bool, double, string)>, Dictionary<string, string>, List<(List<(double, bool, double, string)>, string)>) CalculateNeededStatistics(
           string? faction1Leader, string? faction2Leader, AnalyzerMatchPlayers.Roster[] faction1Players, AnalyzerMatchPlayers.Roster[] faction2Players, List<AnalyzerPlayerStats.Rootobject> playerStats, List<(string playerId, AnalyzerMatchStats.Rootobject)> playerMatchStats, string? excludePlayerId = null)
        {
            List<AnalyzerMatchPlayers.Roster> filteredFaction1Players = faction1Players.Where(p => p.player_id != excludePlayerId).ToList();
            List<AnalyzerMatchPlayers.Roster> filteredFaction2Players = faction2Players.Where(p => p.player_id != excludePlayerId).ToList();

            var faction1PlayerStats = playerStats.Where(ps => filteredFaction1Players.Any(p => p.player_id == ps.player_id)).ToList();
            var faction2PlayerStats = playerStats.Where(ps => filteredFaction2Players.Any(p => p.player_id == ps.player_id)).ToList();

            List<string> maps = new List<string> { "DUST2", "MIRAGE", "INFERNO", "NUKE", "VERTIGO", "ANCIENT", "ANUBIS" };

            var faction1MapStatsKD = maps.ToDictionary(map => map, map => new List<string>());
            var faction1MapStatsWR = maps.ToDictionary(map => map, map => new List<string>());
            var faction1MapStatsKR = maps.ToDictionary(map => map, map => new List<string>());
            var faction1MapStatsMatches = maps.ToDictionary(map => map, map => new List<string>());

            var faction2MapStatsKD = maps.ToDictionary(map => map, map => new List<string>());
            var faction2MapStatsWR = maps.ToDictionary(map => map, map => new List<string>());
            var faction2MapStatsKR = maps.ToDictionary(map => map, map => new List<string>());
            var faction2MapStatsMatches = maps.ToDictionary(map => map, map => new List<string>());

            var faction1PlayerIds = filteredFaction1Players.Select(p => p.player_id).ToList();
            var faction2PlayerIds = filteredFaction2Players.Select(p => p.player_id).ToList();

            foreach (var playerStat in faction1PlayerStats)
            {
                foreach (var segment in playerStat.segments.Where(s => maps.Contains(s.label.ToUpper())))
                {
                    faction1MapStatsKD[segment.label.ToUpper()].Add(segment.stats.AverageKDRatio);
                    faction1MapStatsWR[segment.label.ToUpper()].Add(segment.stats.WinRate);
                    faction1MapStatsKR[segment.label.ToUpper()].Add(segment.stats.AverageKRRatio);
                    faction1MapStatsMatches[segment.label.ToUpper()].Add(segment.stats.Matches);
                }
            }

            foreach (var playerStat in faction2PlayerStats)
            {
                foreach (var segment in playerStat.segments.Where(s => maps.Contains(s.label.ToUpper())))
                {
                    faction2MapStatsKD[segment.label.ToUpper()].Add(segment.stats.AverageKDRatio);
                    faction2MapStatsWR[segment.label.ToUpper()].Add(segment.stats.WinRate);
                    faction2MapStatsKR[segment.label.ToUpper()].Add(segment.stats.AverageKRRatio);
                    faction2MapStatsMatches[segment.label.ToUpper()].Add(segment.stats.Matches);
                }
            }

            List<(string, double)> mapScoresFaction1 = new List<(string, double)>();
            List<(string, double)> mapScoresFaction2 = new List<(string, double)>();

            var mapName = "";
            var mapNameSecond = "";
            double averageKD = 0, averageWR = 0, averageKR = 0, totalMatches = 0, AvgKd = 0, winRatio = 0, AvgKr = 0;
            double averageKDSecond = 0, averageWRSecond = 0, averageKRSecond = 0, totalMatchesSecond = 0, AvgKdSecond = 0, winRatioSecond = 0, AvgKrSecond = 0;

            List<(double, bool, double, string)> mapAverageKDs = new List<(double, bool, double, string)>();
            List<(double, bool, double, string)> mapAverageKDsSecond = new List<(double, bool, double, string)>();

            foreach (var map in maps)
            {
                mapName = CapitalizeFirstLetter(map.ToLower());
                averageKD = CalculateAverage(faction1MapStatsKD[map]);
                averageWR = CalculateAverage(faction1MapStatsWR[map]);
                averageKR = CalculateAverage(faction1MapStatsKR[map]);
                totalMatches = CalculateTotalMatches(faction1MapStatsMatches[map]);

                mapAverageKDs = CalculateMapAverageKD(playerMatchStats, map, faction1PlayerIds);
                AvgKd = CalculateAverage(mapAverageKDs.Select(kd => new List<double> { kd.Item1 }).ToList());
                winRatio = CalculateWinRatio(mapAverageKDs.Select(kd => new List<bool> { kd.Item2 }).ToList());
                AvgKr = CalculateAverage(mapAverageKDs.Select(kr => new List<double> { kr.Item3 }).ToList());

                var CurrenMapScore = Score(map, averageKD.ToString(), totalMatches.ToString(), averageWR.ToString(), averageKR.ToString(), AvgKd.ToString(), winRatio.ToString(), mapAverageKDs.Count().ToString(), AvgKr.ToString());

                mapScoresFaction1.Add((map, CurrenMapScore));

                mapNameSecond = CapitalizeFirstLetter(map.ToLower());
                averageKDSecond = CalculateAverage(faction2MapStatsKD[map]);
                averageWRSecond = CalculateAverage(faction2MapStatsWR[map]);
                averageKRSecond = CalculateAverage(faction2MapStatsKR[map]);
                totalMatchesSecond = CalculateTotalMatches(faction2MapStatsMatches[map]);

                mapAverageKDsSecond = CalculateMapAverageKD(playerMatchStats, map, faction2PlayerIds);
                AvgKdSecond = CalculateAverage(mapAverageKDsSecond.Select(kd => new List<double> { kd.Item1 }).ToList());
                winRatioSecond = CalculateWinRatio(mapAverageKDsSecond.Select(kd => new List<bool> { kd.Item2 }).ToList());
                AvgKrSecond = CalculateAverage(mapAverageKDsSecond.Select(kr => new List<double> { kr.Item3 }).ToList());

                var CurrenMapScoreSecond = Score(map, averageKDSecond.ToString(), totalMatchesSecond.ToString(), averageWRSecond.ToString(), averageKRSecond.ToString(), AvgKdSecond.ToString(), winRatioSecond.ToString(), mapAverageKDsSecond.Count().ToString(), AvgKrSecond.ToString());

                mapScoresFaction2.Add((map, CurrenMapScoreSecond));
            }

            var mapLinks = new Dictionary<string, string>
            {
                { "mirage", "https://distribution.faceit-cdn.net/images/c47710c4-4407-4dbd-ac89-2ef3b20a262e.jpeg" },
                { "vertigo", "https://distribution.faceit-cdn.net/images/a8d0572f-8a89-474a-babc-c2009cdc42f7.jpeg" },
                { "nuke", "https://distribution.faceit-cdn.net/images/faa7775b-f42b-4627-891a-21ee7cc13637.jpeg" },
                { "inferno", "https://distribution.faceit-cdn.net/images/d71cae42-b38c-470d-a548-0c59d6c71fbe.jpeg" },
                { "dust2", "https://distribution.faceit-cdn.net/images/4eafa800-b504-4dd2-afd0-90882c729140.jpeg" },
                { "anubis", "https://distribution.faceit-cdn.net/images/1c2412c7-ae0c-4fa1-ad86-82a3287cb479.jpeg" },
                { "ancient", "https://distribution.faceit-cdn.net/images/6f72ffec-7607-44cf-9c31-09a865fa92f5.jpeg" }
            };

            List<(List<(double, bool, double, string)>, string)> MapStatsForSinglePlayer = new List<(List<(double, bool, double, string)>, string)>();

            return (maps, faction1PlayerIds, faction2PlayerIds, faction1Leader, faction2Leader, filteredFaction1Players.ToArray(), filteredFaction2Players.ToArray(), faction1PlayerStats,
                faction2PlayerStats, faction1MapStatsKD, faction2MapStatsKD, faction1MapStatsKR, faction2MapStatsKR, faction1MapStatsWR, faction2MapStatsWR, faction1MapStatsMatches, faction2MapStatsMatches, mapScoresFaction1, mapScoresFaction2, mapAverageKDs,
                mapAverageKDsSecond, mapLinks, MapStatsForSinglePlayer);
        }







        public static bool IsPlayerInWinningTeam(AnalyzerMatchStats.Round match, AnalyzerMatchStats.Player player)
        {
            foreach (var team in match.teams)
            {
                foreach (var teamPlayer in team.players)
                {
                    if (teamPlayer.player_id == player.player_id && team.team_id == match.round_stats.Winner)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public static double CalculateAverage(List<List<double>> mapAverageKDs)
        {
            if (mapAverageKDs == null || mapAverageKDs.Count == 0)
                return 0;

            int QuantityOfMatches = 0;
            double Avg = 0.00;

            foreach (var playerKD in mapAverageKDs)
            {
                foreach (var kd in playerKD)
                {
                    QuantityOfMatches++;
                    Avg += kd;
                }
            }

            if (QuantityOfMatches > 0)
            {
                Avg = Avg / QuantityOfMatches;
            }

            return Avg;
        }

        public static double CalculateWinRatio(List<List<bool>> mapAverageKDs)
        {
            if (mapAverageKDs == null || mapAverageKDs.Count == 0)
                return 0;

            double wins = 0;
            double QuantityOfMatches = 0;


            foreach (var playerKD in mapAverageKDs)
            {
                foreach (var matchResult in playerKD)
                {
                    QuantityOfMatches++;
                    if (matchResult == true)
                    {
                        wins++;
                    }
                }
            }

            return (wins / QuantityOfMatches) * 100;
        }

        public static double NormalizeFactor(double value, double MinValue, double MaxValue)
        {
            if (MinValue != MaxValue)
            {
                return (value - MinValue) / (MaxValue - MinValue);
            }
            else
            {
                return 0;
            }
        }


        public static double ParseDoubleOrDefault(string value)
        {
            double result;
            if (!double.TryParse(value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                result = 0;
            }
            return result;
        }

        public static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
        }

        public static string CapitalizeOnlyFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static double CalculateAverage(List<string> ratios)
        {
            if (ratios == null || ratios.Count == 0)
                return 0;
            var total = ratios.Select(ratio => double.Parse(ratio.Replace(",", "."), CultureInfo.InvariantCulture)).Sum();
            return total / ratios.Count;
        }

        public static (double avgKD, double avgKR, double winRatio, int totalMatches, string map) CalculatePlayerMapAverage(
        List<(List<(double kd, bool isWinner, double kr, string playerId)>, string map)> mapAverageKDs,
        string playerId,
        string map)
        {

                if (mapAverageKDs == null || !mapAverageKDs.Any())
                {
                    return (0.0, 0.0, 0.0, 0, map);
                }

                // Filter the list based on the map name
                var mapData = mapAverageKDs
                    .Where(m => string.Equals(m.map, map, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(m => m.Item1)  // Flatten the list of player stats
                    .Where(p => string.Equals(p.playerId, playerId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!mapData.Any())
                {
                    return (0.0, 0.0, 0.0, 0, map);
                }

                // Calculate averages for the filtered list
                double totalKD = mapData.Sum(p => p.kd);
                double totalKR = mapData.Sum(p => p.kr);
                int totalMatches = mapData.Count;
                int wins = mapData.Count(p => p.isWinner);

                double avgKD = totalKD / totalMatches;
                double avgKR = totalKR / totalMatches;

                // Calculate win ratio as a percentage
                double winRatio = (double)wins / totalMatches * 100;

                return (avgKD, avgKR, winRatio, totalMatches, map);

        }



        public static int CalculateTotalMatches(List<string> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;
            return matches.Select(match => int.Parse(match)).Sum();
        }

        public static List<(double, bool, double, string)> CalculateMapAverageKD(List<(string playerId, AnalyzerMatchStats.Rootobject)> playerMatchStats, string map, List<string> playerIds)
        {
            var mapAverageKDs = new List<(double, bool, double, string)>();

            foreach (var playerId in playerIds)
            {
                var playerMatches = playerMatchStats
                    .Where(p => p.playerId == playerId)
                    .SelectMany(p => p.Item2.rounds)
                    .Where(r => string.Equals(r.round_stats.Map, $"de_{map.ToLower()}", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(r => r.match_id)
                    .Take(20)
                    .ToList();

                foreach (var match in playerMatches)
                {
                    var player = match.teams.SelectMany(t => t.players).FirstOrDefault(p => p.player_id == playerId);
                    if (player != null && double.TryParse(player.player_stats.KDRatio.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double kd) &&
                    double.TryParse(player.player_stats.KRRatio.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double kr))
                    {
                        // Determine if player was in the winning team
                        var winnerTeamId = match.round_stats.Winner; // Assuming Winner directly gives team_id
                        var isWinner = (winnerTeamId != null && IsPlayerInWinningTeam(match, player));

                        mapAverageKDs.Add((kd, isWinner, kr, player.player_id));
                    }
                }
            }

            return mapAverageKDs;
        }

    }

}