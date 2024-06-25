using Faceit_Stats_Provider.Models;
using Newtonsoft.Json;
using System.Net;

public class HttpClientManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpClientManager> _logger;
    private List<WebProxy> _proxies;
    private readonly Random _random;

    public HttpClientManager(ILogger<HttpClientManager> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        LoadProxiesFromConfiguration();
        _random = new Random();
    }

    private void LoadProxiesFromConfiguration()
    {
        var configurationJson = File.ReadAllText("appsettings.json");
        var proxyConfiguration = JsonConvert.DeserializeObject<ProxyConfiguration>(configurationJson);
        _proxies = proxyConfiguration.Proxies.Select(p => new WebProxy(p.Address, p.Port)
        {
            Credentials = string.IsNullOrEmpty(p.Username) ? null : new NetworkCredential(p.Username, p.Password)
        }).ToList();
    }

    private WebProxy GetRandomProxy()
    {
        if (_proxies == null || _proxies.Count == 0)
        {
            return null;
        }

        int index = _random.Next(_proxies.Count);
        return _proxies[index];
    }

    public HttpClient GetHttpClientWithRandomProxy()
    {
        var randomProxy = GetRandomProxy();

        if (randomProxy == null)
        {
            _logger.LogError("No proxies available.");
            return null;
        }

        try
        {
            var handler = new HttpClientHandler { UseProxy = true, Proxy = randomProxy };
            if (randomProxy.Credentials != null)
            {
                handler.Proxy.Credentials = randomProxy.Credentials;
                handler.UseDefaultCredentials = false;
            }

            var client = new HttpClient(handler);
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating HttpClient with proxy: {ex.Message}");
            return null;
        }
    }
}
