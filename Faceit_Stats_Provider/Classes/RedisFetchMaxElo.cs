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

    public async Task<(int HighestCsgoElo, string CsgoMatchId, int HighestCs2Elo, string Cs2MatchId)> GetHighestEloAsync(string userId)
    {
        IDatabase db = _redis.GetDatabase();
        var key = $"userMatchesHistory_{userId}";

        string userMatchesJson = await db.StringGetAsync(key);
        if (string.IsNullOrEmpty(userMatchesJson))
        {
            return (0, string.Empty, 0, string.Empty); // No data found, return default values
        }

        var userMatches = JsonSerializer.Deserialize<IEnumerable<RedisMatchData.MatchData>>(userMatchesJson);
        if (userMatches == null || !userMatches.Any())
        {
            return (0, string.Empty, 0, string.Empty); // No matches found, return default values
        }

        // Get the highest ELO and match ID for CSGO
        var highestCsgoMatch = userMatches
            .Where(match => match.game == "csgo")
            .Select(match => new { match.elo, match.matchId })
            .OrderByDescending(match => match.elo)
            .FirstOrDefault();

        // Get the highest ELO and match ID for CS2
        var highestCs2Match = userMatches
            .Where(match => match.game == "cs2")
            .Select(match => new { match.elo, match.matchId })
            .OrderByDescending(match => match.elo)
            .FirstOrDefault();

        // Extract the ELO and match ID for both games
        int highestCsgoElo = highestCsgoMatch?.elo ?? 0;
        string csgoMatchId = highestCsgoMatch?.matchId ?? string.Empty;

        int highestCs2Elo = highestCs2Match?.elo ?? 0;
        string cs2MatchId = highestCs2Match?.matchId ?? string.Empty;

        return (highestCsgoElo, csgoMatchId, highestCs2Elo, cs2MatchId);
    }

}
