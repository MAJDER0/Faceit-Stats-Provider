using Faceit_Stats_Provider.Models;

namespace Faceit_Stats_Provider.Interfaces
{
    public interface ILoadMoreMatches
    {
        Task<MatchHistoryWithStatsViewModel> LoadMoreMatches(string nickname, int offset, string playerID, bool isOffsetModificated, int QuantityOfEloRetrieves=10, List<EloDiff.Root> currentModel = null, int currentPage = 0);
    }
}
