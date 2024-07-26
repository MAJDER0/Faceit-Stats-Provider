namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class TransformedPlayerMatchStats
    {
        public string playerId { get; set; }
        public AnalyzerMatchStats.Rootobject matchStats { get; set; }
    }
}
