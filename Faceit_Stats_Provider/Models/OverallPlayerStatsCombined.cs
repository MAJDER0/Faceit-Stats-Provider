namespace Faceit_Stats_Provider.Models
{
    public class OverallPlayerStatsCombined
    {

        public class CombinedPlayerStats
        {
            public string PlayerId { get; set; }
            public CombinedLifetime Lifetime { get; set; }
            public List<CombinedSegment> Segments { get; set; }
        }

        public class CombinedLifetime
        {
            public double AverageHeadshots { get; set; }
            public double AverageKDRatio { get; set; }
            public double AverageKRRatio { get; set; }
            public double AverageKills { get; set; }
            public double AverageDeaths { get; set; }
            public double AverageAssists { get; set; }
            public int CurrentWinStreak { get; set; }
            public int LongestWinStreak { get; set; }
            public int Matches { get; set; }
            public double TotalHeadshots { get; set; }
            public int Wins { get; set; }
            public double KDRatio { get; set; }
            public double KRRatio { get; set; }
            public List<string> RecentResults { get; set; }
            public double WinRate { get; set; }
        }

        public class CombinedSegment
        {
            public string Label { get; set; }
            public string img_small { get; set; }
            public string ImgRegular { get; set; }
            public CombinedStats Stats { get; set; }
            public string Type { get; set; }
            public string Mode { get; set; }
        }

        public class CombinedStats
        {
            public int Kills { get; set; }
            public int QuadroKills { get; set; }
            public int TripleKills { get; set; }
            public int MVPs { get; set; }
            public double AverageTripleKills { get; set; }
            public int PentaKills { get; set; }
            public double AverageMVPs { get; set; }
            public double AverageHeadshots { get; set; }
            public int Assists { get; set; }
            public double AverageKills { get; set; }
            public double HeadshotsPerMatch { get; set; }
            public double AverageKRRatio { get; set; }
            public double AverageQuadroKills { get; set; }
            public int Matches { get; set; }
            public double WinRate { get; set; }
            public int Rounds { get; set; }
            public int TotalHeadshots { get; set; }
            public double KRRatio { get; set; }
            public int Deaths { get; set; }
            public double KDRatio { get; set; }
            public double AverageAssists { get; set; }
            public double AveragePentaKills { get; set; }
            public int Headshots { get; set; }
            public int Wins { get; set; }
            public double AverageDeaths { get; set; }
            public double AverageKDRatio { get; set; }
        }

    }
}
