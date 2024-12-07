using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Entities.Portal;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace EMRNext.Core.Services.Portal
{
    public class PortalAuthenticationService : IPortalAuthenticationService
    {
        private readonly EMRNextDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly ITokenService _tokenService;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;

        public PortalAuthenticationService(
            EMRNextDbContext context,
            IEncryptionService encryptionService,
            ITokenService tokenService,
            IAuditService auditService,
            INotificationService notificationService,
            IConfiguration configuration)
        {
            _context = context;
            _encryptionService = encryptionService;
            _tokenService = tokenService;
            _auditService = auditService;
            _notificationService = notificationService;
            _configuration = configuration;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
        {
            var user = await _context.PortalUsers
                .Include(u => u.Devices)
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "Invalid credentials" };
            }

            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "Account is locked" };
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= 5)
                {
                    await LockAccountAsync(user.Id, TimeSpan.FromMinutes(30), "Too many failed attempts");
                }
                await _context.SaveChangesAsync();
                return new AuthenticationResult { Success = false, ErrorMessage = "Invalid credentials" };
            }

            if (user.TwoFactorEnabled)
            {
                var twoFactorCode = await GenerateTwoFactorCodeAsync(user.Id);
                await _notificationService.SendTwoFactorCodeAsync(user.Email, twoFactorCode);
                
                return new AuthenticationResult 
                { 
                    Success = true,
                    RequiresTwoFactor = true,
                    User = user
                };
            }

            return await GenerateAuthenticationResultAsync(user);
        }

        public async Task<AuthenticationResult> ValidateTwoFactorAsync(string username, string code)
        {
            var user = await _context.PortalUsers
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

            if (user == null)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "Invalid user" };
            }

            var storedCode = await _context.TwoFactorCodes
                .FirstOrDefaultAsync(t => t.UserId == user.Id && !t.IsUsed && t.ExpiryTime > DateTime.UtcNow);

            if (storedCode == null || storedCode.Code != code)
            {
                return new AuthenticationResult { Success = false, ErrorMessage = "Invalid or expired code" };
            }

            storedCode.IsUsed = true;
            await _context.SaveChangesAsync();

            return await GenerateAuthenticationResultAsync(user);
        }

        public async Task<bool> ValidateSessionAsync(string sessionToken)
        {
            var session = await _context.PortalUserSessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session == null || !session.IsActive || session.ExpiryDate < DateTime.UtcNow)
            {
                return false;
            }

            session.LastActivityDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeSessionAsync(string sessionToken)
        {
            var session = await _context.PortalUserSessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session == null)
            {
                return false;
            }

            session.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeAllSessionsAsync(int portalUserId)
        {
            var sessions = await _context.PortalUserSessions
                .Where(s => s.PortalUserId == portalUserId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LockAccountAsync(int portalUserId, TimeSpan duration, string reason)
        {
            var user = await _context.PortalUsers.FindAsync(portalUserId);
            if (user == null)
            {
                return false;
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.Add(duration);
            
            await _auditService.LogSecurityEventAsync(
                portalUserId,
                "AccountLocked",
                $"Account locked for {duration.TotalMinutes} minutes. Reason: {reason}"
            );

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockAccountAsync(int portalUserId)
        {
            var user = await _context.PortalUsers.FindAsync(portalUserId);
            if (user == null)
            {
                return false;
            }

            user.LockoutEnabled = false;
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;

            await _auditService.LogSecurityEventAsync(
                portalUserId,
                "AccountUnlocked",
                "Account unlocked manually"
            );

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<AuthenticationResult> GenerateAuthenticationResultAsync(PortalUser user)
        {
            var sessionToken = await _tokenService.GenerateSessionTokenAsync(user.Id);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id);
            var expiry = DateTime.UtcNow.AddHours(8);

            var session = new PortalUserSession
            {
                PortalUserId = user.Id,
                SessionToken = sessionToken,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = expiry,
                IsActive = true,
                LastActivityDate = DateTime.UtcNow
            };

            _context.PortalUserSessions.Add(session);
            
            user.LastLoginDate = DateTime.UtcNow;
            user.AccessFailedCount = 0;

            await _context.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                SessionToken = sessionToken,
                RefreshToken = refreshToken,
                TokenExpiry = expiry,
                User = user
            };
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return _encryptionService.VerifyHash(password, passwordHash);
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.PortalUsers.FindAsync(userId);
            if (user == null)
                return false;

            if (!_encryptionService.VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = _encryptionService.HashPassword(newPassword);
            user.LastPasswordChangeDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Password updated for user {userId}");

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _context.PortalUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            if (!await ValidatePasswordResetTokenAsync(email, token))
                return false;

            user.PasswordHash = _encryptionService.HashPassword(newPassword);
            user.LastPasswordChangeDate = DateTime.UtcNow;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Password reset for user {user.Id}");
            await _notificationService.SendEmailAsync(email, "Password Reset Successful", "Your password has been successfully reset.");

            return true;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.PortalUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return null;

            var token = _tokenService.GenerateRandomToken();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();
            await _notificationService.SendEmailAsync(
                email,
                "Password Reset Request",
                $"Your password reset token is: {token}. This token will expire in 24 hours.");

            return token;
        }

        public async Task<bool> ValidatePasswordResetTokenAsync(string email, string token)
        {
            var user = await _context.PortalUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            return user.PasswordResetToken == token &&
                   user.PasswordResetTokenExpiry > DateTime.UtcNow;
        }

        public async Task<bool> EnableTwoFactorAsync(int userId)
        {
            var user = await _context.PortalUsers.FindAsync(userId);
            if (user == null)
                return false;

            user.TwoFactorEnabled = true;
            user.TwoFactorSecret = _tokenService.GenerateRandomToken();

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Two-factor authentication enabled for user {userId}");

            return true;
        }

        public async Task<bool> DisableTwoFactorAsync(int userId)
        {
            var user = await _context.PortalUsers.FindAsync(userId);
            if (user == null)
                return false;

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Two-factor authentication disabled for user {userId}");

            return true;
        }

        public async Task<string> GenerateTwoFactorCodeAsync(int userId)
        {
            var user = await _context.PortalUsers.FindAsync(userId);
            if (user == null || !user.TwoFactorEnabled)
                return null;

            var code = _tokenService.GenerateRandomToken(6);
            user.TwoFactorCode = code;
            user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5);

            await _context.SaveChangesAsync();
            await _notificationService.SendSMSAsync(user.PhoneNumber, $"Your verification code is: {code}");

            return code;
        }

        public async Task<bool> ValidateDeviceAsync(int userId, string deviceId)
        {
            return await _context.PortalUserDevices
                .AnyAsync(d => d.UserId == userId.ToString() && 
                              d.DeviceId == deviceId && 
                              d.IsActive);
        }

        public async Task<bool> RegisterDeviceAsync(int userId, PortalUserDevice device)
        {
            device.UserId = userId.ToString();
            device.IsActive = true;
            device.LastUsed = DateTime.UtcNow;

            _context.PortalUserDevices.Add(device);
            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"New device registered for user {userId}");

            return true;
        }

        public async Task<bool> RemoveDeviceAsync(int userId, string deviceId)
        {
            var device = await _context.PortalUserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId.ToString() && 
                                        d.DeviceId == deviceId);

            if (device == null)
                return false;

            device.IsActive = false;
            device.DeactivatedAt = DateTime.UtcNow;
            device.DeactivatedReason = "User requested removal";

            await _context.SaveChangesAsync();
            await _auditService.LogActivityAsync($"Device removed for user {userId}");

            return true;
        }
    }
}
