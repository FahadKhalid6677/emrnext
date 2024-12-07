using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMRNext.Core.Services
{
    public interface IVariableResolutionService
    {
        Task<string> ResolveVariableAsync(string variableName, Dictionary<string, object> context);
        Task<Dictionary<string, object>> ResolveAllVariablesAsync(IEnumerable<string> variables, Dictionary<string, object> context);
        Task<bool> ValidateVariableAsync(string variableName, string value);
        Task CacheVariableValueAsync(string variableName, string value, int duration);
        Task<string> GetCachedVariableValueAsync(string variableName);
    }
}
