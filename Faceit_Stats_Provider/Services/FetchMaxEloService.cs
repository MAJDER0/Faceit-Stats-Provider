using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Services
{
    public class FetchMaxEloService : IFetchMaxElo
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FetchMaxEloService> _logger;
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IConnectionMultiplexer _redis;
        private readonly GetTotalEloRetrievesCountFromRedis _getTotalEloRetrievesCountFromRedis;
        private readonly IRetryPolicy _retryPolicyService;
        private readonly IHttpClientRetryService _httpClientRetryService;
        private readonly HttpClientManager _httpClientManager; // Inject HttpClientManager

        public FetchMaxEloService(
            IHttpClientFactory clientFactory,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<FetchMaxEloService> logger,
            IConnectionMultiplexer multiplexer,
            IConnectionMultiplexer redis,
            GetTotalEloRetrievesCountFromRedis getTotalEloRetrievesCountFromRedis,
            IRetryPolicy retryPolicyService,
            IHttpClientRetryService httpClientRetryService,
            HttpClientManager httpClientManager) // Inject HttpClientManager
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _logger = logger;
            _multiplexer = multiplexer;
            _redis = redis;
            _getTotalEloRetrievesCountFromRedis = getTotalEloRetrievesCountFromRedis;
            _retryPolicyService = retryPolicyService;
            _httpClientRetryService = httpClientRetryService;
            _httpClientManager = httpClientManager; // Assign to local variable
        }

        public async Task<HighestEloDataModel> FetchMaxEloAsync(string playerId)
        {
            long RedisEloRetrievesCount = 0;
            var client = _clientFactory.CreateClient("Faceit");
            OverallPlayerStats.Rootobject overallplayerstats;

            try
            {
                overallplayerstats = await client.GetFromJsonAsync<OverallPlayerStats.Rootobject>($"v4/players/{playerId}/stats/cs2");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching overall player stats: {ex.Message}");
                throw new Exception("Failed to fetch player stats");
            }

            if (_multiplexer.IsConnected)
            {
                var isPlayerInRedisDb = new IsPlayerInRedisDb(_configuration, _redis);
                bool isPlayerInRedis = await isPlayerInRedisDb.IsPlayerInRedisAsync(playerId);

                int SendDataToRedisLoopCondition = 0;
                int page = 0;

                if (!isPlayerInRedis)
                {
                    RedisEloRetrievesCount = (int)Math.Ceiling((double)int.Parse(overallplayerstats.lifetime.Matches) / 100) + 1;
                }
                else
                {
                    SendDataToRedisLoopCondition = (int)(await _getTotalEloRetrievesCountFromRedis.GetTotalEloRetrievesCountFromRedisAsync(playerId) / 100);
                    RedisEloRetrievesCount = (int)Math.Ceiling((double)int.Parse(overallplayerstats.lifetime.Matches) / 100) + 1;
                }

                var eloDiffTasks = new List<Task<List<RedisMatchData.MatchData>>>();
                HttpClient eloDiffClient = null;

                var retrieveCount = (int)RedisEloRetrievesCount - SendDataToRedisLoopCondition;
                var eloRetrievesCount = Enumerable.Range(0, retrieveCount).ToList();

                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 3 };

                object pageLock = new object();

                // Fetch CS:GO data
                var csGoTasks = eloRetrievesCount.Select(async id =>
                {
                    int currentPage;
                    lock (pageLock)
                    {
                        currentPage = page++;
                    }

                    string cacheKey = $"PlayerDataCsGoElo_{playerId}_Page_{currentPage}";

                    if (!_memoryCache.TryGetValue(cacheKey, out List<RedisMatchData.MatchData> cachedData))
                    {
                        if (currentPage == 0 || currentPage % 30 == 0 || currentPage == SendDataToRedisLoopCondition)
                        {
                            eloDiffClient = _httpClientRetryService.GetHttpClientWithRetry(_httpClientManager);
                            if (eloDiffClient != null)
                            {
                                eloDiffClient.BaseAddress = new Uri("https://api.faceit.com/stats/");
                            }
                            else
                            {
                                _logger.LogError("No proxies available.");
                                return new List<RedisMatchData.MatchData>();  // Return an empty list instead of null
                            }
                        }

                        if (!isPlayerInRedis)
                        {
                            var result = await _retryPolicyService.RetryPolicyAsync(() => eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                                $"v1/stats/time/users/{playerId}/games/csgo?page={currentPage}&size=100"));

                            _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(3));
                            Console.WriteLine("Downloaded 100 CS:GO records for page " + currentPage);
                            return result ?? new List<RedisMatchData.MatchData>(); // Ensure to return a non-null result
                        }
                    }
                    else
                    {
                        Console.WriteLine("Using cached CS:GO data for page " + currentPage);
                        return cachedData ?? new List<RedisMatchData.MatchData>();
                    }
                    return new List<RedisMatchData.MatchData>(); // Fallback in case nothing is fetched
                }).ToList();

                // Fetch CS2 data
                page = 0; // Reset page
                var cs2Tasks = eloRetrievesCount.Select(async id =>
                {
                    int currentPage;
                    lock (pageLock)
                    {
                        currentPage = page++;
                    }

                    string cacheKey = $"PlayerDataCs2Elo_{playerId}_Page_{currentPage}";

                    if (!_memoryCache.TryGetValue(cacheKey, out List<RedisMatchData.MatchData> cachedData))
                    {
                        if (currentPage == 0 || currentPage % 30 == 0 || currentPage == SendDataToRedisLoopCondition)
                        {
                            eloDiffClient = _httpClientRetryService.GetHttpClientWithRetry(_httpClientManager);
                            if (eloDiffClient != null)
                            {
                                eloDiffClient.BaseAddress = new Uri("https://api.faceit.com/stats/");
                            }
                            else
                            {
                                _logger.LogError("No proxies available.");
                                return new List<RedisMatchData.MatchData>();  // Return an empty list instead of null
                            }
                        }

                        var result = await _retryPolicyService.RetryPolicyAsync(() => eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                            $"v1/stats/time/users/{playerId}/games/cs2?page={currentPage}&size=100"));

                        _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(3));
                        Console.WriteLine("Downloaded 100 CS2 records for page " + currentPage);
                        return result ?? new List<RedisMatchData.MatchData>(); // Ensure to return a non-null result
                    }
                    else
                    {
                        Console.WriteLine("Using cached CS2 data for page " + currentPage);
                        return cachedData ?? new List<RedisMatchData.MatchData>();
                    }
                }).ToList();

                // Await both CS:GO and CS2 tasks
                await Task.WhenAll(csGoTasks.Concat(cs2Tasks));

                // Collect all results
                var allEloDiffResults = csGoTasks.Concat(cs2Tasks)
                    .Where(t => t.Result != null && t.Result.Count > 0) // Ensure only non-null results
                    .SelectMany(t => t.Result)
                    .ToList();

                // If there's no new data, skip saving to Redis
                if (allEloDiffResults.Any())
                {
                    var saver = new SaveToRedisAsynchronous(_configuration);
                    await saver.SavePlayerToRedis(playerId, allEloDiffResults);
                }
            }

            // Fetch the highest elo data
            var FetchedMaxElosFromRedisCs2 = 0;
            var FetchedMaxElosFromRedisCs2MatchID = "";
            var FetchedMaxElosFromRedisCsgo = 0;
            var FetchedMaxElosFromRedisCsgoMatchID = "";

            if (_multiplexer.IsConnected)
            {
                var redisFetcher = new RedisFetchMaxElo(_configuration);
                var FetchedMaxElos = await redisFetcher.GetHighestEloAsync(playerId);
                FetchedMaxElosFromRedisCs2 = FetchedMaxElos.HighestCs2Elo;
                FetchedMaxElosFromRedisCs2MatchID = FetchedMaxElos.Cs2MatchId;

                FetchedMaxElosFromRedisCsgo = FetchedMaxElos.HighestCsgoElo;
                FetchedMaxElosFromRedisCsgoMatchID = FetchedMaxElos.CsgoMatchId;
            }

            var HighestElos = new HighestEloDataModel
            {
                HighestCs2Elo = FetchedMaxElosFromRedisCs2,
                HighestCs2MatchID = FetchedMaxElosFromRedisCs2MatchID,
                HighestCsgoElo = FetchedMaxElosFromRedisCsgo,
                HighestCsgoMatchID = FetchedMaxElosFromRedisCsgoMatchID
            };

            return HighestElos;
        }


    }
}
