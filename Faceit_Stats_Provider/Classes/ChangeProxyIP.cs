using Faceit_Stats_Provider.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

public class ChangeProxyIP
{
    private readonly ILogger<ChangeProxyIP> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ChangeProxyIP(ILogger<ChangeProxyIP> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Get()
    {
        var proxies = LoadProxiesFromConfiguration();

        var randomProxy = GetRandomProxy(proxies);
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
        var proxies = LoadProxiesFromConfiguration();
        var randomProxy = GetRandomProxy(proxies);

        if (randomProxy == null)
        {
            _logger.LogError("No proxies available.");
            return null;
        }

        var ip = GetIpWithProxy(randomProxy);
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine($"Selected Proxy: {randomProxy.Address.Host}:{randomProxy.Address.Port}, IP: {ip}");
        Console.ResetColor();

        return GetHttpClientWithProxy(randomProxy);
    }

    private List<WebProxy> LoadProxiesFromConfiguration()
    {
        // Load proxy settings from the configuration file
        var configurationJson = File.ReadAllText("appsettings.json"); // Adjust the path if needed

        var proxyConfiguration = Newtonsoft.Json.JsonConvert.DeserializeObject<ProxyConfiguration>(configurationJson);

        var webProxies = new List<WebProxy>();
        foreach (var proxySettings in proxyConfiguration.Proxies)
        {
            var webProxy = new WebProxy(proxySettings.Address, proxySettings.Port);

            // Set credentials if provided
            if (!string.IsNullOrEmpty(proxySettings.Username) && !string.IsNullOrEmpty(proxySettings.Password))
            {
                webProxy.Credentials = new NetworkCredential(proxySettings.Username, proxySettings.Password);
            }

            webProxies.Add(webProxy);
        }

        return webProxies;
    }

    private WebProxy GetRandomProxy(List<WebProxy> proxies)
    {
        if (proxies == null || proxies.Count == 0)
        {
            return null;
        }

        var random = new Random();
        return proxies[random.Next(proxies.Count)];
    }

    private async Task<string> GetIpWithProxyAsync(WebProxy proxy)
    {
        using (var httpClient = GetHttpClientWithProxy(proxy))
        {
            // Use httpClient for making requests with the specified proxy
            return await httpClient.GetStringAsync("https://api.ipify.org/");
        }
    }

    private HttpClient GetHttpClientWithProxy(WebProxy proxy)
    {
        var handler = new HttpClientHandler { UseProxy = true, Proxy = proxy };

        // If your proxy requires credentials, add them here
        if (proxy.Credentials != null)
        {
            handler.Proxy.Credentials = proxy.Credentials;
            handler.UseDefaultCredentials = false;
        }

        return new HttpClient(handler);
    }


    private string GetIpWithProxy(WebProxy proxy)
    {
        // Use a new instance of HttpClient for getting IP
        using (var httpClient = GetHttpClientWithProxy(proxy))
        {
            return httpClient.GetStringAsync("https://api.ipify.org/").GetAwaiter().GetResult();
        }
    }
}
