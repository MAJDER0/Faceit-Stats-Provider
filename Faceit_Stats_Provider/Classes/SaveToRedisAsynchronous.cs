using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Faceit_Stats_Provider.Models;

namespace Faceit_Stats_Provider.Classes
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

        public async Task SaveToRedisAsync(string userId, string matchId, EloDiff.Root data)
        {
            try
            {
                IDatabase db = Connection.GetDatabase();

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
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error saving data to Redis: {ex.Message}");
            }
        }
    }
}
