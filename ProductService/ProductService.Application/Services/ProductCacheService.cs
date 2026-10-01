using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ProductService.Application.Services
{
    public class ProductCacheService
    {
        private readonly IDistributedCache _cache;
        public ProductCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var cachedData = await _cache.GetStringAsync(key);
            if(cachedData ==  null)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task SetAsync<T>(string key,T data,TimeSpan expiration)
        {
            var jsonData = JsonSerializer.Serialize(data);

            await _cache.SetStringAsync(key,
                                        jsonData,
                                        new DistributedCacheEntryOptions
                                        {
                                            AbsoluteExpirationRelativeToNow = expiration
                                        });
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
