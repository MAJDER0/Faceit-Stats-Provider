using System.Collections.Generic;

namespace Faceit_Stats_Provider.ModelsForAnalyzer
{

    public class AnalyzerViewModel
    {
        public string RoomId { get; set; }
        public AnalyzerMatchPlayers.Rootobject Players { get; set; }
        public List<AnalyzerPlayerStats.Rootobject> PlayerStats { get; set; }
        public List<AnalyzerPlayerStatsForCsgo.Rootobject> PlayerStatsForCsGo { get; set; }
        public List<(string playerId, AnalyzerMatchStats.Rootobject)> PlayerMatchStats { get; set; }
        public List<AnalyzerPlayerStatsCombined.Rootobject> PlayerStatsCombinedViewModel { get; set; }
        public ExcludePlayerModel InitialModelCopy { get; set; }
        public List<(List<(double, bool, double, string)>, string)> MapStatsForSinglePlayer { get; set; }
        public List<(List<(double, bool, double, string)>, string)> MapStatsForSinglePlayerCopy { get; set; }
        public bool IsIncludedCsGoStats { get; set; }
    }
}
