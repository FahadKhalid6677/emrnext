using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace EMRNext.Infrastructure.Configuration
{
    public class ConfigurationValidator
    {
        private readonly IConfiguration _configuration;
        private readonly List<string> _requiredSettings;
        private readonly List<string> _missingSettings;

        public ConfigurationValidator(IConfiguration configuration)
        {
            _configuration = configuration;
            _requiredSettings = new List<string>
            {
                // Database settings
                "Database:Provider",
                "Database:ConnectionString",
                
                // JWT settings
                "Jwt:SecretKey",
                "Jwt:Issuer",
                "Jwt:Audience",
                
                // Application settings
                "EMRNEXT_ENVIRONMENT",
                "EMRNEXT_API_URL",
                "EMRNEXT_CLIENT_URL"
            };
            _missingSettings = new List<string>();
        }

        public bool Validate()
        {
            foreach (var setting in _requiredSettings)
            {
                if (string.IsNullOrEmpty(_configuration[setting]))
                {
                    _missingSettings.Add(setting);
                }
            }

            return !_missingSettings.Any();
        }

        public string GetValidationErrors()
        {
            if (!_missingSettings.Any())
            {
                return "Configuration validation passed successfully.";
            }

            return $"Missing required configuration settings: {string.Join(", ", _missingSettings)}";
        }

        public bool ValidateConnectionString()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                return false;
            }

            try
            {
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                return !string.IsNullOrEmpty(builder.InitialCatalog) && 
                       !string.IsNullOrEmpty(builder.DataSource);
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateJwtSettings()
        {
            var jwtSection = _configuration.GetSection("Jwt");
            return !string.IsNullOrEmpty(jwtSection["SecretKey"]) &&
                   !string.IsNullOrEmpty(jwtSection["Issuer"]) &&
                   !string.IsNullOrEmpty(jwtSection["Audience"]) &&
                   int.TryParse(jwtSection["ExpiryMinutes"], out _);
        }
    }
}
