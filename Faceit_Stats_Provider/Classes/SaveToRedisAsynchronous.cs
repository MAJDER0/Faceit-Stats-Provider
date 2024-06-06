using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Faceit_Stats_Provider.Models
{
    public class SaveToRedisAsynchronous
    {
        private readonly IConfiguration _configuration;
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

        public SaveToRedisAsynchronous(IConfiguration configuration)
        {
            _configuration = configuration;
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                string connectionString = _configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connectionString);
            });
        }

        private ConnectionMultiplexer Connection => _lazyConnection.Value;

        public async Task SavePlayerToRedis(string userId, IEnumerable<EloDiff.Root> data)
        {
            IDatabase db = Connection.GetDatabase();

            try
            {
                var redisData = new List<RedisMatchData.MatchData>(); // Updated to a list of MatchData

                foreach (var item in data)
                {
                    // Check if Elo and MatchId are not null before adding
                    if (item.elo != null && item.match_Id != null)
                    {
                        // Add each match data to RedisMatchData
                        redisData.Add(new RedisMatchData.MatchData
                        {
                            Elo = item.elo.ToString(),
                            MatchId = item.match_Id
                        });
                    }
                }

                var userMatches = JsonConvert.SerializeObject(redisData);
                var key = $"userMatchesHistory_{userId}";
                await db.StringSetAsync(key, userMatches);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error saving player data to Redis: {ex.Message}");
            }
        }
    }
}
