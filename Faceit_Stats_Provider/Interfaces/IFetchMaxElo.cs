using Faceit_Stats_Provider.Models;
using Microsoft.AspNetCore.Mvc;

namespace Faceit_Stats_Provider.Interfaces
{
    public interface IFetchMaxElo
    {
        Task<HighestEloDataModel> FetchMaxEloAsync(string playerId);
    }
}
