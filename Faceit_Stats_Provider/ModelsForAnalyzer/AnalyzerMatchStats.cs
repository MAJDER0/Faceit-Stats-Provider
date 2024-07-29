using System.Text.Json.Serialization;

namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class AnalyzerMatchStats
    {
        public class Rootobject
        {
            public Round[] rounds { get; set; }
        }

        public class Round
        {
            public string game_mode { get; set; }
            public string match_id { get; set; }
            public Round_Stats round_stats { get; set; }
            public Team[] teams { get; set; }
        }

        public class Round_Stats
        {
            public string Winner { get; set; }
            public string Map { get; set; }
        }

        public class Team
        {
            public string team_id { get; set; }
            public Player[] players { get; set; }
        }

        public class Player
        {
            public string player_id { get; set; }
            public string nickname { get; set; }
            [JsonConverter(typeof(PlayerStatsConverter))]
            public Player_Stats player_stats { get; set; }
        }

        public class Player_Stats
        {
            [JsonPropertyName("K/D Ratio")]
            public string KDRatio { get; set; }
            [JsonPropertyName("K/R Ratio")]
            public string KRRatio { get; set; }
        }
    }
}
