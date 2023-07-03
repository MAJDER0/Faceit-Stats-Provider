// ...

using Faceit_Stats_Provider.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

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
            var client2 = _clientFactory.CreateClient("FaceitV1");

            PlayerStats.Rootobject playerinf;
            MatchHistory.Rootobject matchhistory;
            List<MatchStats.Round> matchstats = new List<MatchStats.Round>();
            OverallPlayerStats.Rootobject overallplayerstats;
            List<EloDiff.Root> eloDiff;

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

                var eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                    $"v1/stats/time/users/{playerinf.player_id}/games/csgo?page=0&size=20");


                await Task.WhenAll(matchhistoryTask, overallplayerstatsTask, eloDiffTask);

                matchhistory = matchhistoryTask.Result!;
                overallplayerstats = overallplayerstatsTask.Result!;
                eloDiff = eloDiffTask.Result!;


                var matchstatsCacheKey = $"{nickname}_matchstats";

                if (!_memoryCache.TryGetValue(matchstatsCacheKey, out List<MatchStats.Round> cachedMatchStats))
                {
                    List<Task<MatchStats.Rootobject>> tasks = matchhistory.items.Select(match => client.GetFromJsonAsync<MatchStats.Rootobject>($"v4/matches/{match.match_id}/stats")).ToList();

                    await Task.WhenAll(tasks);

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
                eloDiff = null;
            }

            if (playerinf is null)
            {
                return RedirectToAction("PlayerNotFound");
            }

            var ConnectionStatus = new PlayerStats
            {
                OverallPlayerStatsInfo = overallplayerstats,
                Last20MatchesStats = matchstats,
                MatchHistory = matchhistory,
                Playerinfo = playerinf,
                EloDiff = eloDiff,
                ErrorMessage = errorString
            };

            return View(ConnectionStatus);
        }

        public IActionResult PlayerNotFound()
        {
            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
        }
    }
}
