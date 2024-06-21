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
            public object competition_name { get; set; }
            public bool calculate_elo { get; set; }
            public string eloaftergame { get; set; }
            public string game_id { get; set; }
            public string game_mode { get; set; }
            public string match_id { get; set; }
            public string match_round { get; set; }
            public string played { get; set; }
            public Round_Stats round_stats { get; set; }
            public Team[] teams { get; set; }
            public string elo { get; set; }
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
            [JsonPropertyName("Triple Kills")]
            public string TripleKills { get; set; }
            public string Headshots { get; set; }
            [JsonPropertyName("Penta Kills")]
            public string PentaKills { get; set; }
            public string Result { get; set; }
            [JsonPropertyName("Quadro Kills")]
            public string QuadroKills { get; set; }
            [JsonPropertyName("Headshots %")]
            public string HeadshotsPercentage { get; set; }
            [JsonPropertyName("K/R Ratio")]
            public string KRRatio { get; set; }
            [JsonPropertyName("K/D Ratio")]
            public string KDRatio { get; set; }
            public string Assists { get; set; }
            public string MVPs { get; set; }
            public string Kills { get; set; }


            [JsonPropertyName("ADR")]
            public string ADR { get; set; } //

            [JsonPropertyName("1v1Wins")]
            public string v1Wins { get; set; } //

            [JsonPropertyName("1v2Wins")]
            public string v2Wins { get; set; } //

            [JsonPropertyName("Sniper Kills")]
            public string SniperKills { get; set; } //

            [JsonPropertyName("Clutch Kills")]
            public string ClutchKills { get; set; } //

            [JsonPropertyName("Flash Count")]
            public string FlashCount { get; set; }//

            [JsonPropertyName("Utility Damage")]
            public string UtilityDamage { get; set; } //

            [JsonPropertyName("Zeus Kills")]
            public string ZeusKills { get; set; } //
            
            [JsonPropertyName("Pistol Kills")]
            public string PistolKills { get; set; }//

            [JsonPropertyName("Damage")]
            public string Damage { get; set; } //

            [JsonPropertyName("Flash Successes")]
            public string FlashSuccesses { get; set; } // 

            [JsonPropertyName("Entry Count")]
            public string EntryCount { get; set; } //

            [JsonPropertyName("Entry Wins")]
            public string EntryWins { get; set; } //

            [JsonPropertyName("First Kills")]
            public string FirstKills { get; set; } //

            [JsonPropertyName("Double Kills")]
            public string DoubleKills { get; set; }//

            [JsonPropertyName("Enemies Flashed")]
            public string EnemiesFlashed { get; set; }//

            [JsonPropertyName("Knife Kills")]
            public string KnifeKills { get; set; } //
        }

    }
}
