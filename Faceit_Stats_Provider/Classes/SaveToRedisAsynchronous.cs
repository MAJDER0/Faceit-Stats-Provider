using Faceit_Stats_Provider.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

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

        var existingDataJson = await db.StringGetAsync(key);
        var existingData = existingDataJson.HasValue
            ? JsonConvert.DeserializeObject<HashSet<RedisMatchData.MatchData>>(existingDataJson)
            : new HashSet<RedisMatchData.MatchData>();

        var matchDictionary = existingData.ToDictionary(match => match.matchId);

        foreach (var match in data)
        {
            matchDictionary[match.matchId] = match;
        }

        var combinedData = new HashSet<RedisMatchData.MatchData>(matchDictionary.Values);

        var combinedDataJson = JsonConvert.SerializeObject(combinedData);
        await db.StringSetAsync(key, combinedDataJson);
    }
}