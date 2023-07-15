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
        private readonly Random _random;
        public PlayerStatsController(IHttpClientFactory clientFactory, IMemoryCache cache)
        {
            _random = new Random();
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

                //allhistory = new List<EloDiff.Root>();
                //var allhistoryTask = new List<Task<List<EloDiff.Root>>>();

                //var pages = (int)Math.Ceiling(double.Parse(overallplayerstats.lifetime.Matches) / 100);

                //var matchdiff = $"{nickname}_matchdiff";
                //List<EloDiff.Root>[] arrayOfResults;

                //if (!_memoryCache.TryGetValue(matchdiff, out List<EloDiff.Root> cachedEloDiff))
                //{
                //    List<Task<List<EloDiff.Root>>> AllLifeTimeMatchesTask = Enumerable.Range(0, pages)
                //    .Select(i => client2.GetFromJsonAsync<List<EloDiff.Root>>(string.Format("v1/stats/time/users/{0}/games/csgo?page={1}&size=100", playerinf.player_id, i))).ToList();

                //    arrayOfResults = await Task.WhenAll(AllLifeTimeMatchesTask);

                //    allhistory.AddRange(arrayOfResults.SelectMany(x => x));

                //    _memoryCache.Set(matchdiff, cachedEloDiff, TimeSpan.FromMinutes(10));
                //}
                //else
                //{
                //    allhistory = cachedEloDiff.ToList();
                //}

                var matchstatsCacheKey = $"{nickname}_matchstats";

                if (!_memoryCache.TryGetValue(matchstatsCacheKey, out List<MatchStats.Round> cachedMatchStats))
                {
                    try
                    {
                        var tasks = matchhistory.items.Select(async match =>
                        {
                            return client.GetFromJsonAsync<MatchStats.Rootobject>($"v4/matches/{match.match_id}/stats");
                        }).ToList();
                        try
                        {
                            var results = await Task.WhenAll(tasks);

                            matchstats.AddRange(results.Where(x => x is not null).SelectMany(x => x!.Result!.rounds));

                            _memoryCache.Set(matchstatsCacheKey, matchstats, TimeSpan.FromMinutes(10));
                        }
                        catch (Exception)
                        {

                        }
                    }
                    catch
                    {
                        Console.WriteLine("Blad");
                    }
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
                //AllHistory = allhistory
            };

            ViewData["PlayerStats"] = false;

            return View(ConnectionStatus);
        }

        public IActionResult PlayerNotFound()
        {
            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
        }
    }
}
