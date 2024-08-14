namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class PlayerPartialViewModel
    {
        public List<(List<(double, bool, double, string)>, string)> MapStatsForSinglePlayer { get; set; }
        public List<(List<(double, bool, double, string)>, string)> MapStatsForSinglePlayerCopy { get; set; }
        public List<string> Maps { get; set; }
        public AnalyzerMatchPlayers.Roster[] Faction1Players { get; set; }
        public AnalyzerMatchPlayers.Roster[] Faction2Players { get; set; }
        public List<AnalyzerPlayerStats.Rootobject> Faction1PlayerStats { get; set; }
        public List<AnalyzerPlayerStats.Rootobject> Faction2PlayerStats { get; set; }
    }

}
