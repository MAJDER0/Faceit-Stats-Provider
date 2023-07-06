namespace Faceit_Stats_Provider.Models
{
    public class EloDiff
    {

        public class Id
        {
            public string matchId { get; set; }
            public string playerId { get; set; }
        }

        public class Root
        {
            public Id _id { get; set; }
            public string status { get; set; }
            public string elo { get; set; }
        }


    }

}
