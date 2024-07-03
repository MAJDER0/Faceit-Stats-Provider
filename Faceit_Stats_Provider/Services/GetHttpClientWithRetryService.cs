using Faceit_Stats_Provider.Interfaces;
using System;
using System.Net.Http;

namespace Faceit_Stats_Provider.Services
{
    public class HttpClientRetryService : IHttpClientRetryService
    {
        public HttpClient GetHttpClientWithRetry(HttpClientManager changeProxyIp, int maxRetryCount = 3)
        {
            int retryCount = 0;
            HttpClient client = null;

            while (client == null && retryCount < maxRetryCount)
            {
                client = changeProxyIp.GetHttpClientWithRandomProxy();
                if (client == null)
                {
                    retryCount++;
                }
            }

            if (client == null)
            {
                throw new Exception("Unable to obtain a working proxy after multiple attempts.");
            }

            return client;
        }
    }
}
