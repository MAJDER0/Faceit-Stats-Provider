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
using System.Reflection;
using System.Net;
using MatchType = Faceit_Stats_Provider.Models.MatchType;

namespace Faceit_Stats_Provider.Controllers
{
    public class PlayerStatsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly Random _random;
        static string playerid = "";

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
            List<MatchType.Rootobject> matchtype;

            string errorString;

            try
            {
                Stopwatch z = new Stopwatch();
                z.Start();
                if (!_memoryCache.TryGetValue(nickname, out playerinf))
                {
                    playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");
                    _memoryCache.Set(nickname, playerinf, TimeSpan.FromMinutes(5));
                    playerid = playerinf.player_id;
                }


                var matchhistoryTask = client.GetFromJsonAsync<MatchHistory.Rootobject>(
                    $"v4/players/{playerinf.player_id}/history?game=cs2&from=120&offset=0&limit=20");

                var overallplayerstatsTask = client.GetFromJsonAsync<OverallPlayerStats.Rootobject>(
                    $"v4/players/{playerinf.player_id}/stats/cs2");

                var eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                    $"v1/stats/time/users/{playerinf.player_id}/games/cs2?page=0&size=21");

                await Task.WhenAll(matchhistoryTask, overallplayerstatsTask, eloDiffTask);

                matchhistory = matchhistoryTask.Result!;
                overallplayerstats = overallplayerstatsTask.Result!;
                eloDiff = eloDiffTask.Result!;

                var matchstatsCacheKey = $"{nickname}_matchstats";

                int WalkoverCount = 0;

                if (!_memoryCache.TryGetValue(matchstatsCacheKey, out List<MatchStats.Round> cachedMatchStats))
                {
              

                    try
                    {
                        var task = matchhistory.items.Select(async match =>
                        {                           

                            try
                            {
                                // Fetch data from v4/matches/{match.match_id}
                                var matchResponse = await client.GetAsync($"v4/matches/{match.match_id}");
                                matchResponse.EnsureSuccessStatusCode();
                                

                                    var matchData = await matchResponse.Content.ReadFromJsonAsync<MatchType.Rootobject>();
                                    var calculateElo = matchData?.calculate_elo ?? false;

                                    // Fetch data from v4/matches/{match.match_id}/stats
                                    var statsResponse = await client.GetAsync($"v4/matches/{match.match_id}/stats");
                                    statsResponse.EnsureSuccessStatusCode();

                                    var matchStats = await statsResponse.Content.ReadFromJsonAsync<MatchStats.Rootobject>();

                                    // Set the calculate_elo property based on the fetched data
                                    if (matchStats != null)
                                    {
                                        foreach (var round in matchStats.rounds)
                                        {
                                            round.calculate_elo = calculateElo;
                                            round.competition_name = matchData?.competition_name;
                                            round.match_id = matchData?.match_id;
                                        }
                                    }

                                    return matchStats;
                                
                            }

                            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                            {
                                Console.WriteLine($"Match ID {match.match_id} not found. Skipping.");

                                WalkoverCount++;

                                return new MatchStats.Rootobject
                                {
                                    rounds = new MatchStats.Round[1]
                                    {
                                            new MatchStats.Round
                                            {
                                                competition_id ="",
                                                best_of = "Walkover",
                                                game_id = "",
                                                game_mode = "",
                                                match_id = "",
                                                match_round = "",
                                                played = "",
                                                round_stats = null,
                                                teams = null,
                                                elo = ""
                                            }
                                    }
                                };
                            }

                        }).ToList();

                        int offset = 20;

                        // Fetch additional non-Walkover matches
                        if (WalkoverCount > 0)
                        {
                            List<MatchStats.Round> AdditionalMatchesList = new List<MatchStats.Round>();

                            var AdditionalMatches = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                                $"v4/players/{playerinf.player_id}/history?game=cs2&from=120&offset={offset}&limit=10");

                            if (AdditionalMatches != null) {

                                    var AddMatches = AdditionalMatches.items.Select(async match =>
                                    {
                                        try{

                                            // Fetch data from v4/matches/{match.match_id}
                                            var matchResponse = await client.GetAsync($"v4/matches/{match.match_id}");
                                            matchResponse.EnsureSuccessStatusCode();


                                            var matchData = await matchResponse.Content.ReadFromJsonAsync<MatchType.Rootobject>();
                                            var calculateElo = matchData?.calculate_elo ?? false;

                                            // Fetch data from v4/matches/{match.match_id}/stats
                                            var statsResponse = await client.GetAsync($"v4/matches/{match.match_id}/stats");
                                            statsResponse.EnsureSuccessStatusCode();

                                            var matchStats = await statsResponse.Content.ReadFromJsonAsync<MatchStats.Rootobject>();

                                            // Set the calculate_elo property based on the fetched data
                                            if (matchStats != null)
                                            {
                                                foreach (var round in matchStats.rounds)
                                                {
                                                    round.calculate_elo = calculateElo;
                                                    round.competition_name = matchData?.competition_name;
                                                    round.match_id = matchData?.match_id;
                                                }
                                            }

                                            return matchStats;
                                        }

                                        catch { 
                                            return null;
                                        }

                                    }).ToList();

                                var FinalAdditionList = AddMatches.Take(WalkoverCount);

                                task.AddRange(FinalAdditionList);
                            }
                        }

                        // Continue with the successful results
                        var results = await Task.WhenAll(task);
                        matchstats.AddRange(results.Where(x => x is not null).SelectMany(x => x!.rounds));
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
            };

