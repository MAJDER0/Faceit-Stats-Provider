namespace Faceit_Stats_Provider.Models
{
    public class RedisMatchData
    {
        public class MatchData
        {
            public string Elo { get; set; }
            public string MatchId { get; set; }
        }

        public List<MatchData> Matches { get; set; }
    }
}
