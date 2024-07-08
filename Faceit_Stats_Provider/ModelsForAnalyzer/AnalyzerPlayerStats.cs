namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class AnalyzerPlayerStats
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
            public string Wins { get; set; }
            public string TotalHeadshots { get; set; }
            public string LongestWinStreak { get; set; }
            public string KDRatio { get; set; }
            public string Matches { get; set; }
            public string[] RecentResults { get; set; }
            public string AverageHeadshots { get; set; }
            public string AverageKDRatio { get; set; }
            public string WinRate { get; set; }
            public string CurrentWinStreak { get; set; }
        }

        public class Segment
        {
            public string type { get; set; }
            public string mode { get; set; }
            public string label { get; set; }
            public string img_small { get; set; }
            public string img_regular { get; set; }
            public Stats stats { get; set; }
        }

        public class Stats
        {
            public string AverageKills { get; set; }
            public string AverageKDRatio { get; set; }
            public string Headshots { get; set; }
            public string PentaKills { get; set; }
            public string TotalHeadshots { get; set; }
            public string AverageMVPs { get; set; }
            public string Wins { get; set; }
            public string MVPs { get; set; }
            public string WinRate { get; set; }
            public string AverageHeadshots { get; set; }
            public string Kills { get; set; }
            public string Assists { get; set; }
            public string AverageAssists { get; set; }
            public string Deaths { get; set; }
            public string AverageDeaths { get; set; }
            public string AverageKRRatio { get; set; }
            public string HeadshotsperMatch { get; set; }
            public string AverageTripleKills { get; set; }
            public string TripleKills { get; set; }
            public string Matches { get; set; }
            public string KRRatio { get; set; }
            public string AveragePentaKills { get; set; }
            public string KDRatio { get; set; }
            public string AverageQuadroKills { get; set; }
            public string QuadroKills { get; set; }
            public string Rounds { get; set; }
        }

    }
}
