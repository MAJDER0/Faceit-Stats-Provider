using Faceit_Stats_Provider.ModelsForAnalyzer;

namespace Faceit_Stats_Provider.Classes
{
    public static class Converters
    {
        public static List<AnalyzerPlayerStats.Rootobject> ConvertCsgoToAnalyzerPlayerStats(List<AnalyzerPlayerStatsForCsgo.Rootobject> csgoStatsList)
        {
            if (csgoStatsList == null || !csgoStatsList.Any()) return new List<AnalyzerPlayerStats.Rootobject>();

            return csgoStatsList
          .Where(csgoStats => csgoStats != null)  // Ensure csgoStats is not null
          .Select(csgoStats => new AnalyzerPlayerStats.Rootobject
          {
              player_id = csgoStats.player_id,
              game_id = csgoStats.game_id,
              lifetime = csgoStats.lifetime != null ? new AnalyzerPlayerStats.Lifetime
              {
                  Wins = csgoStats.lifetime?.Wins,
                  TotalHeadshots = csgoStats.lifetime?.TotalHeadshots,
                  LongestWinStreak = csgoStats.lifetime?.LongestWinStreak,
                  KDRatio = csgoStats.lifetime?.KDRatio,
                  Matches = csgoStats.lifetime?.Matches,
                  RecentResults = csgoStats.lifetime?.RecentResults,
                  AverageHeadshots = csgoStats.lifetime?.AverageHeadshots,
                  AverageKDRatio = csgoStats.lifetime?.AverageKDRatio,
                  WinRate = csgoStats.lifetime?.WinRate,
                  CurrentWinStreak = csgoStats.lifetime?.CurrentWinStreak,
                  ExtensionData = csgoStats.lifetime?.ExtensionData
              } : null,  // Return null for lifetime if csgoStats.lifetime is null

              segments = csgoStats.segments?.Where(segment => segment != null)  // Ensure no null segments
                  .Select(segment => new AnalyzerPlayerStats.Segment
                  {
                      label = segment.label,
                      img_small = segment.img_small,
                      img_regular = segment.img_regular,
                      stats = segment.stats != null ? new AnalyzerPlayerStats.Stats
                      {
                          Kills = segment.stats.Kills,
                          AverageHeadshots = segment.stats.AverageHeadshots,
                          Assists = segment.stats.Assists,
                          AverageKills = segment.stats.AverageKills,
                          HeadshotsperMatch = segment.stats.HeadshotsperMatch,
                          AverageKRRatio = segment.stats.AverageKRRatio,
                          AverageKDRatio = segment.stats.AverageKDRatio,
                          AverageQuadroKills = segment.stats.AverageQuadroKills,
                          Matches = segment.stats.Matches,
                          WinRate = segment.stats.WinRate,
                          Rounds = segment.stats.Rounds,
                          TotalHeadshots = segment.stats.TotalHeadshots,
                          KRRatio = segment.stats.KRRatio,
                          Deaths = segment.stats.Deaths,
                          KDRatio = segment.stats.KDRatio,
                          AverageAssists = segment.stats.AverageAssists,
                          Headshots = segment.stats.Headshots,
                          Wins = segment.stats.Wins,
                          AverageDeaths = segment.stats.AverageDeaths,
                          ExtensionData = segment.stats.ExtensionData
                      } : null,  // Return null for stats if segment.stats is null
                      type = segment.type,
                      mode = segment.mode
                  }).ToArray() ?? Array.Empty<AnalyzerPlayerStats.Segment>()  // Handle null segments array
          }).ToList();

        }


