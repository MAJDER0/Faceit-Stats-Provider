using Faceit_Stats_Provider.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
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
using Microsoft.Extensions.FileSystemGlobbing;
using Faceit_Stats_Provider.Classes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using static Faceit_Stats_Provider.Models.PlayerStats;

namespace Faceit_Stats_Provider.Controllers
{
    public class PlayerStatsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly Random _random;
        static string playerid = "";
        private readonly ILogger<ChangeProxyIP> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;



        public PlayerStatsController(ILogger<ChangeProxyIP> logger, IHttpClientFactory clientFactory, IMemoryCache cache, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _random = new Random();
            _clientFactory = clientFactory;
            _memoryCache = cache;
            _logger = logger;
            _configuration = configuration; // Assign IConfiguration
            _redis = redis;
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

            long RedisEloRetrievesCount = 0;
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
                    $"v1/stats/time/users/{playerinf.player_id}/games/cs2?page=0&size=31");

                var isPlayerInRedisDb = new IsPlayerInRedisDb(_configuration, _redis);

                bool isPlayerInRedis = await isPlayerInRedisDb.IsPlayerInRedisAsync(playerinf.player_id);

                int SendDataToRedisLoopCondition = 0;

                int page = 0;

                if (!isPlayerInRedis)
                {

                    RedisEloRetrievesCount = (int)Math.Ceiling((double)int.Parse(overallplayerstatsTask.Result.lifetime.Matches) / 100);
                }

                else
                {

                    SendDataToRedisLoopCondition = (int)(await GetEloRetrievesCountFromRedis(playerinf.player_id) / 100);
                    RedisEloRetrievesCount = (int)Math.Ceiling((double)int.Parse(overallplayerstatsTask.Result.lifetime.Matches) / 100);
                    page = SendDataToRedisLoopCondition;
                }

                var eloDiffTasks = new List<Task<List<RedisMatchData.MatchData>>>();

                var changeProxyIp = new ChangeProxyIP(_logger, _clientFactory);
                HttpClient eloDiffClient = null;

                var eloRetrievesCount = Enumerable.Range(0, (int)RedisEloRetrievesCount);

                ParallelOptions parallelOptions = new()
                {
                    MaxDegreeOfParallelism = 3
                };

