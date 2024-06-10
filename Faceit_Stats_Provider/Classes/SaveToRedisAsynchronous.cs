using Faceit_Stats_Provider.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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

    public async Task SavePlayerToRedis(string userId, IEnumerable<RedisMatchData.MatchData> data)
    {
        IDatabase db = Connection.GetDatabase();
        var key = $"userMatchesHistory_{userId}";

        // Retrieve the existing data from the database
        var existingDataJson = await db.StringGetAsync(key);
        var existingData = existingDataJson.HasValue
            ? JsonConvert.DeserializeObject<HashSet<RedisMatchData.MatchData>>(existingDataJson)
            : new HashSet<RedisMatchData.MatchData>();

        // Create a dictionary to track unique matches by MatchId
        var matchDictionary = existingData.ToDictionary(match => match.MatchId);

        // Add new data to the dictionary, ensuring uniqueness by MatchId
        foreach (var match in data)
        {
            matchDictionary[match.MatchId] = match;
        }

        // Get the combined data as a HashSet
        var combinedData = new HashSet<RedisMatchData.MatchData>(matchDictionary.Values);

        // Serialize the combined data and save it back to the database
        var combinedDataJson = JsonConvert.SerializeObject(combinedData);
        await db.StringSetAsync(key, combinedDataJson);
    }
}
