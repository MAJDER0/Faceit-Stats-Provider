using Faceit_Stats_Provider.ModelsForAnalyzer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Interfaces
{
    public interface IPlayerStatistics
    {
        Task<(List<(string playerId, AnalyzerMatchStats.Rootobject)> playerMatchStats, List<AnalyzerPlayerStatsCombined.Rootobject> combinedPlayerStats)> ProcessPlayerStatisticsAsync(
            List<(string playerId, AnalyzerMatchHistory.Rootobject)> playerMatchHistory,
            List<AnalyzerPlayerStats.Rootobject> playerStats,
            List<AnalyzerPlayerStatsForCsgo.Rootobject> playerStatsForCsGo);
    }
}
