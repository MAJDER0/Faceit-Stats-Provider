using System.Text.Json.Serialization;

namespace Faceit_Stats_Provider.Models
{
    public class MatchStats
    {
        public class Rootobject
        {
            public Round[] rounds { get; set; }
        }

        public class Round
        {
            public string best_of { get; set; }
            public object competition_id { get; set; }
            public string game_id { get; set; }
            public string game_mode { get; set; }
            public string match_id { get; set; }
            public string match_round { get; set; }
            public string played { get; set; }
            public Round_Stats round_stats { get; set; }
            public Team[] teams { get; set; }
        }

        public class Round_Stats
        {
            public string Winner { get; set; }
            public string Score { get; set; }
            public string Map { get; set; }
            public string Region { get; set; }
            public string Rounds { get; set; }
        }

        public class Team
        {
            public string team_id { get; set; }
            public bool premade { get; set; }
            public Team_Stats team_stats { get; set; }
            public Player[] players { get; set; }
        }

        public class Team_Stats
        {
            public string FirstHalfScore { get; set; }
            public string SecondHalfScore { get; set; }
            public string TeamWin { get; set; }
            public string Team { get; set; }
            public string TeamHeadshots { get; set; }
            public string FinalScore { get; set; }
            public string Overtimescore { get; set; }
        }

        public class Player
        {
            public string player_id { get; set; }
            public string nickname { get; set; }
            public Player_Stats player_stats { get; set; }
        }

        public class Player_Stats
        {
            public string Deaths { get; set; }
            public string TripleKills { get; set; }
            public string Headshots { get; set; }
            public string PentaKills { get; set; }
            public string Result { get; set; }
            public string QuadroKills { get; set; }

            [JsonPropertyName("K/R Ratio")]
            public string KRRatio { get; set; }
            [JsonPropertyName("K/D Ratio")]
            public string KDRatio { get; set; }
            public string Assists { get; set; }
            public string MVPs { get; set; }
            public string Kills { get; set; }
        }

    }
}
