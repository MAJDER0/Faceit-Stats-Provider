using static Faceit_Stats_Provider.Models.MatchHistory;

namespace Faceit_Stats_Provider.Models
{
    public class PlayerStats
    {
        public Rootobject Playerinfo { get; set; }
        public MatchHistory.Rootobject MatchHistory { get; set; }
        public List<MatchStats.Round> Last20MatchesStats { get; set; }
        public OverallPlayerStats.Rootobject OverallPlayerStatsInfo { get; set; }
        public List<EloDiff.Root> EloDiff { get; set; }
        public List<EloDiff.Root> AllHistory { get; set; }
        public int? HighestElo { get; set; }
        public string ErrorMessage { get; set; }


        public class Rootobject
        {
            public string player_id { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public string country { get; set; }
            public string cover_image { get; set; }
            public Platforms platforms { get; set; }
            public Games games { get; set; }
            public Settings settings { get; set; }
            public string[] friends_ids { get; set; }
            public string new_steam_id { get; set; }
            public string steam_id_64 { get; set; }
            public string steam_nickname { get; set; }
            public string[] memberships { get; set; }
            public string faceit_url { get; set; }
            public string membership_type { get; set; }
            public string cover_featured_image { get; set; }
            public Infractions infractions { get; set; }
            public bool verified { get; set; }
            public DateTime activated_at { get; set; }
        }

        public class Platforms
        {
            public string steam { get; set; }
        }

        public class Games
        {
            public Cs2 cs2 { get; set; }
            public Csgo csgo { get; set; }
        }

        public class Cs2
        {
            public string region { get; set; }
            public string game_player_id { get; set; }
            public int skill_level { get; set; }
            public int faceit_elo { get; set; }
            public string game_player_name { get; set; }
            public string skill_level_label { get; set; }
            public Regions regions { get; set; }
            public string game_profile_id { get; set; }
        }

        public class Regions
        {
        }

        public class Csgo
        {
            public string region { get; set; }
            public string game_player_id { get; set; }
            public int skill_level { get; set; }
            public int faceit_elo { get; set; }
            public string game_player_name { get; set; }
            public string skill_level_label { get; set; }
            public Regions1 regions { get; set; }
            public string game_profile_id { get; set; }
        }

        public class Regions1
        {
        }

        public class Settings
        {
            public string language { get; set; }
        }

        public class Infractions
        {
        }

    }
}
