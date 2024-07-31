using System.Collections.Generic;

namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class AnalyzerViewModel
    {
        public string RoomId { get; set; }
        public AnalyzerMatchPlayers.Rootobject Players { get; set; }
        public List<AnalyzerPlayerStats.Rootobject> PlayerStats { get; set; }
        public List<(string playerId, AnalyzerMatchStats.Rootobject)> PlayerMatchStats { get; set; }
        public ExcludePlayerModel InitialModelCopy { get; set; }  
    }
}
