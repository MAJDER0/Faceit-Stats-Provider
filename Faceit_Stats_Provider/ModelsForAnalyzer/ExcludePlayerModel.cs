using Faceit_Stats_Provider.ModelsForAnalyzer;
using Newtonsoft.Json;

public class ExcludePlayerModel
{
    public string RoomId { get; set; }
    public string PlayerId { get; set; }
    public bool IsChecked { get; set; }  // Add this property
    public AnalyzerMatchPlayers.Rootobject Players { get; set; }
    public List<AnalyzerPlayerStats.Rootobject> PlayerStats { get; set; }

    [JsonProperty("PlayerMatchStats")]
    public List<TransformedPlayerMatchStats> PlayerMatchStats { get; set; }
}