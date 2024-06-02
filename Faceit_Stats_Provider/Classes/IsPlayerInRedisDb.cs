using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
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
                string hashKey = $"user:{userId}:matches";
                return await db.KeyExistsAsync(hashKey);
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
