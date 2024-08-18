using Faceit_Stats_Provider.ModelsForAnalyzer;
using System;
using System.Globalization;
using System.Reflection;
using static Faceit_Stats_Provider.Classes.StatsHelper;

namespace Faceit_Stats_Provider.Classes
{
    public class GenerateStats
    {


        public static (double, double, double, double, double, double, double, double, List<(double, bool, double, string)>) OverallSection(List<AnalyzerPlayerStats.Rootobject> playerStats, List<string> playerIds, List<(string, double)> MapScores, List<(List<(double, bool, double, string)>, string)> MapStatsForSinglePlayer, Dictionary<string, List<string>> TeamMapStatsMatches, List<string> maps, bool SecondTeam, List<(string playerId, AnalyzerMatchStats.Rootobject)> PlayerMatchStats)
        {

            double KdCombined = 0.00;
            double KrCombined = 0.00;
            double WrCombined = 0.00;

            int totalMatchesCombined = 0;

            List<(double, bool, double, string)> mapAverageKDsResult = new List<(double, bool, double, string)>();


            int TotalRounds = 0;
            int TotalKills = 0;
            int TotalDeaths = 0;
            int TotalWins = 0;
            int TotalMatches = 0;

            foreach (var playerStat in playerStats)
            {
                if (playerStat?.segments == null) continue;

                var properMaps = playerStat.segments
                    .Where(p => maps.Any(map => map.Equals(p.label.Replace("de_", ""), StringComparison.OrdinalIgnoreCase)) && p.mode == "5v5")
                    .ToList();

                foreach (var map in properMaps)
                {
                    if (map?.stats == null) continue;

                    TotalRounds += int.TryParse(map.stats.Rounds, out var rounds) ? rounds : 0;
                    TotalKills += int.TryParse(map.stats.Kills, out var kills) ? kills : 0;
                    TotalDeaths += int.TryParse(map.stats.Deaths, out var deaths) ? deaths : 0;
                    TotalMatches += int.TryParse(map.stats.Matches, out var matches) ? matches : 0;
                    TotalWins += int.TryParse(map.stats.Wins, out var wins) ? wins : 0;
                }
            }

            KdCombined = (double.Parse(TotalKills.ToString().Replace(",", "."), CultureInfo.InvariantCulture) / double.Parse(TotalDeaths.ToString().Replace(",", "."), CultureInfo.InvariantCulture));
               KrCombined = (double.Parse(TotalKills.ToString().Replace(",", "."), CultureInfo.InvariantCulture) / double.Parse(TotalRounds.ToString().Replace(",", "."), CultureInfo.InvariantCulture));
               WrCombined = (double.Parse(TotalWins.ToString().Replace(",", "."), CultureInfo.InvariantCulture) / double.Parse(TotalMatches.ToString().Replace(",", "."), CultureInfo.InvariantCulture))*100;

            foreach (var mapScore in MapScores)
            {
                totalMatchesCombined += CalculateTotalMatches(TeamMapStatsMatches[mapScore.Item1]);
                mapAverageKDsResult = CalculateMapAverageKD(PlayerMatchStats, mapScore.Item1, playerIds);
                MapStatsForSinglePlayer.Add((mapAverageKDsResult, mapScore.Item1.ToString()));
            }


            if (SecondTeam == true)
            {
                MapStatsForSinglePlayer = MapStatsForSinglePlayer.Skip(7).ToList();
            }

            var last20totalMatchesCombined = MapStatsForSinglePlayer.SelectMany(p => p.Item1).Count();
            var last20KdCombined = MapStatsForSinglePlayer.SelectMany(p => p.Item1).Sum(k => k.Item1);
            var last20KrCombined = MapStatsForSinglePlayer.SelectMany(p => p.Item1).Sum(k => k.Item3);

            double last20WrCombined = MapStatsForSinglePlayer.SelectMany(p => p.Item1).Count(k => k.Item2 == true);

            last20KdCombined = last20KdCombined / last20totalMatchesCombined;
            last20KrCombined = last20KrCombined / last20totalMatchesCombined;
            double last20WrCombinedResult = (last20WrCombined / last20totalMatchesCombined) * 100;



            return (totalMatchesCombined, WrCombined, KdCombined, KrCombined, last20totalMatchesCombined, last20WrCombinedResult, last20KdCombined, last20KrCombined, mapAverageKDsResult);

        }


        public static (double, double, double, int) Last20PlayerSection(
     AnalyzerPlayerStats.Rootobject playerStat,
     List<string> maps,
     List<(List<(double, bool, double, string)>, string)> MapStatsForSinglePlayer,
     AnalyzerMatchPlayers.Roster player)
        {
            if (playerStat?.segments == null)
            {
                return (0.0, 0.0, 0.0, 0); // Return default values if segments are null
            }

            var displayedMapsLast20 = playerStat.segments
            .Where(map => map.mode == "5v5" && maps.Contains(map.label?.Replace("de_", "").ToUpper()))
            .OrderByDescending(x => x.label)
            .ToList();


            var localMapStatsForSinglePlayer = MapStatsForSinglePlayer.ToList();

            var mapData = localMapStatsForSinglePlayer
                .Where(m => displayedMapsLast20.Any(dm => string.Equals(dm.label, m.Item2, StringComparison.OrdinalIgnoreCase)) &&
                            m.Item1.Any(item => item.Item4 == player.player_id))
                .Select(m => new
                {
                    m.Item2, // map label
                    PlayerStats = m.Item1.Where(item => item.Item4 == player.player_id).ToList()
                })
                .ToList();

            double PlayerLast20KD = 0.00;
            double PlayerLast20KR = 0.00;
            double PlayerLast20WR = 0.00;
            int PlayerTotalMatchesLast20 = 0;

            foreach (var map in mapData)
            {
                var Kd = map.PlayerStats.Select(k => k.Item1).Sum();
                var Kr = map.PlayerStats.Select(k => k.Item3).Sum();
                var Wr = map.PlayerStats.Count(k => k.Item2);

                PlayerLast20KD += Kd;
                PlayerLast20KR += Kr;
                PlayerLast20WR += Wr;
                PlayerTotalMatchesLast20 += map.PlayerStats.Count;
            }

            if (PlayerTotalMatchesLast20 > 0)
            {
                PlayerLast20KD /= PlayerTotalMatchesLast20;
                PlayerLast20KR /= PlayerTotalMatchesLast20;
                PlayerLast20WR = (PlayerLast20WR / PlayerTotalMatchesLast20) * 100;
            }
            else
            {
                PlayerLast20KD = 0;
                PlayerLast20KR = 0;
                PlayerLast20WR = 0;
            }

            return (PlayerLast20KD, PlayerLast20WR, PlayerLast20KR, PlayerTotalMatchesLast20);
        }
    }
}