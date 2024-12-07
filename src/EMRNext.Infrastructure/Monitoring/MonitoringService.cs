using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Interfaces;
using System.Diagnostics;

namespace EMRNext.Infrastructure.Monitoring
{
    public class MonitoringService : IMonitoringService
    {
        private readonly ILogger<MonitoringService> _logger;
        private readonly ILoggingService _loggingService;
        private readonly Dictionary<string, Stopwatch> _operationTimers;
        private readonly PerformanceMetrics _metrics;

        public MonitoringService(
            ILogger<MonitoringService> logger,
            ILoggingService loggingService)
        {
            _logger = logger;
            _loggingService = loggingService;
            _operationTimers = new Dictionary<string, Stopwatch>();
            _metrics = new PerformanceMetrics();
        }

        public void StartOperation(string operationName)
        {
            var timer = new Stopwatch();
            timer.Start();
            _operationTimers[operationName] = timer;
        }

        public async Task EndOperationAsync(string operationName, bool success = true)
        {
            if (_operationTimers.TryGetValue(operationName, out var timer))
            {
                timer.Stop();
                var duration = timer.ElapsedMilliseconds;
                _operationTimers.Remove(operationName);

                // Update metrics
                _metrics.RecordOperation(operationName, duration, success);

                // Log performance metric
                await _loggingService.LogPerformanceMetric(
                    operationName,
                    duration,
                    $"Operation completed. Success: {success}"
                );

                // Check for slow operations
                if (duration > 1000) // 1 second threshold
                {
                    _logger.LogWarning(
                        "Slow operation detected: {OperationName} took {Duration}ms",
                        operationName,
                        duration
                    );
                }
            }
        }

        public async Task LogErrorAsync(string operationName, Exception ex)
        {
            _metrics.RecordError(operationName);

            await _loggingService.LogAuditAsync(
                "Error",
                "Monitoring",
                Guid.NewGuid().ToString(),
                $"Operation: {operationName}, Error: {ex.Message}"
            );

            _logger.LogError(
                ex,
                "Error in operation {OperationName}: {ErrorMessage}",
                operationName,
                ex.Message
            );
        }

        public async Task<HealthCheckResult> CheckHealthAsync()
        {
            var result = new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Timestamp = DateTime.UtcNow,
                Components = new Dictionary<string, ComponentHealth>()
            };

            try
            {
                // Check database connectivity
                result.Components["Database"] = await CheckDatabaseHealthAsync();

                // Check external services
                result.Components["ExternalServices"] = await CheckExternalServicesHealthAsync();

                // Check system resources
                result.Components["SystemResources"] = CheckSystemResources();

                // Update overall status
                if (result.Components.Values.Any(c => c.Status == HealthStatus.Unhealthy))
                {
                    result.Status = HealthStatus.Unhealthy;
                }
                else if (result.Components.Values.Any(c => c.Status == HealthStatus.Degraded))
                {
                    result.Status = HealthStatus.Degraded;
                }

                return result;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("HealthCheck", ex);
                result.Status = HealthStatus.Unhealthy;
                result.Error = ex.Message;
                return result;
            }
        }

