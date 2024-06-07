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

    public async Task SavePlayerToRedis(string userId, IEnumerable<EloDiff.Root> data)
    {
        IDatabase db = Connection.GetDatabase();

        // Check if match_id already exists in the hash

        var userMatches = data.ToHashSet();
        var key = $"userMatchesHistory_{userId}";
        var userMatchesJson = JsonConvert.SerializeObject(userMatches);

        await db.StringSetAsync(key, userMatchesJson);
    }
}