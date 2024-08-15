namespace Faceit_Stats_Provider.Models
{
    public class MatchHistory
    {
        public class Rootobject
        {
            public Item[] items { get; set; }
        }

        public class Item
        {
            public string match_id { get; set; }
            public string game_id { get; set; }
            public string region { get; set; }
            public string match_type { get; set; }
            public Teams teams { get; set; }
            public string[] playing_players { get; set; }
            public string competition_id { get; set; }
            public string competition_name { get; set; }
            public string competition_type { get; set; }
            public Results results { get; set; }
            public long finished_at { get; set; }
        }

        public class Teams
        {
            public Faction2 faction2 { get; set; }
            public Faction1 faction1 { get; set; }
        }

        public class Faction2
        {
            public string team_id { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public Player[] players { get; set; }
        }

        public class Player
        {
            public string player_id { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public int skill_level { get; set; }
            public string game_player_id { get; set; }
            public string game_player_name { get; set; }
        }

        public class Faction1
        {
            public string team_id { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public string type { get; set; }
            public Player1[] players { get; set; }
        }

        public class Player1
        {
            public string player_id { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public int skill_level { get; set; }
            public string game_player_id { get; set; }
            public string game_player_name { get; set; }
        }

        public class Results
        {
            public Score score { get; set; }
        }

        public class Score
        {
            public int faction1 { get; set; }
            public int faction2 { get; set; }
        }

    }
}