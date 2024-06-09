using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using Faceit_Stats_Provider.Models;
using System.Text.Json;

public class RedisFetchMaxElo
{
    private readonly IConfiguration _configuration;
    private readonly ConnectionMultiplexer _redis;

    public RedisFetchMaxElo(IConfiguration configuration)
    {
        _configuration = configuration;
        _redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
    }

    public async Task<int> GetHighestEloAsync(string userId)
    {
        IDatabase db = _redis.GetDatabase();
        var key = $"userMatchesHistory_{userId}";

        string userMatchesJson = await db.StringGetAsync(key);
        if (string.IsNullOrEmpty(userMatchesJson))
        {
            // No data found
            return 0; // Or any default value
        }

        var userMatches = JsonSerializer.Deserialize<IEnumerable<RedisMatchData.MatchData>>(userMatchesJson);
        if (userMatches == null || !userMatches.Any())
        {
            // No matches found
            return 0; // Or any default value
        }

        // Assuming Elo is an integer property in MatchData
        var highestElo = userMatches.Max(match => match.elo);
        return highestElo;
    }
}
