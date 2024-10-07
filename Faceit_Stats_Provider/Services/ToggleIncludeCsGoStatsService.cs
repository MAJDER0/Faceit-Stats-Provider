using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Faceit_Stats_Provider.Classes;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Faceit_Stats_Provider.Services
{
    public class ToggleIncludeCsGoStatsService : IToggleIncludeCsGoStats
    {
        public async Task<AnalyzerPartialViewModel> ProcessToggleIncludeCsGoStatsAsync(ToggleIncludeCsGoStatsRequest toggleRequest)
        {
            if (toggleRequest == null)
            {
                throw new ArgumentNullException(nameof(toggleRequest), "Request is null");
            }

            // Validate the request data
            if (string.IsNullOrEmpty(toggleRequest.RoomId) || toggleRequest.Players == null || toggleRequest.PlayerMatchStats == null)
            {
                throw new ArgumentException("Invalid request data");
            }

            AnalyzerViewModel viewModel;

            if (toggleRequest.IncludeCsGoStats)
            {
                var convertedCombinedStatsToMatchModel = Converters.ConvertCombinedToPlayerStats(toggleRequest.PlayerStatsCombinedViewModel);

                viewModel = new AnalyzerViewModel
                {
                    RoomId = toggleRequest.RoomId,
                    Players = toggleRequest.Players,
                    PlayerStats = convertedCombinedStatsToMatchModel,
                    PlayerStatsForCsGo = toggleRequest.PlayerStatsForCsGo,
                    PlayerStatsCombinedViewModel = toggleRequest.PlayerStatsCombinedViewModel,
                    PlayerMatchStats = toggleRequest.PlayerMatchStats
                        .Select(pms => (pms.playerId, pms.matchStats))
                        .ToList(),
                    InitialModelCopy = toggleRequest.InitialModelCopy,
                    IsIncludedCsGoStats = true,
                    CsGoStatsOnlyDisplayed = false
                };
            }
            else
            {
                var initialModelCopy = toggleRequest.InitialModelCopy;
                viewModel = new AnalyzerViewModel
                {
                    RoomId = initialModelCopy.RoomId,
                    Players = initialModelCopy.Players,
                    PlayerStats = initialModelCopy.PlayerStats,
                    PlayerStatsForCsGo = initialModelCopy.PlayerStatsForCsGo,
                    PlayerStatsCombinedViewModel = initialModelCopy.PlayerStatsCombinedViewModel,
                    PlayerMatchStats = initialModelCopy.PlayerMatchStats
                        .Select(pms => (pms.playerId, pms.matchStats))
                        .ToList(),
                    InitialModelCopy = initialModelCopy.InitialModelCopy,
                    IsIncludedCsGoStats = false,
                    CsGoStatsOnlyDisplayed = false,
                };
            }

            // Apply the excluded players logic
            if (toggleRequest.ExcludedPlayers != null && toggleRequest.ExcludedPlayers.Any())
            {
                viewModel.Players.teams.faction1.roster = viewModel.Players.teams.faction1.roster.Where(p => !toggleRequest.ExcludedPlayers.Contains(p.player_id)).ToArray();
                viewModel.Players.teams.faction2.roster = viewModel.Players.teams.faction2.roster.Where(p => !toggleRequest.ExcludedPlayers.Contains(p.player_id)).ToArray();
                viewModel.PlayerStats = viewModel.PlayerStats.Where(ps => !toggleRequest.ExcludedPlayers.Contains(ps.player_id)).ToList();
                viewModel.PlayerMatchStats = viewModel.PlayerMatchStats.Where(pms => !toggleRequest.ExcludedPlayers.Contains(pms.playerId)).ToList();
            }

            var partialViewModel = new AnalyzerPartialViewModel
            {
                ModifiedViewModel = viewModel,
                OriginalViewModel = ModelMapper.ToAnalyzerViewModel(toggleRequest.InitialModelCopy) // Use mapping function
            };

            return partialViewModel;
        }
    }
}
