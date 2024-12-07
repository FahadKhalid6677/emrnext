using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace EMRNext.Infrastructure.Configuration
{
    public class EnvironmentVariableProvider
    {
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _environmentVariables;

        public EnvironmentVariableProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            _environmentVariables = new Dictionary<string, string>();
            LoadEnvironmentVariables();
        }

        private void LoadEnvironmentVariables()
        {
            foreach (var setting in _configuration.AsEnumerable())
            {
                if (setting.Key.StartsWith("EMRNEXT_", StringComparison.OrdinalIgnoreCase))
                {
                    _environmentVariables[setting.Key] = setting.Value;
                }
            }
        }

        public string GetValue(string key, string defaultValue = null)
        {
            // Try environment variable first
            var envKey = $"EMRNEXT_{key.ToUpper()}";
            var value = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            // Then try configuration
            if (_environmentVariables.TryGetValue(envKey, out string configValue))
            {
                return configValue;
            }

            // Finally, try direct configuration key
            value = _configuration[key];
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            var value = GetValue(key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool IsDevelopment()
        {
            return GetValue("ENVIRONMENT", "Production")
                .Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsDocker()
        {
            return GetValue("DOCKER_CONTAINER", "false")
                .Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public string GetConnectionString()
        {
            var provider = GetValue("DB_PROVIDER", "SqlServer");
            var host = GetValue("DB_HOST", "localhost");
            var port = GetValue("DB_PORT", "1433");
            var database = GetValue("DB_NAME", "EMRNextDb");
            var user = GetValue("DB_USER", "sa");
            var password = GetValue("DB_PASSWORD", "");

            return provider.ToLower() switch
            {
                "sqlite" => $"Data Source={database}.db",
                "postgresql" => $"Host={host};Port={port};Database={database};Username={user};Password={password}",
                _ => $"Server={host};Database={database};User Id={user};Password={password};TrustServerCertificate=true"
            };
        }
    }
}
