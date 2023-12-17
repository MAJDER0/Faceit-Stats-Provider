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
            public Teams teams { get; set; }
            public Voting voting { get; set; }
            public bool calculate_elo { get; set; }
            public int configured_at { get; set; }
            public int started_at { get; set; }
            public int finished_at { get; set; }
            public string[] demo_url { get; set; }
            public string chat_room_id { get; set; }
            public int best_of { get; set; }
            public Results results { get; set; }
            public Detailed_Results[] detailed_results { get; set; }
            public string status { get; set; }
            public string faceit_url { get; set; }
        }

        public class Teams
        {
            public Faction2 faction2 { get; set; }
            public Faction1 faction1 { get; set; }
        }

        public class Faction2
        {
            public string faction_id { get; set; }
            public string leader { get; set; }
            public string avatar { get; set; }
            public Roster[] roster { get; set; }
            public Stats stats { get; set; }
            public bool substituted { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        public class Stats
        {
            public float winProbability { get; set; }
            public Skilllevel skillLevel { get; set; }
            public int rating { get; set; }
        }

        public class Skilllevel
        {
            public int average { get; set; }
            public Range range { get; set; }
        }

        public class Range
        {
            public int min { get; set; }
            public int max { get; set; }
        }

        public class Roster
        {
            public string player_id { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public string membership { get; set; }
            public string game_player_id { get; set; }
            public string game_player_name { get; set; }
            public int game_skill_level { get; set; }
            public bool anticheat_required { get; set; }
        }

        public class Faction1
        {
            public string faction_id { get; set; }
            public string leader { get; set; }
            public string avatar { get; set; }
            public Roster1[] roster { get; set; }
            public Stats1 stats { get; set; }
            public bool substituted { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        public class Stats1
        {
            public float winProbability { get; set; }
            public Skilllevel1 skillLevel { get; set; }
            public int rating { get; set; }
        }

        public class Skilllevel1
        {
            public int average { get; set; }
            public Range1 range { get; set; }
        }

        public class Range1
        {
            public int min { get; set; }
            public int max { get; set; }
        }

        public class Roster1
        {
            public string player_id { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public string membership { get; set; }
            public string game_player_id { get; set; }
            public string game_player_name { get; set; }
            public int game_skill_level { get; set; }
            public bool anticheat_required { get; set; }
        }

        public class Voting
        {
            public string[] voted_entity_types { get; set; }
            public Location location { get; set; }
            public Map map { get; set; }
        }

        public class Location
        {
            public Entity[] entities { get; set; }
            public string[] pick { get; set; }
        }

        public class Entity
        {
            public string image_sm { get; set; }
            public string name { get; set; }
            public string class_name { get; set; }
            public string game_location_id { get; set; }
            public string guid { get; set; }
            public string image_lg { get; set; }
        }

        public class Map
        {
            public Entity1[] entities { get; set; }
            public string[] pick { get; set; }
        }

        public class Entity1
        {
            public string game_map_id { get; set; }
            public string guid { get; set; }
            public string image_lg { get; set; }
            public string image_sm { get; set; }
            public string name { get; set; }
            public string class_name { get; set; }
        }

        public class Results
        {
            public string winner { get; set; }
            public Score score { get; set; }
        }

        public class Score
        {
            public int faction1 { get; set; }
            public int faction2 { get; set; }
        }

        public class Detailed_Results
        {
            public bool asc_score { get; set; }
            public string winner { get; set; }
            public Factions factions { get; set; }
        }

        public class Factions
        {
            public Faction11 faction1 { get; set; }
            public Faction21 faction2 { get; set; }
        }

        public class Faction11
        {
            public int score { get; set; }
        }

        public class Faction21
        {
            public int score { get; set; }
        }

    }
}
