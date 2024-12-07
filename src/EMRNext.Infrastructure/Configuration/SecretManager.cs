using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace EMRNext.Infrastructure.Configuration
{
    public class SecretManager : ISecretManager
    {
        private readonly SecretClient _secretClient;
        private readonly IConfiguration _configuration;

        public SecretManager(IConfiguration configuration)
        {
            _configuration = configuration;
            
            var keyVaultUri = _configuration["KeyVault:Uri"];
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                _secretClient = new SecretClient(
                    new Uri(keyVaultUri),
                    new DefaultAzureCredential());
            }
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            // First check environment variables
            var envValue = Environment.GetEnvironmentVariable($"EMRNEXT_{secretName.ToUpper()}");
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }

            // Then check configuration
            var configValue = _configuration[secretName];
            if (!string.IsNullOrEmpty(configValue))
            {
                return configValue;
            }

            // Finally, check Key Vault if available
            if (_secretClient != null)
            {
                try
                {
                    var secret = await _secretClient.GetSecretAsync(secretName);
                    return secret.Value.Value;
                }
                catch
                {
                    // Log error but don't expose Key Vault errors
                    return null;
                }
            }

            return null;
        }

        public async Task SetSecretAsync(string secretName, string secretValue)
        {
            if (_secretClient != null)
            {
                await _secretClient.SetSecretAsync(secretName, secretValue);
            }
            else
            {
                throw new InvalidOperationException("Key Vault is not configured");
            }
        }
    }
}
