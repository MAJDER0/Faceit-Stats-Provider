using Faceit_Stats_Provider.ModelsForAnalyzer;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Interfaces
{
    public interface IOnlyCsGoStats
    {
        Task<AnalyzerPartialViewModel> ProcessOnlyCsGoStatsAsync(OnlyCsGoStatsRequest toggleRequest);
    }
}
