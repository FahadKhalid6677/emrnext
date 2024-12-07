using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace EMRNext.Core.Infrastructure.ServiceDiscovery
{
    // Service Registration and Discovery Model
    public class ServiceRegistration
    {
        // Thread-safe service registry
        private static ConcurrentDictionary<string, ServiceInstance> _serviceRegistry 
            = new ConcurrentDictionary<string, ServiceInstance>();

        // Register a service instance
        public static void RegisterService(ServiceInstance serviceInstance)
        {
            if (serviceInstance == null)
                throw new ArgumentNullException(nameof(serviceInstance));

            // Generate unique service key
            var serviceKey = GenerateServiceKey(serviceInstance);
            
            // Add or update service registration
            _serviceRegistry.AddOrUpdate(
                serviceKey, 
                serviceInstance, 
                (key, oldValue) => serviceInstance
            );
        }

        // Unregister a service instance
        public static bool UnregisterService(string serviceId)
        {
            return _serviceRegistry.TryRemove(serviceId, out _);
        }

        // Find service instances by name
        public static List<ServiceInstance> FindServiceInstances(string serviceName)
        {
            return _serviceRegistry
                .Where(s => s.Value.ServiceName == serviceName)
                .Select(s => s.Value)
                .ToList();
        }

        // Generate a unique service key
        private static string GenerateServiceKey(ServiceInstance serviceInstance)
        {
            return $"{serviceInstance.ServiceName}_{serviceInstance.InstanceId}";
        }

        // Heartbeat mechanism to check service health
        public static async Task PerformHealthCheck()
        {
            var unhealthyServices = new List<string>();

            foreach (var service in _serviceRegistry)
            {
                try
                {
                    // Perform health check
                    bool isHealthy = await CheckServiceHealth(service.Value);

                    if (!isHealthy)
                    {
                        unhealthyServices.Add(service.Key);
                    }
                }
                catch
                {
                    unhealthyServices.Add(service.Key);
                }
            }

            // Remove unhealthy services
            foreach (var serviceKey in unhealthyServices)
            {
                _serviceRegistry.TryRemove(serviceKey, out _);
            }
        }

        // Actual health check implementation
        private static async Task<bool> CheckServiceHealth(ServiceInstance serviceInstance)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"{serviceInstance.BaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    // Service Instance Details
    public class ServiceInstance
    {
        public string InstanceId { get; set; } = Guid.NewGuid().ToString();
        public string ServiceName { get; set; }
        public string BaseUrl { get; set; }
        public IPAddress IpAddress { get; set; }
        public int Port { get; set; }
        public ServiceStatus Status { get; set; } = ServiceStatus.Active;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    // Service Status Enum
    public enum ServiceStatus
    {
        Active,
        Inactive,
        Degraded
    }

    // Service Discovery Configuration
    public class ServiceDiscoveryConfig
    {
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(5);
        public int MaxRetries { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    }
}
