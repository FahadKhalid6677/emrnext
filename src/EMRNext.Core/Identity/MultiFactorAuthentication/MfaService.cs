using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OtpNet;
using EMRNext.Core.Identity;

namespace EMRNext.Core.Identity.MultiFactorAuthentication
{
    /// <summary>
    /// Service for managing Multi-Factor Authentication (MFA)
    /// </summary>
    public class MfaService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MfaService> _logger;
        private const int BackupCodeCount = 5;
        private const int BackupCodeLength = 8;

        public MfaService(
            UserManager<ApplicationUser> userManager,
            ILogger<MfaService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate a new TOTP secret key for MFA
        /// </summary>
        public string GenerateMfaSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        /// <summary>
        /// Generate backup recovery codes
        /// </summary>
        public string[] GenerateBackupCodes()
        {
            var random = new Random();
            return Enumerable.Range(0, BackupCodeCount)
                .Select(_ => GenerateBackupCode(random))
                .ToArray();
        }

        /// <summary>
        /// Validate TOTP code
        /// </summary>
        public bool ValidateTotpCode(string secretKey, string userProvidedCode)
        {
            try 
            {
                var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                
                // Allow a small time window for code validation
                return totp.VerifyTotp(
                    userProvidedCode, 
                    out _, 
                    VerificationWindow.RfcSpecifiedNetworkDelay
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TOTP validation failed");
                return false;
            }
        }

        /// <summary>
        /// Enable MFA for a user
        /// </summary>
        public async Task<MfaSetupResult> EnableMfaAsync(ApplicationUser user, string secretKey)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try 
            {
                // Generate backup codes
                var backupCodes = GenerateBackupCodes();

                // Store MFA details
                user.MfaEnabled = true;
                user.MfaSecretKey = secretKey;
                user.MfaBackupCodes = string.Join(",", backupCodes.Select(HashBackupCode));

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"MFA enabled for user {user.Email}");
                    return new MfaSetupResult 
                    { 
                        Success = true, 
                        BackupCodes = backupCodes,
                        SecretKey = secretKey 
                    };
                }

                return new MfaSetupResult 
                { 
                    Success = false, 
                    Errors = result.Errors.Select(e => e.Description).ToArray() 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling MFA");
                return new MfaSetupResult 
                { 
                    Success = false, 
                    Errors = new[] { "An unexpected error occurred" } 
                };
            }
        }

        /// <summary>
        /// Disable MFA for a user
        /// </summary>
        public async Task<bool> DisableMfaAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try 
            {
                user.MfaEnabled = false;
                user.MfaSecretKey = null;
                user.MfaBackupCodes = null;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"MFA disabled for user {user.Email}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA");
                return false;
            }
        }

        /// <summary>
        /// Validate MFA code (TOTP or Backup Code)
        /// </summary>
        public async Task<MfaValidationResult> ValidateMfaCodeAsync(
            ApplicationUser user, 
            string providedCode)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!user.MfaEnabled)
                return new MfaValidationResult { Success = true };

            // Check TOTP
            if (!string.IsNullOrEmpty(user.MfaSecretKey))
            {
                if (ValidateTotpCode(user.MfaSecretKey, providedCode))
                    return new MfaValidationResult { Success = true };
            }

            // Check Backup Codes
            if (!string.IsNullOrEmpty(user.MfaBackupCodes))
            {
                var backupCodes = user.MfaBackupCodes.Split(',');
                var hashedProvidedCode = HashBackupCode(providedCode);

                if (backupCodes.Contains(hashedProvidedCode))
                {
                    // Remove used backup code
                    var updatedBackupCodes = backupCodes
                        .Where(code => code != hashedProvidedCode)
                        .ToArray();

                    user.MfaBackupCodes = string.Join(",", updatedBackupCodes);
                    await _userManager.UpdateAsync(user);

                    return new MfaValidationResult { Success = true, UsedBackupCode = true };
                }
            }

            return new MfaValidationResult { Success = false };
        }

        /// <summary>
        /// Generate a single backup code
        /// </summary>
        private string GenerateBackupCode(Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, BackupCodeLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Hash backup code for secure storage
        /// </summary>
        private string HashBackupCode(string code)
        {
            return BCrypt.Net.BCrypt.HashPassword(code);
        }
    }

    /// <summary>
    /// Result of MFA setup
    /// </summary>
    public class MfaSetupResult
    {
        public bool Success { get; set; }
        public string SecretKey { get; set; }
        public string[] BackupCodes { get; set; }
        public string[] Errors { get; set; }
    }

    /// <summary>
    /// Result of MFA validation
    /// </summary>
    public class MfaValidationResult
    {
        public bool Success { get; set; }
        public bool UsedBackupCode { get; set; }
    }
}
