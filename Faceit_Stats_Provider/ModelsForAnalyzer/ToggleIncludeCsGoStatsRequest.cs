using Newtonsoft.Json;

namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class ToggleIncludeCsGoStatsRequest
    {
        public string RoomId { get; set; }
        public bool IncludeCsGoStats { get; set; }
        public List<string> ExcludedPlayers { get; set; }
        public List<AnalyzerPlayerStats.Rootobject> PlayerStats { get; set; }
        public List<AnalyzerPlayerStatsCombined.Rootobject> PlayerStatsCombinedViewModel { get; set; }

        [JsonProperty("PlayerMatchStats")]
        public List<TransformedPlayerMatchStats> PlayerMatchStats { get; set; }
        public AnalyzerMatchPlayers.Rootobject Players { get; set; }
        public List<AnalyzerPlayerStatsForCsgo.Rootobject> PlayerStatsForCsGo { get; set; }

        public ExcludePlayerModel InitialModelCopy { get; set; }
    }
}
