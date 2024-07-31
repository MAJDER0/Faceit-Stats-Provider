using Faceit_Stats_Provider.ModelsForAnalyzer;

namespace Faceit_Stats_Provider.Classes
{

    public static class ModelMapper
    {
        public static ExcludePlayerModel ToExcludePlayerModel(AnalyzerViewModel viewModel)
        {
            return new ExcludePlayerModel
            {
                RoomId = viewModel.RoomId,
                ExcludedPlayers = new List<string>(), // Initialize with an empty list or use actual excluded players if available
                Players = viewModel.Players,
                PlayerStats = viewModel.PlayerStats,
                PlayerMatchStats = viewModel.PlayerMatchStats.Select(pms => new TransformedPlayerMatchStats
                {
                    playerId = pms.playerId,
                    matchStats = pms.Item2
                }).ToList(),
                InitialModelCopy = null // This will be set later as needed
            };
        }

        public static AnalyzerViewModel ToAnalyzerViewModel(ExcludePlayerModel model)
        {
            return new AnalyzerViewModel
            {
                RoomId = model.RoomId,
                Players = model.Players,
                PlayerStats = model.PlayerStats,
                PlayerMatchStats = model.PlayerMatchStats.Select(pms => (pms.playerId, pms.matchStats)).ToList(),
                InitialModelCopy = model.InitialModelCopy // This is already of type ExcludePlayerModel
            };
        }
    }

}
