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
            MatchHistory.Rootobject matchhistory;
            List<MatchStats.Round> matchstats = new List<MatchStats.Round>();
            OverallPlayerStats.Rootobject overallplayerstats;

            string errorString;

            try
            {
                playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>
                ($"v4/players?nickname={nickname}");

                matchhistory = await client.GetFromJsonAsync<MatchHistory.Rootobject>
                ($"v4/players/{playerinf.player_id}/history?game=csgo&offset=0&limit=20");

                overallplayerstats = await client.GetFromJsonAsync<OverallPlayerStats.Rootobject>
                ($"v4/players/{playerinf.player_id}/stats/csgo");

                for (int i = 0; i < matchhistory.items.Count(); i++)
                {
                    var match = await client.GetFromJsonAsync<MatchStats.Rootobject>($"v4/matches/{matchhistory.items[i].match_id}/stats");
                    matchstats.AddRange(match.rounds);
                }

                errorString = null;
            }
            catch (Exception ex)
            {         
                errorString = $"Error: {ex.Message}";
                playerinf = null;
                matchhistory = null;
                overallplayerstats = null;
                matchstats = null;
            }

            var ConnectionStatus = new PlayerStats {OverallPlayerStatsInfo = overallplayerstats, Last20MatchesStats = matchstats, MatchHistory = matchhistory, Playerinfo = playerinf, ErrorMessage = errorString };
            
            return View(ConnectionStatus);
        }
    }
}

