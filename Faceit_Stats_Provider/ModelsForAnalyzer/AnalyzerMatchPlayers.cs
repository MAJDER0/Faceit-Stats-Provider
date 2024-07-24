namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class AnalyzerMatchPlayers
    {

        public class Rootobject
        {

            public Teams teams { get; set; }

        }

        public class Teams
        {
            public Faction1 faction1 { get; set; }
            public Faction2 faction2 { get; set; }
        }

        public class Faction1
        {
            public string faction_id { get; set; }
            public string leader { get; set; }
            public string avatar { get; set; }
            public Roster[] roster { get; set; }
            public bool substituted { get; set; }
            public string name { get; set; }
            public string type { get; set; }
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

        public class Faction2
        {
            public string faction_id { get; set; }
            public string leader { get; set; }
            public string avatar { get; set; }
            public Roster[] roster { get; set; }
            public bool substituted { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

    }
}
        