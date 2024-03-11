namespace Faceit_Stats_Provider.Models
{
    public class ProxyConfig
    {
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ProxyConfiguration
    {
        public List<ProxyConfig> Proxies { get; set; }
    }
}
