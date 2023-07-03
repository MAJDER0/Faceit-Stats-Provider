namespace Faceit_Stats_Provider.Models
{
    public class EloDiff
    {

        public class Root
        {
            public Class1[] Property1 { get; set; }
        }

        public class Class1
        {
            public _Id _id { get; set; }
            public long created_at { get; set; }
            public long updated_at { get; set; }
            public string i9 { get; set; }
            public string nickname { get; set; }
            public string i10 { get; set; }
            public string i13 { get; set; }
            public string i15 { get; set; }
            public string i6 { get; set; }
            public string i14 { get; set; }
            public string i7 { get; set; }
            public string i16 { get; set; }
            public string i8 { get; set; }
            public string playerId { get; set; }
            public string c3 { get; set; }
            public string c2 { get; set; }
            public string c4 { get; set; }
            public string c1 { get; set; }
            public string i19 { get; set; }
            public string teamId { get; set; }
            public string i3 { get; set; }
            public string i4 { get; set; }
            public string i5 { get; set; }
            public bool premade { get; set; }
            public string c5 { get; set; }
            public string bestOf { get; set; }
            public string competitionId { get; set; }
            public long date { get; set; }
            public string game { get; set; }
            public string gameMode { get; set; }
            public string i0 { get; set; }
            public string i1 { get; set; }
            public string i12 { get; set; }
            public string i18 { get; set; }
            public string i2 { get; set; }
            public string matchId { get; set; }
            public string matchRound { get; set; }
            public string played { get; set; }
            public string status { get; set; }
            public string elo { get; set; }
        }

        public class _Id
        {
            public string matchId { get; set; }
            public string playerId { get; set; }
        }

    }

}
