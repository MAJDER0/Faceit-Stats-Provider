using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Classes
{
    public class IsPlayerInRedisDb
    {
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;

        public IsPlayerInRedisDb(IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _configuration = configuration;
            _redis = redis;
        }

        public async Task<bool> IsPlayerInRedisAsync(string userId)
        {
            try
            {
                IDatabase db = _redis.GetDatabase();
                string key = $"userMatchesHistory_{userId}"; // Adjusted key
                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error checking player in Redis: {ex.Message}");
                return false;
            }
        }
    }
}
