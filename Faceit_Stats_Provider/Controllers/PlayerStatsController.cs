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
using System.Text.RegularExpressions;
using System.Diagnostics;

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
            List<EloDiff.Root> allhistory;

            string errorString;

            try
            {
                Stopwatch z = new Stopwatch();
                z.Start();
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
                    $"v1/stats/time/users/{playerinf.player_id}/games/csgo?page=0&size=21");

                await Task.WhenAll(matchhistoryTask, overallplayerstatsTask, eloDiffTask);

                matchhistory = matchhistoryTask.Result!;
                overallplayerstats = overallplayerstatsTask.Result!;
                eloDiff = eloDiffTask.Result!;

                allhistory = new List<EloDiff.Root>();

                var pages = (int)Math.Floor(double.Parse(overallplayerstats.lifetime.Matches) / 100);

                var AllLifeTimeMatchesTask = Enumerable.Range(0, pages)
                    .Select(async i =>
                    {
                        var response = await client2.GetFromJsonAsync<List<EloDiff.Root>>(
                            $"v1/stats/time/users/{playerinf.player_id}/games/csgo?page={i}&size=100");

                        lock (allhistory)
                        {
                            allhistory.AddRange(response);
                        }
                    })
                    .ToList();

                await Task.WhenAll(AllLifeTimeMatchesTask);

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

                z.Stop();
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(z.ElapsedMilliseconds);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                errorString = $"Error: {ex.Message}";
                playerinf = null;
                matchhistory = null;
                overallplayerstats = null;
                matchstats = null;
                eloDiff = null;
                allhistory = null;
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
                ErrorMessage = errorString,
                AllHistory = allhistory
            };

            return View(ConnectionStatus);
        }

        public IActionResult PlayerNotFound()
        {
            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
        }
    }
}
