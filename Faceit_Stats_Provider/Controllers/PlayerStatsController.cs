using Faceit_Stats_Provider.Models;
using Microsoft.AspNetCore.Mvc;
using static Faceit_Stats_Provider.Models.PlayerStats;
using static Faceit_Stats_Provider.Models.MatchHistory;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace Faceit_Stats_Provider.Controllers
{
    public class PlayerStatsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        private readonly IMemoryCache _memoryCache;

        public PlayerStatsController(IHttpClientFactory clientFactory, IMemoryCache cache)
        {
            _clientFactory = clientFactory;
            _memoryCache = cache;
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
                if (!_memoryCache.TryGetValue(nickname, out playerinf))
                {
                    playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");
                    _memoryCache.Set(nickname, playerinf, TimeSpan.FromMinutes(4));
                }

                var matchhistoryTask = client.GetFromJsonAsync<MatchHistory.Rootobject>
                ($"v4/players/{playerinf.player_id}/history?game=csgo&from=120&offset=0&limit=20");

                var overallplayerstatsTask = client.GetFromJsonAsync<OverallPlayerStats.Rootobject>
                ($"v4/players/{playerinf.player_id}/stats/csgo");

                await Task.WhenAll(matchhistoryTask, overallplayerstatsTask);


                matchhistory = matchhistoryTask.Result!;
                overallplayerstats = overallplayerstatsTask.Result!;

                var matchstatsCacheKey = $"{nickname}_matchstats";

                if (!_memoryCache.TryGetValue(matchstatsCacheKey, out List<MatchStats.Round> cachedMatchStats))
                {
                    List<Task<MatchStats.Rootobject>> tasks = matchhistory.items.Select(match => client.GetFromJsonAsync<MatchStats.Rootobject>($"v4/matches/{match.match_id}/stats")).ToList();

                    //await Task.WhenAll(tasks);

                    matchstats.AddRange(tasks.SelectMany(task => task.Result.rounds));

                    _memoryCache.Set(matchstatsCacheKey, matchstats, TimeSpan.FromMinutes(4));
                }
                else
                {
                    matchstats = cachedMatchStats;
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

            if (playerinf is null)
            {
                return RedirectToAction("PlayerNotFound");
            }

            var ConnectionStatus = new PlayerStats { OverallPlayerStatsInfo = overallplayerstats, Last20MatchesStats = matchstats, MatchHistory = matchhistory, Playerinfo = playerinf, ErrorMessage = errorString };

            return View(ConnectionStatus);
        }

        public IActionResult PlayerNotFound()
        {
            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
        }
    }
}

