using System;
using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(string userId, string[] roles);
        Task<string> GenerateRefreshTokenAsync(string userId);
        Task<bool> ValidateTokenAsync(string token);
        Task<(string UserId, string[] Roles)> DecodeTokenAsync(string token);
        Task RevokeTokenAsync(string token);
    }
}
