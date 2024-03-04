namespace Faceit_Stats_Provider.Models
{
    public class Proxies
    {

        public class Rootobject
        {
            public int count { get; set; }
            public string next { get; set; }
            public object previous { get; set; }
            public Result[] results { get; set; }
        }

        public class Result
        {
            public string id { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string proxy_address { get; set; }
            public int port { get; set; }
            public bool valid { get; set; }
            public DateTime last_verification { get; set; }
            public string country_code { get; set; }
            public string city_name { get; set; }
            public string asn_name { get; set; }
            public int asn_number { get; set; }
            public bool high_country_confidence { get; set; }
            public DateTime created_at { get; set; }
        }
    }

}
