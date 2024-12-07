using Microsoft.Extensions.Configuration;
using System;

namespace EMRNext.Infrastructure.Configuration
{
    public static class EnvironmentConfigurationHelper
    {
        public static string GetRequiredEnvironmentVariable(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Required environment variable '{variableName}' is not set.");
            }

            return value;
        }

        public static string GetConnectionString(IConfiguration configuration, string connectionStringName)
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{connectionStringName}' is not configured.");
            }

            return connectionString;
        }
    }
}
