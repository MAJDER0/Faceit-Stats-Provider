using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Faceit_Stats_Provider.Models
{
    public class OverallPlayerStats
    {

        public class Rootobject
        {
            public string player_id { get; set; }
            public string game_id { get; set; }
            public Lifetime lifetime { get; set; }
            public Segment[] segments { get; set; }
        }

        public class Lifetime
        {
            [JsonPropertyName("Average Headshots %")]
            public string AverageHeadshots { get; set; }

            [JsonPropertyName("Average K/D Ratio")]
            public string AverageKDRatio { get; set; }

            [JsonPropertyName("Current Win Streak")]
            public string CurrentWinStreak { get; set; }

            [JsonPropertyName("Longest Win Streak")]
            public string LongestWinStreak { get; set; }

            public string Matches { get; set; }
            [JsonPropertyName("Total Headshots %")]
            public string TotalHeadshots { get; set; }

            public string Wins { get; set; }

            public string KDRatio { get; set; }

            [JsonPropertyName("Recent Results")]
            public string[] RecentResults { get; set; }

            [JsonPropertyName("Win Rate %")]
            public string WinRate { get; set; }
        }

        public class Segment
        {
            public string label { get; set; }
            [JsonPropertyName("img_small")]
            public string img_small { get; set; }
            [JsonPropertyName("img_regular")]
            public string img_regular { get; set; }
            public Stats stats { get; set; }
            public string type { get; set; }
            public string mode { get; set; }
        }

        public class Stats
        {
            public string Kills { get; set; }
            public string QuadroKills { get; set; }
            public string TripleKills { get; set; }
            public string MVPs { get; set; }
            public string AverageTripleKills { get; set; }
            public string PentaKills { get; set; }
            public string AverageMVPs { get; set; }
            public string AverageHeadshots { get; set; }
            public string Assists { get; set; }
            public string AverageKills { get; set; }
            public string HeadshotsperMatch { get; set; }
            public string AverageKRRatio { get; set; }
            public string AverageQuadroKills { get; set; }
            public string Matches { get; set; }
            [JsonPropertyName("Win Rate %")]           
            public string WinRate { get; set; }
            public string Rounds { get; set; }
            public string TotalHeadshots { get; set; }
            [JsonPropertyName("Average K/R Ratio")]
            public string KRRatio { get; set; }
            public string Deaths { get; set; }
            [JsonPropertyName("Average K/D Ratio")]
            public string KDRatio { get; set; }
            public string AverageAssists { get; set; }
            public string AveragePentaKills { get; set; }
            public string Headshots { get; set; }
            public string Wins { get; set; }
            public string AverageDeaths { get; set; }
            public string AverageKDRatio { get; set; }
        }

    }
}
