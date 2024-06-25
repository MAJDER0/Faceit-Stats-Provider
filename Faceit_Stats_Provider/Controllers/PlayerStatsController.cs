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
using System.Net.Sockets;

namespace Faceit_Stats_Provider.Controllers
{
    public class PlayerStatsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly Random _random;
        static string playerid = "";
        private readonly ILogger<HttpClientManager> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;
        private readonly GetTotalEloRetrievesCountFromRedis _getTotalEloRetrievesCountFromRedis;



        public PlayerStatsController(ILogger<HttpClientManager> logger, IHttpClientFactory clientFactory, IMemoryCache cache, IConfiguration configuration, IConnectionMultiplexer redis, GetTotalEloRetrievesCountFromRedis getTotalEloRetrievesCountFromRedis)
        {
            _random = new Random();
            _clientFactory = clientFactory;
            _memoryCache = cache;
            _logger = logger;
            _configuration = configuration; // Assign IConfiguration
            _redis = redis;
            _getTotalEloRetrievesCountFromRedis = getTotalEloRetrievesCountFromRedis;
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
            string game = "cs2";

            try
            {
                Stopwatch z = new Stopwatch();
                z.Start();
                if (!_memoryCache.TryGetValue(nickname, out playerinf))
                {
                    playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");
                    _memoryCache.Set(nickname, playerinf, TimeSpan.FromMinutes(3));
                    playerid = playerinf.player_id;
                }

                var matchhistoryTask = client.GetFromJsonAsync<MatchHistory.Rootobject>(
                    $"v4/players/{playerinf.player_id}/history?game=cs2&from=120&offset=0&limit=20");


                var MatchhHistoryTaskResult = await matchhistoryTask;

                if (MatchhHistoryTaskResult.items.Count() == 0)
                {
                    matchhistoryTask = client.GetFromJsonAsync<MatchHistory.Rootobject>(
                        $"v4/players/{playerinf.player_id}/history?game=csgo&from=120&offset=0&limit=20");
                    game = "csgo";

                }

                var overallplayerstatsTask = client.GetFromJsonAsync<OverallPlayerStats.Rootobject>(
                    $"v4/players/{playerinf.player_id}/stats/cs2");

                try
                {
                    var overallplayerstatsTaskResult = await overallplayerstatsTask;

                    if (overallplayerstatsTaskResult.segments.Count() == 0) {

                        throw new Exception("No cs2 Matches");
                    }
                }
                catch
                {

                    overallplayerstatsTask = client.GetFromJsonAsync<OverallPlayerStats.Rootobject>(
                       $"v4/players/{playerinf.player_id}/stats/csgo");

                }

                var eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                    $"v1/stats/time/users/{playerinf.player_id}/games/cs2?page=0&size=31");

                var eloDiffTaskResult = await eloDiffTask;

                if (eloDiffTaskResult.Count() == 0)
                {
                    eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                       $"v1/stats/time/users/{playerinf.player_id}/games/csgo?page=0&size=31");
                }

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

