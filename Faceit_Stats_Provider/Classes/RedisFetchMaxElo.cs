using StackExchange.Redis;
using System.Text.Json;

public class RedisFetchMaxElo
{
    private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");

    public static async Task<int> GetHighestEloAsync(string userId)
    {
        try
        {
            IDatabase db = redis.GetDatabase();
            string hashKey = $"user:{userId}:matches";
            var matchKeys = await db.HashKeysAsync(hashKey);

            int highestElo = 0;

            foreach (var matchKey in matchKeys)
            {
                var jsonData = await db.HashGetAsync(hashKey, matchKey);
                Console.WriteLine($"Fetched JSON Data: {jsonData}");

                MatchData matchData;
                try
                {
                    matchData = JsonSerializer.Deserialize<MatchData>(jsonData);
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

public class MatchData
{
    public string elo { get; set; }
    public string match_Id { get; set; }
}
