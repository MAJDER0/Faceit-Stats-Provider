namespace Faceit_Stats_Provider.Models
{
    public class MatchType
    {

        public class Rootobject
        {
            public string match_id { get; set; }
            public int version { get; set; }
            public string game { get; set; }
            public string region { get; set; }
            public string competition_id { get; set; }
            public string competition_type { get; set; }
            public string competition_name { get; set; }
            public string organizer_id { get; set; }
            public bool calculate_elo { get; set; }
            public int configured_at { get; set; }
            public int started_at { get; set; }
            public int finished_at { get; set; }
            public string[] demo_url { get; set; }
            public string chat_room_id { get; set; }
            public int best_of { get; set; }
            public string status { get; set; }
        }

    }
}
