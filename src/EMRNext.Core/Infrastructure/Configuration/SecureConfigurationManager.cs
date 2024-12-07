using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;

namespace EMRNext.Core.Infrastructure.Configuration
{
    public class SecureConfigurationManager
    {
        private readonly IConfiguration _configuration;
        private static readonly string KEY_VAULT_PREFIX = "KeyVault:";

        public SecureConfigurationManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetSecureConnectionString(string name)
        {
            var connectionString = _configuration.GetConnectionString(name);
            
            // If the connection string is stored in Key Vault
            if (connectionString?.StartsWith(KEY_VAULT_PREFIX) == true)
            {
                // In production, implement Azure Key Vault retrieval
                return DecryptConnectionString(connectionString.Substring(KEY_VAULT_PREFIX.Length));
            }

            // For development, use the connection string as is
            return connectionString;
        }

        private string DecryptConnectionString(string encryptedString)
        {
            try
            {
                // In production, implement proper decryption using Azure Key Vault
                // This is a placeholder for development
                return encryptedString;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to decrypt connection string", ex);
            }
        }

        public static string EncryptConnectionString(string connectionString)
        {
            try
            {
                // In production, implement proper encryption using Azure Key Vault
                // This is a placeholder for development
                return $"{KEY_VAULT_PREFIX}{connectionString}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to encrypt connection string", ex);
            }
        }
    }
}