                Parallel.ForEach(eloRetrievesCount, parallelOptions, (id, _) =>
                {
                    try
                    {

                        if (page == 0 || page % 30 == 0 || page == SendDataToRedisLoopCondition)
                        {
                            changeProxyIp = new ChangeProxyIP(_logger, _clientFactory);
                            eloDiffClient = changeProxyIp.GetHttpClientWithRandomProxy();

                            if (eloDiffClient != null)
                            {
                                eloDiffClient.BaseAddress = new Uri("https://api.faceit.com/stats/");
                            }
                            else
                            {
                                _logger.LogError("No proxies available.");
                            }
                        }

                        eloDiffTasks.Add(eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                            $"v1/stats/time/users/{playerinf.player_id}/games/csgo?page={page}&size=100"));

                        page++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error getting HttpClient with random proxy: {ex.Message}");
                    }
                });


                // Await all tasks to complete
                await Task.WhenAll(eloDiffTasks);

                var allEloDiffResults = await Task.WhenAll(eloDiffTasks);

                var lala = allEloDiffResults.SelectMany(x => x);
                var saver = new SaveToRedisAsynchronous(_configuration); // Create an instance
                await saver.SavePlayerToRedis(playerid, lala);


                matchhistory = matchhistoryTask.Result!;
                overallplayerstats = overallplayerstatsTask.Result!;
                eloDiff = eloDiffTask.Result!;

                var matchstatsCacheKey = $"{nickname}_matchstats";

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

                                return new MatchStats.Rootobject
                                {
                                    rounds = new MatchStats.Round[1]
                                    {
                                            new MatchStats.Round
                                            {
                                                competition_id ="",
                                                competition_name = match.competition_name,
                                                best_of = "Walkover",
                                                game_id = "",
                                                game_mode = "",
                                                match_id = match.match_id,
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

                        var results = await Task.WhenAll(task);

                        int offset = 20;

                        int WalkoverCount = results.Count(matchStats =>
                            matchStats?.rounds.Any(round => round?.best_of == "Walkover") ?? false);

                        // Fetch additional non-Walkover matches
                        if (WalkoverCount > 0)
                        {
                            List<MatchStats.Round> AdditionalMatchesList = new List<MatchStats.Round>();

                            var AdditionalMatches = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                                $"v4/players/{playerinf.player_id}/history?game=cs2&from=120&offset={offset}&limit=10");

                            if (AdditionalMatches != null)
                            {

                                var AddMatches = AdditionalMatches.items.Select(async match =>
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

                                    catch
                                    {
                                        return null;
                                    }

                                }).ToList();

                                var SecondResults = await Task.WhenAll(AddMatches);

                                var FinalAdditionList = SecondResults
                                    .Where(matchStats => matchStats != null)
                                    .SelectMany(matchStats => matchStats.rounds)
                                    .Where(round => round?.best_of != "Walkover")
                                    .Take(WalkoverCount);

                                // Create a new MatchStats.Rootobject to encapsulate FinalAdditionList
                                var finalAdditionStats = new MatchStats.Rootobject
                                {
                                    rounds = FinalAdditionList.ToArray() // Assuming rounds is an array property in MatchStats.Rootobject
                                };

                                // Add finalAdditionStats to the results list
                                results = results.Concat(new[] { finalAdditionStats }).ToArray();

                            }
                        }


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

            //var redisFetcher = new RedisFetchMaxElo(_configuration);
            //int highestElo = await redisFetcher.GetHighestEloAsync(playerinf.player_id);

            var ConnectionStatus = new PlayerStats
            {
                OverallPlayerStatsInfo = overallplayerstats,
                Last20MatchesStats = matchstats,
                MatchHistory = matchhistory,
                Playerinfo = playerinf,
                EloDiff = eloDiff,
                ErrorMessage = errorString,
                HighestElo = 100,
                RedisEloRetrievesCount = RedisEloRetrievesCount
            };

            ViewData["PlayerStats"] = false;

            return View(ConnectionStatus);
        }

        private async Task<long> GetEloRetrievesCountFromRedis(string playerId)
        {
            var db = _redis.GetDatabase();
            string key = $"user:{playerId}:matches";
            return await db.HashLengthAsync(key);
        }

        public IActionResult PlayerNotFound()
        {
            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
        }

        public async Task<ActionResult> LoadMoreMatches(string nickname, int offset, string playerID, bool isOffsetModificated)
        {
            int limit = 10;
            int page = 1;

            if (offset > 100)
            {
                page = (offset / 100) + 1;

            }

            if (offset % 2 == 0)
            {
                limit = LookForBiggestDivider(offset);
            }


            static int LookForBiggestDivider(int liczba)
            {
                // Znajdź największy dzielnik mniejszy od 15
                for (int dzielnik = Math.Min(liczba / 2, 14); dzielnik >= 2; dzielnik--)
                {
                    if (liczba % dzielnik == 0)
                    {
                        return dzielnik;
                    }
                }

                // Jeśli nie znaleziono, zwróć 2 (najmniejszy parzysty dzielnik)
                return 2;
            }

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
                    $"v4/players/{playerID}/history?game=cs2&from=1200&offset={offset}&limit={limit}");

                if (isOffsetModificated)
                {
                    var AdditionalMatch = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                       $"v4/players/{playerID}/history?game=cs2&from=1200&offset={offset + limit}&limit=1");

                    matchhistory.items = matchhistory.items.Concat(AdditionalMatch.items).ToArray();
                }

                if (matchhistory != null)
                {
                    // Create a list to store MatchStats
                    var matchStatsList = new List<MatchStats.Rootobject>();

                    // Fetch MatchStats for each match in MatchHistory
                    var task = matchhistory.items.Select(async match =>
                    {
                        try
                        {
                            return await client.GetFromJsonAsync<MatchStats.Rootobject>($"v4/matches/{match.match_id}/stats");
                        }

                        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                        {
                            Console.WriteLine($"Match ID {match.match_id} not found. Skipping.");

                            return new MatchStats.Rootobject
                            {
                                rounds = new MatchStats.Round[1]
                                {
                                      new MatchStats.Round
                                        {
                                         competition_id ="",
                                         competition_name = match.competition_name,
                                         best_of = "Walkover",
                                         game_id = "",
                                         game_mode = "",
                                         match_id = match.match_id,
                                         match_round = "",
                                         played = "",
                                         round_stats = null,
                                         teams = null,
                                         elo = ""
                                       }
                                }
                            };
                        }

                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"Error fetching MatchStats for match {match.match_id}: {innerEx.Message}");
                            throw; // Rethrow the exception to terminate the Task.WhenAll operation
                        }
                    }).ToList();

                    var eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                        $"v1/stats/time/users/{playerID}/games/cs2?page={page}&size={limit + 1}");

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