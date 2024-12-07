using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Text.Json;

namespace EMRNext.Web.Configuration
{
    public static class CachingConfig
    {
        public static IServiceCollection AddCachingConfiguration(this IServiceCollection services)
        {
            // Add Memory Cache
            services.AddMemoryCache();

            // Add Distributed Cache (Redis)
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
                options.InstanceName = "EMRNext_";
            });

            // Register Cache Service
            services.AddSingleton<ICacheService, CacheService>();

            return services;
        }
    }

    public interface ICacheService
    {
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly MemoryCacheEntryOptions _defaultMemoryOptions;
        private readonly DistributedCacheEntryOptions _defaultDistributedOptions;

        public CacheService(IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            
            _defaultMemoryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _defaultDistributedOptions = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            // Try memory cache first
            if (_memoryCache.TryGetValue(key, out T cachedValue))
            {
                return cachedValue;
            }

            // Try distributed cache
            var distributedValue = await GetAsync<T>(key);
            if (distributedValue != null)
            {
                // Set in memory cache and return
                SetMemoryCache(key, distributedValue, expiration);
                return distributedValue;
            }

            // Get fresh value
            var value = await factory();

            // Set in both caches
            await SetAsync(key, value, expiration);
            SetMemoryCache(key, value, expiration);

            return value;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _distributedCache.GetStringAsync(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.SetAbsoluteExpiration(expiration.Value);
            }
            else
            {
                options = _defaultDistributedOptions;
            }

            var jsonValue = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, jsonValue, options);
        }

        public async Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key);
        }

        private void SetMemoryCache<T>(string key, T value, TimeSpan? expiration)
        {
            var options = expiration.HasValue
                ? new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiration.Value)
                : _defaultMemoryOptions;

            _memoryCache.Set(key, value, options);
        }
    }
}
