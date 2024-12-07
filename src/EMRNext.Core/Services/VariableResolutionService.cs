using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using EMRNext.Core.Domain.Entities;
using Newtonsoft.Json;

namespace EMRNext.Core.Services
{
    public class VariableResolutionService : IVariableResolutionService
    {
        private readonly EMRDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IClinicalDecisionSupportService _clinicalService;

        public VariableResolutionService(
            EMRDbContext context,
            IMemoryCache cache,
            IClinicalDecisionSupportService clinicalService)
        {
            _context = context;
            _cache = cache;
            _clinicalService = clinicalService;
        }

        public async Task<string> ResolveVariableAsync(string variableName, Dictionary<string, object> context)
        {
            // Check cache first
            var cachedValue = await GetCachedVariableValueAsync(variableName);
            if (!string.IsNullOrEmpty(cachedValue))
                return cachedValue;

            // Check context
            if (context.ContainsKey(variableName))
                return context[variableName]?.ToString();

            // Resolve from database
            var variable = await _context.TemplateVariables
                .FirstOrDefaultAsync(v => v.Name == variableName);

            if (variable == null)
                return string.Empty;

            var value = await ResolveFromSourceAsync(variable, context);
            
            // Cache if enabled
            if (variable.EnableCache)
            {
                await CacheVariableValueAsync(variableName, value, variable.CacheDuration ?? 300);
            }

            return value;
        }

        public async Task<Dictionary<string, object>> ResolveAllVariablesAsync(IEnumerable<string> variables, Dictionary<string, object> context)
        {
            var results = new Dictionary<string, object>();

            foreach (var variable in variables)
            {
                results[variable] = await ResolveVariableAsync(variable, context);
            }

            return results;
        }

        public async Task<bool> ValidateVariableAsync(string variableName, string value)
        {
            var variable = await _context.TemplateVariables
                .FirstOrDefaultAsync(v => v.Name == variableName);

            if (variable == null)
                return false;

            if (variable.EnableValidation && !string.IsNullOrEmpty(variable.ValidationRules))
            {
                var rules = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(variable.ValidationRules);
                return await ValidateAgainstRulesAsync(value, rules);
            }

            return true;
        }

        public async Task CacheVariableValueAsync(string variableName, string value, int duration)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(duration));

            _cache.Set(GetCacheKey(variableName), value, cacheOptions);
        }

        public async Task<string> GetCachedVariableValueAsync(string variableName)
        {
            if (_cache.TryGetValue(GetCacheKey(variableName), out string value))
                return value;

            return null;
        }

        private async Task<string> ResolveFromSourceAsync(TemplateVariable variable, Dictionary<string, object> context)
        {
            switch (variable.SourceType?.ToLower())
            {
                case "database":
                    return await ResolveDatabaseVariableAsync(variable, context);
                
                case "api":
                    return await ResolveApiVariableAsync(variable, context);
                
                case "function":
                    return await ResolveFunctionVariableAsync(variable, context);
                
                case "clinical":
                    return await ResolveClinicalVariableAsync(variable, context);
                
                default:
                    return variable.DefaultValue ?? string.Empty;
            }
        }

        private async Task<string> ResolveDatabaseVariableAsync(TemplateVariable variable, Dictionary<string, object> context)
        {
            try
            {
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable.SourceConfig);
                
                if (!config.ContainsKey("table") || !config.ContainsKey("field"))
                    return string.Empty;

                var table = config["table"];
                var field = config["field"];
                var whereClause = config.GetValueOrDefault("where", "");

                // Implementation would depend on your specific database access pattern
                // This is a placeholder for demonstration
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> ResolveApiVariableAsync(TemplateVariable variable, Dictionary<string, object> context)
        {
            try
            {
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable.SourceConfig);
                
                if (!config.ContainsKey("endpoint"))
                    return string.Empty;

                // Implementation would depend on your API client
                // This is a placeholder for demonstration
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> ResolveFunctionVariableAsync(TemplateVariable variable, Dictionary<string, object> context)
        {
            try
            {
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable.SourceConfig);
                
                if (!config.ContainsKey("function"))
                    return string.Empty;

                var functionName = config["function"];
                var parameters = config.GetValueOrDefault("parameters", "{}");

                // Implementation would depend on your function resolution system
                // This is a placeholder for demonstration
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> ResolveClinicalVariableAsync(TemplateVariable variable, Dictionary<string, object> context)
        {
            try
            {
                if (!context.ContainsKey("EncounterId"))
                    return string.Empty;

                var encounterId = Convert.ToInt32(context["EncounterId"]);
                
                switch (variable.MappingPath?.ToLower())
                {
                    case "vitals.latest":
                        var vitals = await _context.Vitals
                            .Where(v => v.EncounterId == encounterId)
                            .OrderByDescending(v => v.RecordedAt)
                            .FirstOrDefaultAsync();
                        return JsonConvert.SerializeObject(vitals);

                    case "medications.active":
                        var medications = await _context.Medications
                            .Where(m => m.EncounterId == encounterId && m.IsActive)
                            .ToListAsync();
                        return JsonConvert.SerializeObject(medications);

                    case "problems.active":
                        var problems = await _context.Problems
                            .Where(p => p.EncounterId == encounterId && p.IsActive)
                            .ToListAsync();
                        return JsonConvert.SerializeObject(problems);

                    default:
                        return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<bool> ValidateAgainstRulesAsync(string value, List<Dictionary<string, object>> rules)
        {
            foreach (var rule in rules)
            {
                if (!await ValidateRuleAsync(value, rule))
                    return false;
            }

            return true;
        }

        private async Task<bool> ValidateRuleAsync(string value, Dictionary<string, object> rule)
        {
            if (!rule.ContainsKey("type"))
                return true;

            switch (rule["type"].ToString().ToLower())
            {
                case "required":
                    return !string.IsNullOrEmpty(value);

                case "regex":
                    if (!rule.ContainsKey("pattern"))
                        return true;
                    return Regex.IsMatch(value, rule["pattern"].ToString());

                case "range":
                    if (!decimal.TryParse(value, out decimal numValue))
                        return false;
                    
                    var min = rule.ContainsKey("min") ? Convert.ToDecimal(rule["min"]) : decimal.MinValue;
                    var max = rule.ContainsKey("max") ? Convert.ToDecimal(rule["max"]) : decimal.MaxValue;
                    
                    return numValue >= min && numValue <= max;

                case "length":
                    var minLength = rule.ContainsKey("min") ? Convert.ToInt32(rule["min"]) : 0;
                    var maxLength = rule.ContainsKey("max") ? Convert.ToInt32(rule["max"]) : int.MaxValue;
                    
                    return value.Length >= minLength && value.Length <= maxLength;

                default:
                    return true;
            }
        }

        private string GetCacheKey(string variableName)
        {
            return $"variable_{variableName}";
        }
    }
}
