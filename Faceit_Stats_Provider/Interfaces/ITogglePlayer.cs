using Faceit_Stats_Provider.ModelsForAnalyzer;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Interfaces
{
    public interface ITogglePlayer
    {
        Task<AnalyzerPartialViewModel> ProcessTogglePlayerAsync(ExcludePlayerModel model);
    }
}
