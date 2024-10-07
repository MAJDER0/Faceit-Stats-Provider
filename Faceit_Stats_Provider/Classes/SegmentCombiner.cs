using Faceit_Stats_Provider.ModelsForAnalyzer;

namespace Faceit_Stats_Provider.Classes
{
    public static class SegmentCombiner
    {

        public static AnalyzerPlayerStatsCombined.Segment CombineSegments(AnalyzerPlayerStatsCombined.Segment cs2Segment, AnalyzerPlayerStatsCombined.Segment csgoSegment)
        {
            int SafeParse(string value)
            {
                return int.TryParse(value, out int result) ? result : 0;
            }

            // Sum the relevant values
            int totalKills = SafeParse(cs2Segment.stats.Kills) + SafeParse(csgoSegment.stats.Kills);
            int totalMatches = SafeParse(cs2Segment.stats.Matches) + SafeParse(csgoSegment.stats.Matches);
            int totalRounds = SafeParse(cs2Segment.stats.Rounds) + SafeParse(csgoSegment.stats.Rounds);
            int totalDeaths = SafeParse(cs2Segment.stats.Deaths) + SafeParse(csgoSegment.stats.Deaths);
            int totalAssists = SafeParse(cs2Segment.stats.Assists) + SafeParse(csgoSegment.stats.Assists);
            int totalHeadshots = SafeParse(cs2Segment.stats.TotalHeadshots) + SafeParse(csgoSegment.stats.TotalHeadshots);
            int totalWins = SafeParse(cs2Segment.stats.Wins) + SafeParse(csgoSegment.stats.Wins);

            // Calculate derived statistics
            double kdratio = totalDeaths != 0 ? totalKills / (double)totalDeaths : 0;
            double krratio = totalRounds != 0 ? totalKills / (double)totalRounds : 0;
            double winRate = totalMatches != 0 ? (totalWins / (double)totalMatches) * 100 : 0;

            return new AnalyzerPlayerStatsCombined.Segment
            {
                label = cs2Segment.label ?? csgoSegment.label,
                img_small = cs2Segment.img_small ?? csgoSegment.img_small,
                img_regular = cs2Segment.img_regular ?? csgoSegment.img_regular,
                stats = new AnalyzerPlayerStatsCombined.Stats
                {
                    Kills = totalKills.ToString(),
                    AverageHeadshots = totalMatches != 0 ? (totalHeadshots / (double)totalMatches).ToString("F2") : "0",
                    Assists = totalAssists.ToString(),
                    AverageKills = totalMatches != 0 ? (totalKills / (double)totalMatches).ToString("F2") : "0",
                    HeadshotsperMatch = totalMatches != 0 ? (totalHeadshots / (double)totalMatches).ToString("F2") : "0",
                    AverageKRRatio = krratio.ToString("F2"),
                    AverageKDRatio = kdratio.ToString("F2"),
                    Matches = totalMatches.ToString(),
                    WinRate = winRate.ToString("F2"),
                    Rounds = totalRounds.ToString(),
                    TotalHeadshots = totalHeadshots.ToString(),
                    KRRatio = krratio.ToString("F2"),
                    Deaths = totalDeaths.ToString(),
                    KDRatio = kdratio.ToString("F2"),
                    AverageAssists = totalMatches != 0 ? (totalAssists / (double)totalMatches).ToString("F2") : "0",
                    Headshots = totalHeadshots.ToString(),
                    Wins = totalWins.ToString(),
                    AverageDeaths = totalMatches != 0 ? (totalDeaths / (double)totalMatches).ToString("F2") : "0",
                    ExtensionData = cs2Segment.stats.ExtensionData ?? csgoSegment.stats.ExtensionData
                },
                type = cs2Segment.type ?? csgoSegment.type,
                mode = cs2Segment.mode ?? csgoSegment.mode
            };
        }

    }
}
