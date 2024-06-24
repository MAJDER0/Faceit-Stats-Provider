using Faceit_Stats_Provider.Models;
using Newtonsoft.Json;
using System.Net;

public class ChangeProxyIP
{
    private readonly ILogger<ChangeProxyIP> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private List<WebProxy> _proxies;
    private Random _random = new Random();

    public ChangeProxyIP(ILogger<ChangeProxyIP> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        LoadProxiesFromConfiguration();
    }

    public async Task Get()
    {
        var randomProxy = GetRandomProxy();
        if (randomProxy == null)
        {
            _logger.LogError("No proxies available.");
            return;
        }

        var ip = await GetIpWithProxyAsync(randomProxy);
        _logger.LogInformation($"Proxy: {randomProxy.Address.Host}:{randomProxy.Address.Port}, IP: {ip}");
    }

    public HttpClient GetHttpClientWithRandomProxy()
    {
        HttpClient client = null;
        while (client == null && _proxies.Count > 0)
        {
            var randomProxy = GetRandomProxy();
            if (randomProxy == null)
            {
                _logger.LogError("No proxies available.");
                break;
            }

            try
            {
                var ip = GetIpWithProxy(randomProxy);
                Console.WriteLine($"Selected Proxy: {randomProxy.Address.Host}:{randomProxy.Address.Port}, IP: {ip}");
                client = GetHttpClientWithProxy(randomProxy);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error getting HttpClient with random proxy: {ex.Message}");
                _proxies.Remove(randomProxy); // Remove the proxy that failed
            }
        }

        return client;
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
        var proxy = _proxies[index];
        _proxies.RemoveAt(index);
        return proxy;
    }

    private async Task<string> GetIpWithProxyAsync(WebProxy proxy)
    {
        using (var httpClient = GetHttpClientWithProxy(proxy))
        {
            return await httpClient.GetStringAsync("https://api.ipify.org/");
        }
    }

    private string GetIpWithProxy(WebProxy proxy)
    {
        using (var httpClient = GetHttpClientWithProxy(proxy))
        {
            return httpClient.GetStringAsync("https://api.ipify.org/").GetAwaiter().GetResult();
        }
    }

    private HttpClient GetHttpClientWithProxy(WebProxy proxy)
    {
        var handler = new HttpClientHandler { UseProxy = true, Proxy = proxy };
        if (proxy.Credentials != null)
        {
            handler.Proxy.Credentials = proxy.Credentials;
            handler.UseDefaultCredentials = false;
        }
        return new HttpClient(handler);
    }
}