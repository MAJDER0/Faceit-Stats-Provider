using Faceit_Stats_Provider.ModelsForAnalyzer;
using System;
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


            foreach (var playerStat in playerStats)
            {

                var KRInfo = playerStat.segments.Where(p => maps.Any(map => map.Equals(p.label, StringComparison.OrdinalIgnoreCase)) && p.mode == "5v5").ToList();
                KrCombined += KRInfo.Sum(map => ParseDoubleOrDefault(map.stats.AverageKRRatio));
                WrCombined += (ParseDoubleOrDefault(playerStat.lifetime.Wins) / ParseDoubleOrDefault(playerStat.lifetime.Matches)) * 100;
                KdCombined += ParseDoubleOrDefault(playerStat.lifetime.AverageKDRatio);
            }

            KrCombined = KrCombined / 7;

            foreach (var mapScore in MapScores)
            {
                totalMatchesCombined += CalculateTotalMatches(TeamMapStatsMatches[mapScore.Item1]);
                mapAverageKDsResult = CalculateMapAverageKD(PlayerMatchStats, mapScore.Item1, playerIds);
                MapStatsForSinglePlayer.Add((mapAverageKDsResult, mapScore.Item1.ToString()));
            }

            KdCombined = KdCombined / 5;
            KrCombined = KrCombined / 5;
            WrCombined = WrCombined / 5;



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


        public static (double, double, double, int) Last20PlayerSection(AnalyzerPlayerStats.Rootobject playerStat, List<string> maps, List<(List<(double, bool, double, string)>, string)> MapStatsForSinglePlayer, AnalyzerMatchPlayers.Roster player)
        {

            var displayedMapsLast20 = playerStat.segments
            .Where(map => map.mode == "5v5" && maps.Contains(map.label.ToUpper()))
            .OrderByDescending(x => x.label)
            .ToList();

            // Make a local copy of MapStatsForSinglePlayer
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

            PlayerLast20KD = PlayerLast20KD / PlayerTotalMatchesLast20;
            PlayerLast20KR = PlayerLast20KR / PlayerTotalMatchesLast20;
            PlayerLast20WR = PlayerLast20WR / PlayerTotalMatchesLast20;

            PlayerLast20WR = PlayerLast20WR * 100;

            return (PlayerLast20KD, PlayerLast20WR, PlayerLast20KR, PlayerTotalMatchesLast20);

        }
    }
}