                    SendDataToRedisLoopCondition = (int)(await _getTotalEloRetrievesCountFromRedis.GetTotalEloRetrievesCountFromRedisAsync(playerinf.player_id) / 100);
                    RedisEloRetrievesCount = (int)Math.Ceiling((double)int.Parse(overallplayerstatsTask.Result.lifetime.Matches) / 100);
                }
                var eloDiffTasks = new List<Task<List<RedisMatchData.MatchData>>>();

                var httpClientManager = new HttpClientManager(_logger, _clientFactory);
                HttpClient eloDiffClient = null;

                var retrieveCount = (int)RedisEloRetrievesCount - SendDataToRedisLoopCondition;
                var eloRetrievesCount = Enumerable.Range(0, 0);

                if (retrieveCount > 0)
                {
                    eloRetrievesCount = Enumerable.Range(0, retrieveCount);
                }

                ParallelOptions parallelOptions = new()
                {
                    MaxDegreeOfParallelism = 3
                };

                Parallel.ForEach(eloRetrievesCount, parallelOptions, async (id, _) =>
                {
                    try
                    {
                        string cacheKey = $"PlayerData_{playerinf.player_id}_Page_{page}";

                        if (!_memoryCache.TryGetValue(cacheKey, out List<RedisMatchData.MatchData> cachedData))
                        {
                            if (page == 0 || page % 30 == 0 || page == SendDataToRedisLoopCondition)
                            {
                                eloDiffClient = httpClientManager.GetHttpClientWithRandomProxy();
                                if (eloDiffClient != null)
                                {
                                    eloDiffClient.BaseAddress = new Uri("https://api.faceit.com/stats/");
                                }
                                else
                                {
                                    _logger.LogError("No proxies available.");
                                    return; // Exit the loop if no proxies are available
                                }
                            }

                            if (!isPlayerInRedis)
                            {
                                eloDiffTasks.Add(RetryPolicyAsync(() => eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                                    $"v1/stats/time/users/{playerinf.player_id}/games/csgo?page={page}&size=100")));
                            }

                            eloDiffTasks.Add(RetryPolicyAsync(() => eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                                $"v1/stats/time/users/{playerinf.player_id}/games/cs2?page={page}&size=100")));

                            _memoryCache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(3));
                            Console.WriteLine("Downloading 100");
                            page++;
                        }
                        else
                        {
                            Console.WriteLine("Using cached data for page " + page);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error getting HttpClient with random proxy: {ex.Message}");
                    }
                });

                await Task.WhenAll(eloDiffTasks);

                var allEloDiffResults = await Task.WhenAll(eloDiffTasks);

                var EloDiffresultsFiltered = allEloDiffResults.SelectMany(x => x);
                var saver = new SaveToRedisAsynchronous(_configuration);
                await saver.SavePlayerToRedis(playerid, EloDiffresultsFiltered);


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


            var redisFetcher = new RedisFetchMaxElo(_configuration);
            int highestElo = await redisFetcher.GetHighestEloAsync(playerinf.player_id);

            var ConnectionStatus = new PlayerStats
            {
                RedisEloRetrievesCount = RedisEloRetrievesCount,
                HighestElo = highestElo,
                OverallPlayerStatsInfo = overallplayerstats,
                Last20MatchesStats = matchstats,
                MatchHistory = matchhistory,
                Playerinfo = playerinf,
                EloDiff = eloDiff,
                Game = game,
                ErrorMessage = errorString,
            };

            ViewData["PlayerStats"] = false;

            return View(ConnectionStatus);
        }

        public async Task<T> RetryPolicyAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delay = 2000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        throw;
                    }
                    await Task.Delay(delay);
                }
            }
            return default;
        }

        // Method to get HttpClient with retry
        public static HttpClient GetHttpClientWithRetry(HttpClientManager changeProxyIp, int maxRetryCount = 3)
        {
            int retryCount = 0;
            HttpClient client = null;

            while (client == null && retryCount < maxRetryCount)
            {
                client = changeProxyIp.GetHttpClientWithRandomProxy();
                if (client == null)
                {
                    retryCount++;
                }
            }

            if (client == null)
            {
                throw new Exception("Unable to obtain a working proxy after multiple attempts.");
            }

            return client;
        }

        public IActionResult PlayerNotFound()
        {
            return View("~/Views/PlayerNotFound/PlayerNotFound.cshtml");
        }

        public async Task<ActionResult> LoadMoreMatches(string nickname, int offset, string playerID, bool isOffsetModificated, int QuantityOfEloRetrieves = 10)
        {
            int limit = 10;
            int page = 1; // doesnt matter can stay static
            string game = "cs2";

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
                for (int dzielnik = Math.Min(liczba / 2, 14); dzielnik >= 2; dzielnik--)
                {
                    if (liczba % dzielnik == 0)
                    {
                        return dzielnik;
                    }
                }
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

                var playerinf = await client.GetFromJsonAsync<PlayerStats.Rootobject>($"v4/players?nickname={nickname}");

                var matchhistory = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                    $"v4/players/{playerID}/history?game={game}&from=1200&offset={offset}&limit={limit}");

                if (matchhistory?.items == null || matchhistory.items.Count() == 0)
                {
                    game = "csgo";
                    matchhistory = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                        $"v4/players/{playerID}/history?game={game}&from=1200&offset={offset}&limit={limit}");
                }

                if (isOffsetModificated)
                {
                    var additionalMatch = await client.GetFromJsonAsync<MatchHistory.Rootobject>(
                        $"v4/players/{playerID}/history?game={game}&from=1200&offset={offset + limit}&limit=1");

                        matchhistory.items = matchhistory.items.Concat(additionalMatch.items).ToArray();
                    
                }

                if (matchhistory?.items != null && matchhistory.items.Count() > 0)
                {
                    var matchStatsList = new List<MatchStats.Rootobject>();

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
                                competition_id = "",
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
                            throw;
                        }
                    }).ToList();

                    var eloDiffTask = client2.GetFromJsonAsync<List<EloDiff.Root>>(
                        $"v1/stats/time/users/{playerID}/games/{game}?page={page}&size={QuantityOfEloRetrieves + 10}");

                    foreach (var result in await Task.WhenAll(task))
                    {
                        matchStatsList.Add(result);
                    }

                    var eloDiff = await eloDiffTask;

                    var viewModel = new MatchHistoryWithStatsViewModel
                    {
                        Playerinfo = playerinf,
                        MatchHistoryItems = matchhistory.items.ToList(),
                        MatchStats = matchStatsList,
                        EloDiff = eloDiff,
                        Game = game
                    };

                    return PartialView("MatchListPartial", viewModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching more matches: {ex.Message}");
            }

            return PartialView("MatchListPartial", new MatchHistoryWithStatsViewModel());
        }

    }
}