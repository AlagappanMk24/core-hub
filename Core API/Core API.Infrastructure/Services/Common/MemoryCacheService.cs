using Core_API.Application.Contracts.Services.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace Core_API.Infrastructure.Services.Common
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<T> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T value);
            return System.Threading.Tasks.Task.FromResult(value);
        }

        public System.Threading.Tasks.Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            _cache.Set(key, value, expiration);
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }
}