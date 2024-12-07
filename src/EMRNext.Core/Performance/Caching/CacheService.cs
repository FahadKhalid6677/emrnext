using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Performance.Caching
{
    /// <summary>
    /// Distributed caching service with advanced features
    /// </summary>
    public class CacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        // Cache configuration options
        private static readonly DistributedCacheEntryOptions DefaultCacheOptions = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30))
            .SetAbsoluteExpiration(TimeSpan.FromHours(2));

        public CacheService(
            IDistributedCache cache, 
            ILogger<CacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get cached item by key
        /// </summary>
        public async Task<T> GetAsync<T>(string key)
        {
            try 
            {
                var cachedValue = await _cache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(cachedValue))
                    return default;

                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cache for key: {key}");
                return default;
            }
        }

        /// <summary>
        /// Set cached item with default options
        /// </summary>
        public async Task SetAsync<T>(string key, T value)
        {
            await SetAsync(key, value, DefaultCacheOptions);
        }

        /// <summary>
        /// Set cached item with custom options
        /// </summary>
        public async Task SetAsync<T>(
            string key, 
            T value, 
            DistributedCacheEntryOptions options)
        {
            try 
            {
                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedValue, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting cache for key: {key}");
            }
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try 
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cache for key: {key}");
            }
        }

        /// <summary>
        /// Get or create cached item
        /// </summary>
        public async Task<T> GetOrCreateAsync<T>(
            string key, 
            Func<Task<T>> factory)
        {
            // Try to get from cache first
            var cachedValue = await GetAsync<T>(key);
            
            if (cachedValue != null)
                return cachedValue;

            // If not in cache, create and cache
            var value = await factory();
            
            if (value != null)
                await SetAsync(key, value);

            return value;
        }

        /// <summary>
        /// Create a cache key with optional prefix
        /// </summary>
        public string CreateCacheKey(string prefix, params object[] keyParts)
        {
            var keyComponents = new[] { prefix }
                .Concat(keyParts.Select(p => p?.ToString() ?? ""))
                .ToArray();

            return string.Join(":", keyComponents);
        }
    }

    /// <summary>
    /// Cache key generation helper
    /// </summary>
    public static class CacheKeys
    {
        // Patient-related cache keys
        public const string PatientPrefix = "patient";
        public const string PatientDetailsKey = "patient:details";
        public const string PatientMedicalRecordKey = "patient:medicalrecord";

        // Encounter-related cache keys
        public const string EncounterPrefix = "encounter";
        public const string EncounterDetailsKey = "encounter:details";

        // Prescription-related cache keys
        public const string PrescriptionPrefix = "prescription";
        public const string ActivePrescriptionsKey = "prescription:active";

        // Clinical Decision Support cache keys
        public const string ClinicalRulesPrefix = "clinicalrules";
        public const string ActiveClinicalRulesKey = "clinicalrules:active";
    }
}
