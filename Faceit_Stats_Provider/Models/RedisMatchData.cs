using System.Text.Json.Serialization;

namespace Faceit_Stats_Provider.Models
{
    public class RedisMatchData
    {
        public class MatchData
        {
            [JsonPropertyName("elo")]
            public int elo { get; set; }
            public string MatchId { get; set; }
        }

        public List<MatchData> Matches { get; set; }
    }
}
