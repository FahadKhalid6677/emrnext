using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Infrastructure.Configuration
{
    public class ServiceConfigurationManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceConfigurationManager> _logger;
        private readonly ConcurrentDictionary<string, object> _cachedConfigurations;

        public ServiceConfigurationManager(
            IConfiguration configuration, 
            ILogger<ServiceConfigurationManager> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _cachedConfigurations = new ConcurrentDictionary<string, object>();
        }

        // Get configuration for a specific service
        public T GetServiceConfiguration<T>(string serviceName) where T : class, new()
        {
            return _cachedConfigurations.GetOrAdd(serviceName, key =>
            {
                try
                {
                    var config = new T();
                    _configuration.GetSection($"Services:{serviceName}").Bind(config);
                    
                    _logger.LogInformation(
                        "Loaded configuration for service: {ServiceName}", 
                        serviceName
                    );

                    return config;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex, 
                        "Error loading configuration for service: {ServiceName}", 
                        serviceName
                    );
                    return new T();
                }
            }) as T;
        }

        // Update configuration dynamically
        public void UpdateServiceConfiguration<T>(string serviceName, T configuration) where T : class
        {
            try
            {
                _cachedConfigurations.AddOrUpdate(
                    serviceName, 
                    configuration, 
                    (key, oldValue) => configuration
                );

                _logger.LogInformation(
                    "Updated configuration for service: {ServiceName}", 
                    serviceName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error updating configuration for service: {ServiceName}", 
                    serviceName
                );
            }
        }

        // Service Configuration Model
        public class ServiceConfiguration
        {
            public string BaseUrl { get; set; }
            public int Port { get; set; }
            public string Version { get; set; }
            public Dictionary<string, string> ConnectionStrings { get; set; }
            public Dictionary<string, object> AdditionalSettings { get; set; }
        }

        // Environment-specific configuration
        public class EnvironmentConfig
        {
            public string Environment { get; set; }
            public bool IsDevelopment => Environment == "Development";
            public bool IsProduction => Environment == "Production";
            public bool IsStagging => Environment == "Staging";
        }

        // Configuration validation
        public bool ValidateServiceConfiguration<T>(T configuration) where T : class
        {
            // Implement custom validation logic
            if (configuration == null)
            {
                _logger.LogWarning("Configuration is null");
                return false;
            }

            // Add specific validation rules
            return true;
        }

        // Get current environment
        public EnvironmentConfig GetEnvironmentConfiguration()
        {
            return new EnvironmentConfig
            {
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"
            };
        }
    }
}
