using Faceit_Stats_Provider.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GetTotalEloRetrievesCountFromRedis
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public GetTotalEloRetrievesCountFromRedis(IConfiguration configuration, IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    public async Task<long> GetTotalEloRetrievesCountFromRedisAsync(string playerId)
    {
        try
        {
            // Adjust the pattern to match the key structure used in SavePlayerToRedis method
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var pattern = $"userMatchesHistory_{playerId}";
            var keys = server.Keys(pattern: pattern).ToArray();

            Console.WriteLine($"Found {keys.Length} keys for playerId {playerId}");

            // Use a HashSet to store unique match IDs where elo != 0
            var uniqueMatchIds = new HashSet<string>();

            foreach (var key in keys)
            {
                try
                {
                    var keyType = await _db.KeyTypeAsync(key);
                    if (keyType == RedisType.String)
                    {
                        var jsonString = await _db.StringGetAsync(key);
                        var matchData = JsonConvert.DeserializeObject<List<RedisMatchData.MatchData>>(jsonString);

                        if (matchData != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Key: {key}, Total Matches: {matchData.Count}");
                            Console.ResetColor();

                            // Filter matches where elo != 0 and add to HashSet
                            foreach (var match in matchData)
                            {
                                if (!string.IsNullOrEmpty(match.matchId) && match.elo != 0)
                                {
                                    uniqueMatchIds.Add(match.matchId);
                                }
                            }

                            Console.WriteLine($"Matches with elo != 0: {uniqueMatchIds.Count}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Key: {key} is not a string, it is of type: {keyType}");
                    }
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"An error occurred while processing key {key}: {innerEx.Message}");
                }
            }

            // Return the count of unique match IDs where elo != 0
            return uniqueMatchIds.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return 0; // or handle as appropriate
        }
    }
}
