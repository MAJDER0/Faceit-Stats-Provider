namespace Faceit_Stats_Provider.Models
{
    public class ResponeFromSteamForCustomSteamID
    {

        public class Rootobject
        {
            public Response response { get; set; }
        }

        public class Response
        {
            public string steamid { get; set; }
            public int success { get; set; }
        }

    }
}
