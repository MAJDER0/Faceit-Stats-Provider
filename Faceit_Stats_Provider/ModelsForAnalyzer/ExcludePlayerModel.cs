using Faceit_Stats_Provider.ModelsForAnalyzer;
using Newtonsoft.Json;

public class ExcludePlayerModel
{
    public string RoomId { get; set; }
    public List<string> ExcludedPlayers { get; set; }
    public AnalyzerMatchPlayers.Rootobject Players { get; set; }

    [JsonProperty("PlayerStats")]
    public List<AnalyzerPlayerStats.Rootobject> PlayerStats { get; set; }

    [JsonProperty("PlayerMatchStats")]
    public List<TransformedPlayerMatchStats> PlayerMatchStats { get; set; }
    public List<AnalyzerPlayerStatsForCsgo.Rootobject> PlayerStatsForCsGo { get; set; }
    public List<AnalyzerPlayerStatsCombined.Rootobject> PlayerStatsCombinedViewModel { get; set; }
    public ExcludePlayerModel InitialModelCopy { get; set; }

    public bool IncludeCsGoStats { get; set; }

    public bool CsGoStatsOnlyDisplayed { get; set; }
}
