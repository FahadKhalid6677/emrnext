using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Security.Services
{
    /// <summary>
    /// Advanced encryption and hashing service
    /// </summary>
    public class EncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EncryptionService> _logger;

        public EncryptionService(
            IConfiguration configuration,
            ILogger<EncryptionService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Hash a password using PBKDF2
        /// </summary>
        public string HashPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(
                password, 
                salt, 
                iterations: 10000, 
                HashAlgorithmName.SHA256
            );

            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verify a password against its hash
        /// </summary>
        public bool VerifyPassword(string password, string savedPasswordHash)
        {
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            
            var pbkdf2 = new Rfc2898DeriveBytes(
                password, 
                salt, 
                iterations: 10000, 
                HashAlgorithmName.SHA256
            );
            
            byte[] hash = pbkdf2.GetBytes(20);
            
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Encrypt sensitive data using AES
        /// </summary>
        public string Encrypt(string plainText)
        {
            try 
            {
                byte[] encryptionKey = GetEncryptionKey();
                
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = encryptionKey;
                    aesAlg.GenerateIV();

                    using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (var msEncrypt = new MemoryStream())
                        {
                            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }

                            var encrypted = msEncrypt.ToArray();
                            var combinedIvCt = new byte[aesAlg.IV.Length + encrypted.Length];
                            
                            Array.Copy(aesAlg.IV, 0, combinedIvCt, 0, aesAlg.IV.Length);
                            Array.Copy(encrypted, 0, combinedIvCt, aesAlg.IV.Length, encrypted.Length);

                            return Convert.ToBase64String(combinedIvCt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encryption error");
                throw;
            }
        }

        /// <summary>
        /// Decrypt sensitive data
        /// </summary>
        public string Decrypt(string cipherText)
        {
            try 
            {
                byte[] encryptionKey = GetEncryptionKey();
                byte[] fullCipher = Convert.FromBase64String(cipherText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = encryptionKey;
                    
                    byte[] iv = new byte[16];
                    byte[] cipher = new byte[fullCipher.Length - iv.Length];
                    
                    Array.Copy(fullCipher, iv, 16);
                    Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);

                    aesAlg.IV = iv;

                    using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (var msDecrypt = new MemoryStream(cipher))
                        {
                            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Decryption error");
                throw;
            }
        }

        /// <summary>
        /// Get encryption key from configuration
        /// </summary>
        private byte[] GetEncryptionKey()
        {
            var key = _configuration["Encryption:SecretKey"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("Encryption key not configured");

            return Convert.FromBase64String(key);
        }
    }
}
