namespace Faceit_Stats_Provider.ModelsForAnalyzer
{
    public class AnalyzerMatchPlayersOldMatch
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
            public Roster_V1[] roster_v1 { get; set; }
            public bool substituted { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        public class Roster_V1
        {
            public string guid { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public string csgo_id { get; set; }
            public int csgo_skill_level { get; set; }
            public string csgo_skill_level_label { get; set; }
        }

        public class Faction2
        {
            public string faction_id { get; set; }
            public string leader { get; set; }
            public string avatar { get; set; }
            public Roster_V11[] roster_v1 { get; set; }
            public bool substituted { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        public class Roster_V11
        {
            public string guid { get; set; }
            public string nickname { get; set; }
            public string avatar { get; set; }
            public string csgo_id { get; set; }
            public int csgo_skill_level { get; set; }
            public string csgo_skill_level_label { get; set; }
        }
    }
}
