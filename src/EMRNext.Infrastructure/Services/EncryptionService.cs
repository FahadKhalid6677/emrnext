using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly string _key;
        private readonly string _salt;

        public EncryptionService(string key = null, string salt = null)
        {
            _key = key ?? "YourDefaultEncryptionKey";  // In production, get from configuration
            _salt = salt ?? "YourDefaultSalt";         // In production, get from configuration
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(_key, Encoding.UTF8.GetBytes(_salt));
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write);
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                encrypted = msEncrypt.ToArray();
            }

            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            string plaintext;
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                var key = new Rfc2898DeriveBytes(_key, Encoding.UTF8.GetBytes(_salt));
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using var msDecrypt = new MemoryStream(cipherBytes);
                using var aesDecryptor = aes.CreateDecryptor();
                using var csDecrypt = new CryptoStream(msDecrypt, aesDecryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
