using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Faceit_Stats_Provider.Classes;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Services
{
    public class OnlyCsGoStatsService : IOnlyCsGoStats
    {
        public async Task<AnalyzerPartialViewModel> ProcessOnlyCsGoStatsAsync(OnlyCsGoStatsRequest toggleRequest)
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

            var convertCsGoStatsAsPlayerStats = Converters.ConvertCsgoToAnalyzerPlayerStats(toggleRequest.PlayerStatsForCsGo);

            if (toggleRequest.IncludeCsGoStats == false && toggleRequest.CsGoStatsOnlyDisplayed == true)
            {
                viewModel = new AnalyzerViewModel
                {
                    RoomId = toggleRequest.RoomId,
                    Players = toggleRequest.Players,
                    PlayerStats = convertCsGoStatsAsPlayerStats,
                    PlayerStatsForCsGo = toggleRequest.PlayerStatsForCsGo,
                    PlayerStatsCombinedViewModel = toggleRequest.PlayerStatsCombinedViewModel,
                    PlayerMatchStats = toggleRequest.PlayerMatchStats
                          .Select(pms => (pms.playerId, pms.matchStats))
                          .ToList(),
                    InitialModelCopy = toggleRequest.InitialModelCopy,
                    IsIncludedCsGoStats = true,
                    CsGoStatsOnlyDisplayed = true,
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
