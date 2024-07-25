using Faceit_Stats_Provider.ModelsForAnalyzer;
using System.Globalization;
using static Faceit_Stats_Provider.ModelsForAnalyzer.AnalyzerMatchStats;

namespace Faceit_Stats_Provider.Classes
{
    public static class StatsHelper
    {
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
