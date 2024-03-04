using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faceit_Stats_Provider.Models
{
    public class EloDiff
    {

        public class Id
        {
            public string matchId { get; set; }
            public string playerId { get; set; }
        }

        public class Root
        {
            public Id _id { get; set; }
            [JsonPropertyName("gameMode")]
            public string mode { get; set; }

            [JsonPropertyName("elo")]
            public object elo { get; set; }

            [JsonPropertyName("matchId")]
            public string match_Id { get; set; }

            [JsonPropertyName("playerId")]
            public string player_Id { get; set; }
        }
 
    }

}
