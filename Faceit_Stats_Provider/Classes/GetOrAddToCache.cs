using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Faceit_Stats_Provider.Classes
{
    public class GetOrAddToCache
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public GetOrAddToCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetOrAddToCacheAsync<T>(string cacheKey, Func<Task<T>> factory)
        {
            if (!_memoryCache.TryGetValue(cacheKey, out T cacheEntry))
            {
                try
                {
                    cacheEntry = await factory(); // Call the API
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Log the specific request that failed with a 404 error
                    Console.WriteLine($"404 error for cache key: {cacheKey}. API endpoint not found.");
                    return default(T); // Return a default value in case of 404
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    Console.WriteLine($"Error occurred while fetching data for cache key: {cacheKey}. Message: {ex.Message}");
                    return default(T); // Return a default value in case of other errors
                }

                // If no exceptions, add the result to the cache
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheDuration
                };
                _memoryCache.Set(cacheKey, cacheEntry, cacheEntryOptions);
            }
            return cacheEntry;
        }
    }
}
