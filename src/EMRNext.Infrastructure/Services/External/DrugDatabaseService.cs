using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMRNext.Infrastructure.Services.External
{
    public class DrugDatabaseService : IDrugDatabaseService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DrugDatabaseService> _logger;
        private readonly DrugDatabaseConfig _config;

        public DrugDatabaseService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<DrugDatabaseService> logger,
            IOptions<ExternalServicesConfiguration> config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config?.Value?.DrugDatabase ?? throw new ArgumentNullException(nameof(config));

            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _config.ApiKey);
        }

        public async Task<DrugInfo> GetDrugInfoAsync(string ndc)
        {
            try
            {
                var cacheKey = $"drug_info_{ndc}";
                if (_config.EnableRealTimeChecks || !_cache.TryGetValue(cacheKey, out DrugInfo cachedInfo))
                {
                    _logger.LogInformation("Fetching drug info for NDC: {NDC}", ndc);
                    var response = await _httpClient.GetAsync($"/api/v1/drugs/{ndc}");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var drugInfo = JsonSerializer.Deserialize<DrugInfo>(content);

                    if (drugInfo != null)
                    {
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_config.CacheExpirationMinutes));
                        _cache.Set(cacheKey, drugInfo, cacheOptions);
                    }

                    return drugInfo;
                }

                return cachedInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching drug info for NDC: {NDC}", ndc);
                throw;
            }
        }

        public async Task<IEnumerable<DrugInteraction>> CheckInteractionsAsync(IEnumerable<string> ndcList)
        {
            try
            {
                _logger.LogInformation("Checking drug interactions for NDCs: {NDCs}", string.Join(", ", ndcList));
                
                var request = new { NDCs = ndcList };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/interactions/check", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<DrugInteraction>>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking drug interactions");
                throw;
            }
        }

        public async Task<IEnumerable<DrugAllergy>> CheckAllergyInteractionsAsync(string ndc, IEnumerable<string> allergyList)
        {
            try
            {
                _logger.LogInformation("Checking allergy interactions for NDC: {NDC}", ndc);
                
                var request = new { NDC = ndc, Allergies = allergyList };
                var response = await _httpClient.PostAsJsonAsync("/api/v1/allergies/check", request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<DrugAllergy>>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking allergy interactions for NDC: {NDC}", ndc);
                throw;
            }
        }

        public async Task<IEnumerable<DrugInfo>> SearchDrugsAsync(string query, int limit = 10)
        {
            try
            {
                _logger.LogInformation("Searching drugs with query: {Query}", query);
                
                var response = await _httpClient.GetAsync($"/api/v1/drugs/search?q={Uri.EscapeDataString(query)}&limit={limit}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<DrugInfo>>(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching drugs with query: {Query}", query);
                throw;
            }
        }

        public async Task<DrugFormulary> GetFormularyInfoAsync(string ndc, string insurancePlanId)
        {
            try
            {
                var cacheKey = $"formulary_{ndc}_{insurancePlanId}";
                if (_config.EnableRealTimeChecks || !_cache.TryGetValue(cacheKey, out DrugFormulary cachedInfo))
                {
                    _logger.LogInformation("Fetching formulary info for NDC: {NDC}, Plan: {PlanId}", ndc, insurancePlanId);
                    
                    var response = await _httpClient.GetAsync($"/api/v1/formulary/{ndc}?planId={insurancePlanId}");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var formularyInfo = JsonSerializer.Deserialize<DrugFormulary>(content);

                    if (formularyInfo != null)
                    {
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_config.CacheExpirationMinutes));
                        _cache.Set(cacheKey, formularyInfo, cacheOptions);
                    }

                    return formularyInfo;
                }

                return cachedInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching formulary info for NDC: {NDC}, Plan: {PlanId}", ndc, insurancePlanId);
                throw;
            }
        }
    }
}
