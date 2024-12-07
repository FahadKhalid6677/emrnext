using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Services.Interfaces;
using EMRNext.Core.Configuration;

namespace EMRNext.Core.Services
{
    /// <summary>
    /// Provides advanced encryption and decryption services for secure document handling
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger<EncryptionService> _logger;
        private readonly EncryptionConfiguration _encryptionConfig;

        public EncryptionService(
            ILogger<EncryptionService> logger,
            EncryptionConfiguration encryptionConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _encryptionConfig = encryptionConfig ?? throw new ArgumentNullException(nameof(encryptionConfig));
        }

        /// <inheritdoc/>
        public async Task<string> EncryptAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be null or empty", nameof(content));

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Convert.FromBase64String(_encryptionConfig.EncryptionKey);
                    aesAlg.IV = GenerateIV();

                    using (var encryptor = aesAlg.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Write IV to the beginning of the encrypted stream
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            await swEncrypt.WriteAsync(content);
                        }

                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during document encryption");
                throw new SecurityException("Document encryption failed", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> DecryptAsync(string encryptedContent)
        {
            if (string.IsNullOrWhiteSpace(encryptedContent))
                throw new ArgumentException("Encrypted content cannot be null or empty", nameof(encryptedContent));

            try
            {
                byte[] fullCipherText = Convert.FromBase64String(encryptedContent);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Convert.FromBase64String(_encryptionConfig.EncryptionKey);

                    // Extract IV from the first 16 bytes
                    byte[] iv = new byte[16];
                    Array.Copy(fullCipherText, 0, iv, 0, 16);
                    aesAlg.IV = iv;

                    using (var decryptor = aesAlg.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(fullCipherText, 16, fullCipherText.Length - 16))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return await srDecrypt.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during document decryption");
                throw new SecurityException("Document decryption failed", ex);
            }
        }

        /// <inheritdoc/>
        public string GenerateEncryptionKey()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateKey();
                return Convert.ToBase64String(aesAlg.Key);
            }
        }

        /// <inheritdoc/>
        public bool ValidateDocumentIntegrity(string encryptedContent)
        {
            try
            {
                // Attempt decryption as a basic integrity check
                DecryptAsync(encryptedContent).Wait();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a secure Initialization Vector (IV)
        /// </summary>
        private byte[] GenerateIV()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateIV();
                return aesAlg.IV;
            }
        }

        /// <summary>
        /// Custom exception for encryption and decryption failures
        /// </summary>
        public class SecurityException : Exception
        {
            public SecurityException(string message, Exception innerException)
                : base(message, innerException) { }
        }
    }
}
