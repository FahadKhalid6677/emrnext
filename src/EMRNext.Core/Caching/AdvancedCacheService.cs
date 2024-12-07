using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using EMRNext.Core.Caching.Models;
using EMRNext.Core.Caching.Strategies;

namespace EMRNext.Core.Caching
{
    public class AdvancedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<AdvancedCacheService> _logger;
        private readonly ICacheStrategyFactory _strategyFactory;
        private readonly ICacheMonitor _cacheMonitor;

        public AdvancedCacheService(
            IDistributedCache cache,
            ILogger<AdvancedCacheService> logger,
            ICacheStrategyFactory strategyFactory,
            ICacheMonitor cacheMonitor)
        {
            _cache = cache;
            _logger = logger;
            _strategyFactory = strategyFactory;
            _cacheMonitor = cacheMonitor;
        }

        public async Task<T> GetOrSetAsync<T>(
            string key, 
            Func<Task<T>> factory,
            CacheOptions options = null)
        {
            try
            {
                // Monitor cache operation
                using var monitor = _cacheMonitor.BeginOperation(key);

                // Try to get from cache
                var cachedValue = await _cache.GetAsync(key);
                if (cachedValue != null)
                {
                    monitor.RecordHit();
                    return JsonSerializer.Deserialize<T>(cachedValue);
                }

                // Cache miss - get from factory
                monitor.RecordMiss();
                var strategy = _strategyFactory.GetStrategy(options?.Strategy ?? CacheStrategy.Standard);
                var value = await factory();

                // Set in cache with strategy-specific options
                var cacheOptions = strategy.GetCacheOptions(options);
                await _cache.SetAsync(
                    key,
                    JsonSerializer.SerializeToUtf8Bytes(value),
                    cacheOptions);

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cache operation for key: {Key}", key);
                throw;
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                using var monitor = _cacheMonitor.BeginOperation(key);
                var value = await _cache.GetAsync(key);
                
                if (value == null)
                {
                    monitor.RecordMiss();
                    return default;
                }

                monitor.RecordHit();
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                throw;
            }
        }

        public async Task SetAsync<T>(
            string key, 
            T value, 
            CacheOptions options = null)
        {
            try
            {
                var strategy = _strategyFactory.GetStrategy(options?.Strategy ?? CacheStrategy.Standard);
                var cacheOptions = strategy.GetCacheOptions(options);

                await _cache.SetAsync(
                    key,
                    JsonSerializer.SerializeToUtf8Bytes(value),
                    cacheOptions);

                _cacheMonitor.RecordSet(key, JsonSerializer.SerializeToUtf8Bytes(value).Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
                throw;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _cacheMonitor.RecordRemoval(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
                throw;
            }
        }

        public async Task<bool> RefreshAsync(string key)
        {
            try
            {
                var value = await _cache.GetAsync(key);
                if (value == null)
                    return false;

                await _cache.RefreshAsync(key);
                _cacheMonitor.RecordRefresh(key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache for key: {Key}", key);
                throw;
            }
        }
    }
}
