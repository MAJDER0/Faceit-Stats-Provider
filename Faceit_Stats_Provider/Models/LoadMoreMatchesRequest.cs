namespace Faceit_Stats_Provider.Models
{
    public class LoadMoreMatchesRequest
    {
        public string nickname { get; set; }
        public int offset { get; set; }
        public string playerID { get; set; }
        public bool isOffsetModificated { get; set; }
        public int QuantityOfEloRetrieves { get; set; }
        public List<EloDiff.Root> currentModel { get; set; }  
        public int currentPage { get; set; }
        public int CsGoSwap { get; set; } 
        public string Game { get; set; }
    }


}
