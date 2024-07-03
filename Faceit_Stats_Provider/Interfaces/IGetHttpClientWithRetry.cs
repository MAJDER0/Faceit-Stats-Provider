namespace Faceit_Stats_Provider.Interfaces
{
    public interface IHttpClientRetryService
    {
        HttpClient GetHttpClientWithRetry(HttpClientManager changeProxyIp, int maxRetryCount = 3);
    }
}
