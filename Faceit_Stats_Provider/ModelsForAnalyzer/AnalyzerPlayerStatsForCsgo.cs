using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class AnalyzerPlayerStatsForCsgo
    {

        public class Rootobject
        {
            public string player_id { get; set; }
            public string game_id { get; set; }
            [JsonConverter(typeof(LifetimeConverter))]
            public Lifetime lifetime { get; set; }
            public Segment[] segments { get; set; }
        }

        public class Lifetime
        {
            public string Wins { get; set; }
            public string TotalHeadshots { get; set; }
            public string LongestWinStreak { get; set; }
            public string KDRatio { get; set; }
            public string Matches { get; set; }
            public string[] RecentResults { get; set; }
            public string AverageHeadshots { get; set; }
            public string AverageKDRatio { get; set; }
            [JsonPropertyName("Win Rate %")]
            public string WinRate { get; set; }
            public string CurrentWinStreak { get; set; }

            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData { get; set; }
        }

        public class Segment
        {
            public string label { get; set; }
            [JsonPropertyName("img_small")]
            public string img_small { get; set; }
            [JsonPropertyName("img_regular")]
            public string img_regular { get; set; }
            [JsonConverter(typeof(StatsConverter))]
            public Stats stats { get; set; }
            public string type { get; set; }
            public string mode { get; set; }
        }

        public class Stats
        {
            public string Kills { get; set; }
            [JsonPropertyName("Average Headshots %")]
            public string AverageHeadshots { get; set; }
            public string Assists { get; set; }
            [JsonPropertyName("Average Kills")]
            public string AverageKills { get; set; }
            [JsonPropertyName("Headshots per Match")]
            public string HeadshotsperMatch { get; set; }
            [JsonPropertyName("Average K/R Ratio")]
            public string AverageKRRatio { get; set; }
            [JsonPropertyName("Average Quadro Kills")]
            public string AverageQuadroKills { get; set; }
            public string Matches { get; set; }
            [JsonPropertyName("Win Rate %")]
            public string WinRate { get; set; }
            public string Rounds { get; set; }
            public string TotalHeadshots { get; set; }
            public string KRRatio { get; set; }
            public string Deaths { get; set; }
            public string KDRatio { get; set; }
            [JsonPropertyName("Average Assists")]
            public string AverageAssists { get; set; }
            public string Headshots { get; set; }
            public string Wins { get; set; }
            [JsonPropertyName("Average Deaths")]
            public string AverageDeaths { get; set; }
            [JsonPropertyName("Average K/D Ratio")]
            public string AverageKDRatio { get; set; }

            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData { get; set; }
        }

    }
}
