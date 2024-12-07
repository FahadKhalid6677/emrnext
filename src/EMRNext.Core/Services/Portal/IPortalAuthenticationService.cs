using System;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models.Portal;

namespace EMRNext.Core.Services.Portal
{
    public interface IPortalAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string username, string password);
        Task<AuthenticationResult> ValidateTwoFactorAsync(string username, string code);
        Task<bool> ValidateSessionAsync(string sessionToken);
        Task<bool> RevokeSessionAsync(string sessionToken);
        Task<bool> RevokeAllSessionsAsync(int portalUserId);
        Task<bool> LockAccountAsync(int portalUserId, TimeSpan duration, string reason);
        Task<bool> UnlockAccountAsync(int portalUserId);
        Task<bool> UpdatePasswordAsync(int portalUserId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string resetToken, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ValidatePasswordResetTokenAsync(string email, string token);
        Task<bool> EnableTwoFactorAsync(int portalUserId);
        Task<bool> DisableTwoFactorAsync(int portalUserId);
        Task<string> GenerateTwoFactorCodeAsync(int portalUserId);
        Task<bool> ValidateDeviceAsync(int portalUserId, string deviceIdentifier);
        Task<bool> RegisterDeviceAsync(int portalUserId, PortalUserDevice device);
        Task<bool> RemoveDeviceAsync(int portalUserId, string deviceIdentifier);
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string SessionToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiry { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string ErrorMessage { get; set; }
        public PortalUser User { get; set; }
    }
}
