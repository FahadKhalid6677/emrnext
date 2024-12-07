using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Models;

namespace EMRNext.Core.Interfaces
{
    public interface ITokenGenerationService
    {
        Task<TokenResponse> GenerateTokenAsync(User user, IEnumerable<string> roles);
        Task<TokenResponse> RefreshTokenAsync(string token);
    }
}