        public static List<AnalyzerPlayerStats.Rootobject> ConvertCombinedToPlayerStats(List<AnalyzerPlayerStatsCombined.Rootobject> combinedStats)
        {
            var playerStats = new List<AnalyzerPlayerStats.Rootobject>();

            foreach (var combined in combinedStats)
            {
                var playerStat = new AnalyzerPlayerStats.Rootobject
                {
                    player_id = combined.player_id,
                    game_id = combined.game_id,
                    lifetime = new AnalyzerPlayerStats.Lifetime
                    {
                        Wins = combined.lifetime.Wins?.Replace(",", "."),
                        TotalHeadshots = combined.lifetime.TotalHeadshots?.Replace(",", "."),
                        LongestWinStreak = combined.lifetime.LongestWinStreak?.Replace(",", "."),
                        KDRatio = combined.lifetime.KDRatio?.Replace(",", "."),
                        Matches = combined.lifetime.Matches?.Replace(",", "."),
                        AverageHeadshots = combined.lifetime.AverageHeadshots?.Replace(",", "."),
                        AverageKDRatio = combined.lifetime.AverageKDRatio?.Replace(",", "."),
                        WinRate = combined.lifetime.WinRate?.Replace(",", "."),
                        ExtensionData = combined.lifetime.ExtensionData
                    },
                    segments = combined.segments.Select(seg => new AnalyzerPlayerStats.Segment
                    {
                        label = seg.label,
                        img_small = seg.img_small,
                        img_regular = seg.img_regular,
                        stats = new AnalyzerPlayerStats.Stats
                        {
                            Kills = seg.stats.Kills?.Replace(",", "."),
                            AverageHeadshots = seg.stats.AverageHeadshots?.Replace(",", "."),
                            Assists = seg.stats.Assists?.Replace(",", "."),
                            AverageKills = seg.stats.AverageKills?.Replace(",", "."),
                            HeadshotsperMatch = seg.stats.HeadshotsperMatch?.Replace(",", "."),
                            AverageKRRatio = seg.stats.AverageKRRatio?.Replace(",", "."),
                            AverageKDRatio = seg.stats.AverageKDRatio?.Replace(",", "."),
                            Matches = seg.stats.Matches?.Replace(",", "."),
                            WinRate = seg.stats.WinRate?.Replace(",", "."),
                            Rounds = seg.stats.Rounds?.Replace(",", "."),
                            TotalHeadshots = seg.stats.TotalHeadshots?.Replace(",", "."),
                            KRRatio = seg.stats.KRRatio?.Replace(",", "."),
                            Deaths = seg.stats.Deaths?.Replace(",", "."),
                            KDRatio = seg.stats.KDRatio?.Replace(",", "."),
                            AverageAssists = seg.stats.AverageAssists?.Replace(",", "."),
                            Headshots = seg.stats.Headshots?.Replace(",", "."),
                            Wins = seg.stats.Wins?.Replace(",", "."),
                            AverageDeaths = seg.stats.AverageDeaths?.Replace(",", "."),
                            ExtensionData = seg.stats.ExtensionData
                        },
                        type = seg.type,
                        mode = seg.mode
                    }).ToArray()
                };

                playerStats.Add(playerStat);
            }

            return playerStats;
        }

        public static AnalyzerPlayerStatsCombined.Segment ConvertToCombinedSegment(AnalyzerPlayerStatsForCsgo.Segment segment)
        {
            return new AnalyzerPlayerStatsCombined.Segment
            {
                label = segment.label,
                img_small = segment.img_small,
                img_regular = segment.img_regular,
                stats = new AnalyzerPlayerStatsCombined.Stats
                {
                    Kills = segment.stats.Kills,
                    AverageHeadshots = segment.stats.AverageHeadshots,
                    Assists = segment.stats.Assists,
                    AverageKills = segment.stats.AverageKills,
                    HeadshotsperMatch = segment.stats.HeadshotsperMatch,
                    AverageKRRatio = segment.stats.AverageKRRatio,
                    AverageKDRatio = segment.stats.AverageKDRatio,
                    Matches = segment.stats.Matches,
                    WinRate = segment.stats.WinRate,
                    Rounds = segment.stats.Rounds,
                    TotalHeadshots = segment.stats.TotalHeadshots,
                    KRRatio = segment.stats.KRRatio,
                    Deaths = segment.stats.Deaths,
                    KDRatio = segment.stats.KDRatio,
                    AverageAssists = segment.stats.AverageAssists,
                    Headshots = segment.stats.Headshots,
                    Wins = segment.stats.Wins,
                    AverageDeaths = segment.stats.AverageDeaths,
                    ExtensionData = segment.stats.ExtensionData
                },
                type = segment.type,
                mode = segment.mode
            };
        }


        public static AnalyzerPlayerStatsCombined.Segment ConvertToCombinedSegment(AnalyzerPlayerStats.Segment segment)
        {
            return new AnalyzerPlayerStatsCombined.Segment
            {
                label = segment.label,
                img_small = segment.img_small,
                img_regular = segment.img_regular,
                stats = new AnalyzerPlayerStatsCombined.Stats
                {
                    Kills = segment.stats.Kills,
                    AverageHeadshots = segment.stats.AverageHeadshots,
                    Assists = segment.stats.Assists,
                    AverageKills = segment.stats.AverageKills,
                    HeadshotsperMatch = segment.stats.HeadshotsperMatch,
                    AverageKRRatio = segment.stats.AverageKRRatio,
                    AverageKDRatio = segment.stats.AverageKDRatio,
                    Matches = segment.stats.Matches,
                    WinRate = segment.stats.WinRate,
                    Rounds = segment.stats.Rounds,
                    TotalHeadshots = segment.stats.TotalHeadshots,
                    KRRatio = segment.stats.KRRatio,
                    Deaths = segment.stats.Deaths,
                    KDRatio = segment.stats.KDRatio,
                    AverageAssists = segment.stats.AverageAssists,
                    Headshots = segment.stats.Headshots,
                    Wins = segment.stats.Wins,
                    AverageDeaths = segment.stats.AverageDeaths,
                    ExtensionData = segment.stats.ExtensionData
                },
                type = segment.type,
                mode = segment.mode
            };
        }

    }
}