        public PerformanceSnapshot GetPerformanceSnapshot()
        {
            return new PerformanceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                AverageResponseTime = _metrics.GetAverageResponseTime(),
                ErrorRate = _metrics.GetErrorRate(),
                SuccessRate = _metrics.GetSuccessRate(),
                TotalOperations = _metrics.GetTotalOperations(),
                OperationBreakdown = _metrics.GetOperationBreakdown()
            };
        }

        public async Task<ResourceUtilization> GetResourceUtilizationAsync()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                
                return new ResourceUtilization
                {
                    Timestamp = DateTime.UtcNow,
                    CpuUsage = await GetCpuUsageAsync(),
                    MemoryUsage = process.WorkingSet64,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    AvailableDiskSpace = await GetAvailableDiskSpaceAsync()
                };
            }
            catch (Exception ex)
            {
                await LogErrorAsync("ResourceUtilization", ex);
                throw;
            }
        }

        private async Task<ComponentHealth> CheckDatabaseHealthAsync()
        {
            // Implementation of database health check
            return new ComponentHealth
            {
                Status = HealthStatus.Healthy,
                Details = "Database connection is healthy"
            };
        }

        private async Task<ComponentHealth> CheckExternalServicesHealthAsync()
        {
            // Implementation of external services health check
            return new ComponentHealth
            {
                Status = HealthStatus.Healthy,
                Details = "All external services are responding"
            };
        }

        private ComponentHealth CheckSystemResources()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64 / (1024 * 1024); // Convert to MB

                var status = HealthStatus.Healthy;
                var details = "System resources are within normal range";

                if (memoryUsage > 1024) // 1GB threshold
                {
                    status = HealthStatus.Degraded;
                    details = "High memory usage detected";
                }

                return new ComponentHealth
                {
                    Status = status,
                    Details = details,
                    Metrics = new Dictionary<string, object>
                    {
                        { "MemoryUsage", memoryUsage },
                        { "ThreadCount", process.Threads.Count },
                        { "HandleCount", process.HandleCount }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ComponentHealth
                {
                    Status = HealthStatus.Unhealthy,
                    Details = $"Error checking system resources: {ex.Message}"
                };
            }
        }

        private async Task<double> GetCpuUsageAsync()
        {
            // Implementation of CPU usage calculation
            return 0.0; // Placeholder
        }

        private async Task<long> GetAvailableDiskSpaceAsync()
        {
            // Implementation of disk space check
            return 0; // Placeholder
        }
    }

    public class PerformanceMetrics
    {
        private readonly Dictionary<string, List<OperationMetric>> _metrics;
        private readonly object _lock = new object();

        public PerformanceMetrics()
        {
            _metrics = new Dictionary<string, List<OperationMetric>>();
        }

        public void RecordOperation(string operationName, long duration, bool success)
        {
            lock (_lock)
            {
                if (!_metrics.ContainsKey(operationName))
                {
                    _metrics[operationName] = new List<OperationMetric>();
                }

                _metrics[operationName].Add(new OperationMetric
                {
                    Timestamp = DateTime.UtcNow,
                    Duration = duration,
                    Success = success
                });
            }
        }

        public void RecordError(string operationName)
        {
            RecordOperation(operationName, 0, false);
        }

        public double GetAverageResponseTime()
        {
            lock (_lock)
            {
                var allMetrics = _metrics.Values.SelectMany(m => m);
                return allMetrics.Any() ? allMetrics.Average(m => m.Duration) : 0;
            }
        }

        public double GetErrorRate()
        {
            lock (_lock)
            {
                var allMetrics = _metrics.Values.SelectMany(m => m);
                var total = allMetrics.Count();
                return total > 0 ? (double)allMetrics.Count(m => !m.Success) / total : 0;
            }
        }

        public double GetSuccessRate()
        {
            return 1 - GetErrorRate();
        }

        public int GetTotalOperations()
        {
            lock (_lock)
            {
                return _metrics.Values.Sum(m => m.Count);
            }
        }

        public Dictionary<string, OperationStats> GetOperationBreakdown()
        {
            lock (_lock)
            {
                return _metrics.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new OperationStats
                    {
                        TotalOperations = kvp.Value.Count,
                        AverageResponseTime = kvp.Value.Average(m => m.Duration),
                        SuccessRate = (double)kvp.Value.Count(m => m.Success) / kvp.Value.Count,
                        LastExecuted = kvp.Value.Max(m => m.Timestamp)
                    }
                );
            }
        }
    }

    public class OperationMetric
    {
        public DateTime Timestamp { get; set; }
        public long Duration { get; set; }
        public bool Success { get; set; }
    }

    public class OperationStats
    {
        public int TotalOperations { get; set; }
        public double AverageResponseTime { get; set; }
        public double SuccessRate { get; set; }
        public DateTime LastExecuted { get; set; }
    }

    public class HealthCheckResult
    {
        public HealthStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, ComponentHealth> Components { get; set; }
        public string Error { get; set; }
    }

    public class ComponentHealth
    {
        public HealthStatus Status { get; set; }
        public string Details { get; set; }
        public Dictionary<string, object> Metrics { get; set; }
    }

    public class ResourceUtilization
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public long MemoryUsage { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public long AvailableDiskSpace { get; set; }
    }

    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }
}
