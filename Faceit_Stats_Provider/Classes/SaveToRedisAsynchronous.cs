using StackExchange.Redis;
using System.Text.Json;
using static Faceit_Stats_Provider.Models.EloDiff;

namespace Faceit_Stats_Provider.Classes
{
    public class SaveToRedisAsynchronous
    {
        public static async Task SaveToRedisAsync(string userId, string matchId, Root data)
        {
            try
            {
                // Connect to the Redis server
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379"); //local (for now) redis ip
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
