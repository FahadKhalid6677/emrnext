using System;
using Microsoft.Extensions.Configuration;

namespace EMRNext.Core.Configuration
{
    /// <summary>
    /// Configuration class for managing encryption settings
    /// </summary>
    public class EncryptionConfiguration
    {
        private readonly IConfiguration _configuration;

        public EncryptionConfiguration(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets the encryption key from configuration
        /// </summary>
        public string EncryptionKey
        {
            get
            {
                var key = _configuration["Encryption:Key"];
                
                // If no key is configured, generate a new one
                if (string.IsNullOrWhiteSpace(key))
                {
                    key = GenerateDefaultEncryptionKey();
                }

                return key;
            }
        }

        /// <summary>
        /// Generates a default encryption key if none is provided
        /// </summary>
        private string GenerateDefaultEncryptionKey()
        {
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }

        /// <summary>
        /// Encryption algorithm configuration
        /// </summary>
        public string Algorithm => _configuration["Encryption:Algorithm"] ?? "AES-256-CBC";

        /// <summary>
        /// Maximum document size allowed for encryption
        /// </summary>
        public long MaxDocumentSizeBytes => 
            long.TryParse(_configuration["Encryption:MaxDocumentSizeBytes"], out long size) 
                ? size 
                : 10 * 1024 * 1024; // Default 10MB
    }
}
