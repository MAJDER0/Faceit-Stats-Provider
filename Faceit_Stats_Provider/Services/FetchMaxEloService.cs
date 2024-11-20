using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent; // For ConcurrentDictionary
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
        private readonly GetTotalEloRetrievesCountFromRedis _getTotalEloRetrievesCountFromRedis;
        private readonly IRetryPolicy _retryPolicyService;
        private readonly IHttpClientRetryService _httpClientRetryService;
        private readonly HttpClientManager _httpClientManager;

        public FetchMaxEloService(
            IHttpClientFactory clientFactory,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<FetchMaxEloService> logger,
            IConnectionMultiplexer multiplexer,
            GetTotalEloRetrievesCountFromRedis getTotalEloRetrievesCountFromRedis,
            IRetryPolicy retryPolicyService,
            IHttpClientRetryService httpClientRetryService,
            HttpClientManager httpClientManager)
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _logger = logger;
            _multiplexer = multiplexer;
            _getTotalEloRetrievesCountFromRedis = getTotalEloRetrievesCountFromRedis;
            _retryPolicyService = retryPolicyService;
            _httpClientRetryService = httpClientRetryService;
            _httpClientManager = httpClientManager;
        }

        public async Task<HighestEloDataModel> FetchMaxEloAsync(string playerId)
        {
            try
            {
                long redisEloRetrievesCount = 0;
                var client = _clientFactory.CreateClient("Faceit");
                OverallPlayerStats.Rootobject overallPlayerStats;

                try
                {
                    overallPlayerStats = await client.GetFromJsonAsync<OverallPlayerStats.Rootobject>($"v4/players/{playerId}/stats/cs2");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching overall player stats for playerId: {PlayerId}", playerId);
                    throw new Exception("Failed to fetch player stats", ex);
                }

                if (_multiplexer.IsConnected)
                {
                    var isPlayerInRedisDb = new IsPlayerInRedisDb(_configuration, _multiplexer);
                    bool isPlayerInRedis = await isPlayerInRedisDb.IsPlayerInRedisAsync(playerId);

                    int sendDataToRedisLoopCondition = 0;
                    int page = 0;

                    if (!isPlayerInRedis)
                    {
                        redisEloRetrievesCount = (int)Math.Ceiling((double)int.Parse(overallPlayerStats.lifetime.Matches) / 100) + 1;
                    }
                    else
                    {
                        sendDataToRedisLoopCondition = (int)(await _getTotalEloRetrievesCountFromRedis.GetTotalEloRetrievesCountFromRedisAsync(playerId) / 100);
                        redisEloRetrievesCount = (int)Math.Ceiling((double)int.Parse(overallPlayerStats.lifetime.Matches) / 100) + 1;
                    }

                    // Ensure retrieveCount is non-negative
                    var retrieveCount = Math.Max(0, (int)redisEloRetrievesCount - sendDataToRedisLoopCondition);

                    if (retrieveCount == 0)
                    {
                        _logger.LogInformation("No new data to fetch for player {PlayerId}", playerId);
                    }

                    var eloRetrievesCount = Enumerable.Range(0, retrieveCount).ToList();

                    // Use ConcurrentDictionary to store HttpClient instances per thread
                    var httpClientDict = new ConcurrentDictionary<int, HttpClient>();

                    // Fetch CS:GO data
                    var csGoTasks = eloRetrievesCount.Select(async id =>
                    {
                        int currentPage;
                        lock (this)
                        {
                            currentPage = page++;
                        }

                        string cacheKey = $"PlayerDataCsGoElo_{playerId}_Page_{currentPage}";

                        if (!_memoryCache.TryGetValue(cacheKey, out List<RedisMatchData.MatchData> cachedData))
                        {
                            HttpClient eloDiffClient = GetOrCreateHttpClient(httpClientDict);

                            if (!isPlayerInRedis)
                            {
                                try
                                {
                                    var result = await _retryPolicyService.RetryPolicyAsync(() =>
                                        eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                                            $"v1/stats/time/users/{playerId}/games/csgo?page={currentPage}&size=100"));

                                    _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(3));
                                    _logger.LogInformation("Downloaded 100 CS:GO records for page {CurrentPage}", currentPage);
                                    return result ?? new List<RedisMatchData.MatchData>();
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error fetching CS:GO data for page {CurrentPage}", currentPage);
                                    return new List<RedisMatchData.MatchData>();
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Using cached CS:GO data for page {CurrentPage}", currentPage);
                            return cachedData ?? new List<RedisMatchData.MatchData>();
                        }
                        return new List<RedisMatchData.MatchData>();
                    }).ToList();

                    // Fetch CS2 data
                    page = 0; // Reset page
                    var cs2Tasks = eloRetrievesCount.Select(async id =>
                    {
                        int currentPage;
                        lock (this)
                        {
                            currentPage = page++;
                        }

                        string cacheKey = $"PlayerDataCs2Elo_{playerId}_Page_{currentPage}";

                        if (!_memoryCache.TryGetValue(cacheKey, out List<RedisMatchData.MatchData> cachedData))
                        {
                            HttpClient eloDiffClient = GetOrCreateHttpClient(httpClientDict);

                            try
                            {
                                var result = await _retryPolicyService.RetryPolicyAsync(() =>
                                    eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                                        $"v1/stats/time/users/{playerId}/games/cs2?page={currentPage}&size=100"));

                                _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(3));
                                _logger.LogInformation("Downloaded 100 CS2 records for page {CurrentPage}", currentPage);
                                return result ?? new List<RedisMatchData.MatchData>();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error fetching CS2 data for page {CurrentPage}", currentPage);
                                return new List<RedisMatchData.MatchData>();
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Using cached CS2 data for page {CurrentPage}", currentPage);
                            return cachedData ?? new List<RedisMatchData.MatchData>();
                        }
                    }).ToList();

                    // Await both CS:GO and CS2 tasks
                    await Task.WhenAll(csGoTasks.Concat(cs2Tasks));

                    // Dispose HttpClient instances
                    foreach (var clientEntry in httpClientDict.Values)
                    {
                        clientEntry.Dispose();
                    }

                    // Collect all results
                    var allEloDiffResults = csGoTasks.Concat(cs2Tasks)
                        .Where(t => t.Result != null && t.Result.Count > 0)
                        .SelectMany(t => t.Result)
                        .ToList();

                    // If there's new data, save to Redis
                    if (allEloDiffResults.Any())
                    {
                        var saver = new SaveToRedisAsynchronous(_configuration);
                        await saver.SavePlayerToRedis(playerId, allEloDiffResults);
                    }
                }

                // Fetch the highest elo data
                int fetchedMaxEloCs2 = 0;
                string fetchedMaxEloCs2MatchId = "";
                int fetchedMaxEloCsgo = 0;
                string fetchedMaxEloCsgoMatchId = "";

                if (_multiplexer.IsConnected)
                {
                    var redisFetcher = new RedisFetchMaxElo(_configuration);
                    var fetchedMaxElos = await redisFetcher.GetHighestEloAsync(playerId);
                    fetchedMaxEloCs2 = fetchedMaxElos.HighestCs2Elo;
                    fetchedMaxEloCs2MatchId = fetchedMaxElos.Cs2MatchId;

                    fetchedMaxEloCsgo = fetchedMaxElos.HighestCsgoElo;
                    fetchedMaxEloCsgoMatchId = fetchedMaxElos.CsgoMatchId;
                }

                var highestElos = new HighestEloDataModel
                {
                    HighestCs2Elo = fetchedMaxEloCs2,
                    HighestCs2MatchID = fetchedMaxEloCs2MatchId,
                    HighestCsgoElo = fetchedMaxEloCsgo,
                    HighestCsgoMatchID = fetchedMaxEloCsgoMatchId
                };

                return highestElos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in FetchMaxEloAsync for playerId: {PlayerId}", playerId);
                throw;
            }
        }

        private HttpClient GetOrCreateHttpClient(ConcurrentDictionary<int, HttpClient> httpClientDict)
        {
            int threadId = Environment.CurrentManagedThreadId;

            return httpClientDict.GetOrAdd(threadId, _ =>
            {
                HttpClient client = _httpClientRetryService.GetHttpClientWithRetry(_httpClientManager);
                if (client == null)
                {
                    _logger.LogError("No proxies available.");
                    throw new Exception("No proxies available.");
                }
                client.BaseAddress = new Uri("https://api.faceit.com/stats/");
                return client;
            });
        }
    }
}
