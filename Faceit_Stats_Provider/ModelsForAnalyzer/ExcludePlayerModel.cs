using Faceit_Stats_Provider.Controllers;
using Newtonsoft.Json;

namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class ExcludePlayerModel
    {
        public string RoomId { get; set; }
        public string PlayerId { get; set; }
        public AnalyzerMatchPlayers.Rootobject Players { get; set; }
        public List<AnalyzerPlayerStats.Rootobject> PlayerStats { get; set; }

        [JsonProperty("PlayerMatchStats")]
        public List<TransformedPlayerMatchStats> PlayerMatchStats { get; set; }

    }
}
