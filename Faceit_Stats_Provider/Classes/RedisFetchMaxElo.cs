using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using Faceit_Stats_Provider.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

public class RedisFetchMaxElo
{
    private readonly IConfiguration _configuration;
    private readonly ConnectionMultiplexer _redis;

    public RedisFetchMaxElo(IConfiguration configuration)
    {
        _configuration = configuration;
        _redis = ConnectionMultiplexer.Connect(_configuration.GetConnectionString("Redis"));
    }

    public async Task<(int HighestCsgoElo, int HighestCs2Elo)> GetHighestEloAsync(string userId)
    {
        IDatabase db = _redis.GetDatabase();
        var key = $"userMatchesHistory_{userId}";

        string userMatchesJson = await db.StringGetAsync(key);
        if (string.IsNullOrEmpty(userMatchesJson))
        {
            return (0, 0); // No data found, return default values
        }

        var userMatches = JsonSerializer.Deserialize<IEnumerable<RedisMatchData.MatchData>>(userMatchesJson);
        if (userMatches == null || !userMatches.Any())
        {
            return (0, 0); // No matches found, return default values
        }

        var HighestCsgoElo = userMatches
            .Where(match => match.game == "csgo") 
            .Select(match => match.elo) 
            .DefaultIfEmpty(0) 
            .Max(); 

        var HighestCs2Elo = userMatches
            .Where(match => match.game == "cs2") 
            .Select(match => match.elo) 
            .DefaultIfEmpty(0) 
            .Max(); 

        return (HighestCsgoElo, HighestCs2Elo);
    }
}
