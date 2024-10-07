using Faceit_Stats_Provider.ModelsForAnalyzer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Services
{
    public class GetMatchDetailsResult
    {
        public AnalyzerMatchPlayers.Rootobject Players { get; set; }
        public List<Task<AnalyzerPlayerStats.Rootobject>> GetPlayerStatsTasks { get; set; }
        public List<Task<AnalyzerPlayerStatsForCsgo.Rootobject>> GetPlayerStatsForCsGoTasks { get; set; }
        public List<(string playerId, Task<AnalyzerMatchHistory.Rootobject>)> GetPlayerMatchHistoryTasks { get; set; }
    }
}
