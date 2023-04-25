using Faceit_Stats_Provider.Models;
using Microsoft.AspNetCore.Mvc;
using static Faceit_Stats_Provider.Models.PlayerStats;
using static Faceit_Stats_Provider.Models.MatchHistory;

namespace Faceit_Stats_Provider.Controllers
{
    public class PlayerStatsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public PlayerStatsController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<ActionResult> PlayerStats(string nickname)
        {
            var client = _clientFactory.CreateClient("Faceit");

            PlayerStats.Rootobject playerinf;

            string errorString;

            try
            {
                playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>
                ($"v4/players?nickname={nickname}");

                errorString = null;
            }
            catch (Exception ex)
            {         
                errorString = $"Error: {ex.Message}";
                playerinf = null;
            }

            var ConnectionStatus = new PlayerStats { Playerinfo = playerinf, ErrorMessage = errorString };
            
            return View(ConnectionStatus);
        }
    }
}

