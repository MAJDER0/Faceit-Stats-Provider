namespace Faceit_Stats_Provider.Models
{
    public class MatchHistoryWithStatsViewModel
    {
        public List<MatchHistory.Item> MatchHistoryItems { get; set; }
        public List<MatchStats.Rootobject> MatchStats { get; set; }
        public List<EloDiff.Root> EloDiff { get; set; }
        public int currentPage { get; set; }
        public PlayerStats.Rootobject Playerinfo { get; set; }
        public string ErrorMessage { get; set; }
        public string Game { get; set; }
    }
}
