namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class AnalyzerMatchHistory
    {

        public class Rootobject
        {
            public Item[] items { get; set; }
        }

        public class Item
        {
            public string match_id { get; set; }
        }
    }
}