            ViewData["PlayerStats"] = false;

            return View(ConnectionStatus);
        }

        public IActionResult PlayerNotFound()
        {
            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
        }

        public async Task<ActionResult> LoadMoreMatches(string nickname, int offset, string playerID)
        {
            try
            {
                var client = _clientFactory.CreateClient("Faceit");
                var client2 = _clientFactory.CreateClient("FaceitV1");

                if (!_memoryCache.TryGetValue(nickname, out playerid))
                {
                    var x = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");
                    _memoryCache.Set(nickname, x.nickname, TimeSpan.FromMinutes(5));
                    playerid = x.nickname;
                }

                // Calculate the offset based on the page and limit
                var playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");

                // Make an API request to fetch additional match data (MatchHistory)
                var matchhistory = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                    $"v4/players/{playerID}/history?game=cs2&from=120&offset={offset}&limit=10");

                if (matchhistory != null)
                {
                    // Create a list to store MatchStats
                    var matchStatsList = new List<MatchStats.Rootobject>();

                    // Fetch MatchStats for each match in MatchHistory
                    var task = matchhistory.items.Select(async match =>
                    {
                        return await client.GetFromJsonAsync<MatchStats.Rootobject>($"v4/matches/{match.match_id}/stats");
                    }).ToList();

                    var eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                        $"v1/stats/time/users/{playerID}/games/cs2?page=1&size={offset}");

                    foreach (var result in await Task.WhenAll(task))
                    {
                        matchStatsList.Add(result);
                    }

                    var eloDiff = await eloDiffTask;

                    // Create a view model with MatchHistory, MatchStats, and EloDiff
                    var viewModel = new MatchHistoryWithStatsViewModel
                    {
                        Playerinfo = playerinf, // Include playerinf in the view model
                        MatchHistoryItems = matchhistory.items.ToList(),
                        MatchStats = matchStatsList,
                        EloDiff = eloDiff
                    };

                    return PartialView("MatchListPartial", viewModel);
                }
            }
            catch (Exception ex)
            {
                // Handle errors if the API request fails
                Console.WriteLine($"Error fetching more matches: {ex.Message}");
            }

            return PartialView("MatchListPartial", new MatchHistoryWithStatsViewModel());
        }

    }
}