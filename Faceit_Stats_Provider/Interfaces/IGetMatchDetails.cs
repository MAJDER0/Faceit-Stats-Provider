using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.ModelsForAnalyzer;
using Faceit_Stats_Provider.Services;

namespace Faceit_Stats_Provider.Interfaces
{
    public interface IGetMatchDetails 
    {
        Task<GetMatchDetailsResult> GetMatchDetailsAsync(string RoomID);
    }
}
