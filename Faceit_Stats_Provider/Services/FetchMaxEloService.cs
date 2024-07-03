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
                        string cacheKey = $"PlayerData_{playerId}_Page_{page}";

                        if (!_memoryCache.TryGetValue(cacheKey, out List<RedisMatchData.MatchData> cachedData))
                        {
                            if (page == 0 || page % 30 == 0 || page == SendDataToRedisLoopCondition)
                            {
                                eloDiffClient = _httpClientRetryService.GetHttpClientWithRetry(_httpClientManager); // Provide HttpClientManager
                                if (eloDiffClient != null)
                                {
                                    eloDiffClient.BaseAddress = new Uri("https://api.faceit.com/stats/");
                                }
                                else
                                {
                                    _logger.LogError("No proxies available.");
                                    return;
                                }
                            }

                            if (!isPlayerInRedis)
                            {
                                eloDiffTasks.Add(_retryPolicyService.RetryPolicyAsync(() => eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                                    $"v1/stats/time/users/{playerId}/games/csgo?page={page}&size=100")));
                            }

                            eloDiffTasks.Add(_retryPolicyService.RetryPolicyAsync(() => eloDiffClient.GetFromJsonAsync<List<RedisMatchData.MatchData>>(
                                $"v1/stats/time/users/{playerId}/games/cs2?page={page}&size=100")));

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
                await saver.SavePlayerToRedis(playerId, EloDiffresultsFiltered);
            }

            var FetchedMaxElosFromRedisCs2 = 0;
            var FetchedMaxElosFromRedisCsgo = 0;

            if (_multiplexer.IsConnected)
            {
                var redisFetcher = new RedisFetchMaxElo(_configuration);
                var FetchedMaxElos = await redisFetcher.GetHighestEloAsync(playerId);
                FetchedMaxElosFromRedisCs2 = FetchedMaxElos.HighestCs2Elo;
                FetchedMaxElosFromRedisCsgo = FetchedMaxElos.HighestCsgoElo;
            }

            var HighestElos = new HighestEloDataModel
            {
                HighestCs2Elo = FetchedMaxElosFromRedisCs2,
                HighestCsgoElo = FetchedMaxElosFromRedisCsgo
            };

            return HighestElos;
        }
    }
}
