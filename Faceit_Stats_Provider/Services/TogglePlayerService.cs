using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Faceit_Stats_Provider.Classes;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Faceit_Stats_Provider.Services
{
    public class TogglePlayerService : ITogglePlayer
    {
        public async Task<AnalyzerPartialViewModel> ProcessTogglePlayerAsync(ExcludePlayerModel model)
        {
            if (model == null || model.Players == null || model.PlayerStats == null || model.PlayerMatchStats == null)
            {
                throw new ArgumentException("Invalid data.");
            }

            var initialModelCopy = model.InitialModelCopy;
            var players = model.Players;
            var excludedPlayerIds = model.ExcludedPlayers;
            var includeCsGoStats = model.IncludeCsGoStats;
            var csGoStatsOnlyDisplayed = model.CsGoStatsOnlyDisplayed; // Ensure this is respected

            List<AnalyzerPlayerStats.Rootobject> playerStats;

            // Respect CS:GO stats-only mode and inclusion state
            if (includeCsGoStats == false && csGoStatsOnlyDisplayed == true)
            {
                // Use CS:GO stats when in CS:GO stats-only mode
                playerStats = Converters.ConvertCsgoToAnalyzerPlayerStats(model.PlayerStatsForCsGo);
            }
            else if (includeCsGoStats == true && csGoStatsOnlyDisplayed == false)
            {
                // Use combined stats (CS2 + CS:GO) when including CS:GO stats
                playerStats = Converters.ConvertCombinedToPlayerStats(model.PlayerStatsCombinedViewModel);
            }
            else
            {
                // Default to CS2 stats
                playerStats = model.PlayerStats;
            }

            // Filter out excluded players from playerMatchStats
            var playerMatchStats = model.PlayerMatchStats
                .Where(pms => !excludedPlayerIds.Contains(pms.playerId))
                .Select(pms => (pms.playerId, pms.matchStats))
                .ToList();

            if (excludedPlayerIds.Count > 0)
            {
                // Exclude players from the roster
                if (players.teams.faction1?.roster != null)
                {
                    players.teams.faction1.roster = players.teams.faction1.roster
                        .Where(p => !excludedPlayerIds.Contains(p.player_id))
                        .ToArray();
                }

                if (players.teams.faction2?.roster != null)
                {
                    players.teams.faction2.roster = players.teams.faction2.roster
                        .Where(p => !excludedPlayerIds.Contains(p.player_id))
                        .ToArray();
                }

                // Calculate updated statistics after exclusion
                var result = StatsHelper.CalculateNeededStatistics(
                    players.teams.faction1.leader,
                    players.teams.faction2.leader,
                    players.teams.faction1.roster,
                    players.teams.faction2.roster,
                    playerStats,
                    playerMatchStats);

                playerStats = result.Item8.Concat(result.Item9).ToList();
            }
            else
            {
                // When all players are included, ensure CS:GO stats remain displayed if required
                if (includeCsGoStats == false && csGoStatsOnlyDisplayed == true)
                {
                    playerStats = Converters.ConvertCsgoToAnalyzerPlayerStats(model.PlayerStatsForCsGo);
                }
            }

            var viewModel = new AnalyzerViewModel
            {
                RoomId = model.RoomId,
                Players = players,
                PlayerStats = playerStats,
                PlayerMatchStats = playerMatchStats,
                IsIncludedCsGoStats = includeCsGoStats,
                CsGoStatsOnlyDisplayed = csGoStatsOnlyDisplayed,
                InitialModelCopy = initialModelCopy
            };

            var partialViewModel = new AnalyzerPartialViewModel
            {
                ModifiedViewModel = viewModel,
                OriginalViewModel = ModelMapper.ToAnalyzerViewModel(initialModelCopy)
            };

            return partialViewModel;
        }
    }
}
