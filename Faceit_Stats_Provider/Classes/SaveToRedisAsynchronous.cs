using StackExchange.Redis;
using System.Text.Json;
using static Faceit_Stats_Provider.Models.EloDiff;
using Microsoft.Extensions.Configuration;

namespace Faceit_Stats_Provider.Classes
{
    public class SaveToRedisAsynchronous
    {
        private readonly IConfiguration _configuration;

        public SaveToRedisAsynchronous(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SaveToRedisAsync(string userId, string matchId, Root data)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("Redis"); // Assuming your connection string key in appsettings.json is named "Redis"

                // Connect to the Redis server using the connection string from appsettings.json
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
                IDatabase db = redis.GetDatabase();

                // Check if match_id already exists in the hash
                if (!await db.HashExistsAsync($"user:{userId}:matches", matchId))
                {
                    // Create a dictionary to represent the data you want to store
                    var dataToStore = new
                    {
                        elo = data.elo,
                        match_Id = data.match_Id
                    };

                    // Convert the data object to JSON
                    string jsonData = JsonSerializer.Serialize(dataToStore);

                    // Save data to Redis using Hashes
                    string hashKey = $"user:{userId}:matches";
                    await db.HashSetAsync(hashKey, matchId, jsonData);
                }
                else
                {
                    // Handle the case when match_id already exists (skip or update)
                    Console.WriteLine($"Match ID {matchId} already exists in the database. Skipping save operation.");
                }

                // Close the connection
                redis.Close();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error saving data to Redis: {ex.Message}");
            }
        }
    }
}
