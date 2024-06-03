using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Faceit_Stats_Provider.Models;

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
        try
        {
            IDatabase db = _redis.GetDatabase();
            string hashKey = $"user:{userId}:matches";
            var matchKeys = await db.HashKeysAsync(hashKey);

            int highestElo = 0;

            foreach (var matchKey in matchKeys)
            {
                var jsonData = await db.HashGetAsync(hashKey, matchKey);
                Console.WriteLine($"Fetched JSON Data: {jsonData}");

                RedisMatchData.MatchData matchData;
                try
                {
                    matchData = JsonSerializer.Deserialize<RedisMatchData.MatchData>(jsonData);
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"Error deserializing JSON data: {jsonEx.Message}");
                    continue; // Skip this entry and continue with the next one
                }

                if (matchData != null && int.TryParse(matchData.elo, out int elo) && elo > highestElo)
                {
                    highestElo = elo;
                }
            }

            return highestElo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching highest ELO from Redis: {ex.Message}");
            return 0;
        }
    }
}