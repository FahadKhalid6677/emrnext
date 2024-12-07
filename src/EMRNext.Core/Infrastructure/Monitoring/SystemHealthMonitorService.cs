using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;
using EMRNext.Core.Infrastructure.Configuration;

namespace EMRNext.Core.Infrastructure.Monitoring
{
    /// <summary>
    /// Advanced system health and performance monitoring service
    /// </summary>
    public class SystemHealthMonitorService : BackgroundService
    {
        private readonly ILogger<SystemHealthMonitorService> _logger;
        private readonly SystemHealthConfiguration _configuration;
        private readonly ISystemMetricsRepository _metricsRepository;
        private readonly PerformanceMetricsCollector _performanceCollector;

        public SystemHealthMonitorService(
            ILogger<SystemHealthMonitorService> logger,
            IOptions<SystemHealthConfiguration> configuration,
            ISystemMetricsRepository metricsRepository)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _metricsRepository = metricsRepository;
            _performanceCollector = new PerformanceMetricsCollector();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try 
                {
                    var systemMetrics = await CollectSystemMetricsAsync();
                    await _metricsRepository.SaveSystemMetricsAsync(systemMetrics);

                    // Check for critical thresholds
                    await EvaluateSystemHealthAsync(systemMetrics);

                    await Task.Delay(
                        TimeSpan.FromMinutes(_configuration.MonitoringIntervalMinutes), 
                        stoppingToken
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during system health monitoring");
                    await Task.Delay(
                        TimeSpan.FromMinutes(5), 
                        stoppingToken
                    );
                }
            }
        }

        private async Task<SystemHealthMetrics> CollectSystemMetricsAsync()
        {
            var cpuUsage = _performanceCollector.GetCpuUsage();
            var memoryUsage = _performanceCollector.GetMemoryUsage();
            var diskUsage = _performanceCollector.GetDiskUsage();

            var metrics = new SystemHealthMetrics
            {
                Timestamp = DateTime.UtcNow,
                CpuUsagePercentage = cpuUsage,
                MemoryUsagePercentage = memoryUsage,
                DiskUsagePercentage = diskUsage,
                RuntimeVersion = Environment.Version.ToString(),
                OperatingSystem = RuntimeInformation.OSDescription,
                ProcessorCount = Environment.ProcessorCount,
                ApplicationUptime = Process.GetCurrentProcess().StartTime
            };

            // Collect application-specific metrics
            metrics.ActiveUserSessions = await _metricsRepository.GetActiveUserSessionsCountAsync();
            metrics.DatabaseConnectionPoolUtilization = await _metricsRepository.GetDatabaseConnectionPoolUtilizationAsync();

            return metrics;
        }

        private async Task EvaluateSystemHealthAsync(SystemHealthMetrics metrics)
        {
            var healthStatus = new SystemHealthStatus
            {
                Timestamp = metrics.Timestamp,
                OverallStatus = DetermineOverallHealthStatus(metrics)
            };

            // Log critical alerts
            if (healthStatus.OverallStatus == SystemStatus.Critical)
            {
                await NotifyCriticalSystemHealthAsync(metrics);
            }

            await _metricsRepository.SaveSystemHealthStatusAsync(healthStatus);
        }

        private SystemStatus DetermineOverallHealthStatus(SystemHealthMetrics metrics)
        {
            if (metrics.CpuUsagePercentage > _configuration.CpuCriticalThreshold ||
                metrics.MemoryUsagePercentage > _configuration.MemoryCriticalThreshold ||
                metrics.DiskUsagePercentage > _configuration.DiskCriticalThreshold)
            {
                return SystemStatus.Critical;
            }

            if (metrics.CpuUsagePercentage > _configuration.CpuWarningThreshold ||
                metrics.MemoryUsagePercentage > _configuration.MemoryWarningThreshold ||
                metrics.DiskUsagePercentage > _configuration.DiskWarningThreshold)
            {
                return SystemStatus.Warning;
            }

            return SystemStatus.Healthy;
        }

        private async Task NotifyCriticalSystemHealthAsync(SystemHealthMetrics metrics)
        {
            var alertMessage = $"CRITICAL SYSTEM HEALTH ALERT\n" +
                               $"Timestamp: {metrics.Timestamp}\n" +
                               $"CPU Usage: {metrics.CpuUsagePercentage:F2}%\n" +
                               $"Memory Usage: {metrics.MemoryUsagePercentage:F2}%\n" +
                               $"Disk Usage: {metrics.DiskUsagePercentage:F2}%";

            await _metricsRepository.SendSystemHealthAlertAsync(alertMessage);
            _logger.LogCritical(alertMessage);
        }

        /// <summary>
        /// Generate comprehensive system health report
        /// </summary>
        public async Task<SystemHealthReport> GenerateSystemHealthReportAsync()
        {
            var recentMetrics = await _metricsRepository.GetRecentSystemMetricsAsync();
            var historicalTrends = await _metricsRepository.GetSystemHealthTrendsAsync();

            return new SystemHealthReport
            {
                GeneratedAt = DateTime.UtcNow,
                RecentMetrics = recentMetrics,
                HealthTrends = historicalTrends,
                PerformanceRecommendations = GeneratePerformanceRecommendations(recentMetrics)
            };
        }

        private List<string> GeneratePerformanceRecommendations(List<SystemHealthMetrics> metrics)
        {
            var recommendations = new List<string>();

            // Example recommendations based on metrics
            if (metrics.Average(m => m.CpuUsagePercentage) > 80)
                recommendations.Add("Consider scaling up CPU resources");

            if (metrics.Average(m => m.MemoryUsagePercentage) > 85)
                recommendations.Add("Optimize memory usage and consider increasing memory allocation");

            if (metrics.Average(m => m.DiskUsagePercentage) > 90)
                recommendations.Add("Implement data archiving and disk cleanup strategies");

            return recommendations;
        }
    }

    /// <summary>
    /// Performance metrics collector using system diagnostics
    /// </summary>
    public class PerformanceMetricsCollector
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _memCounter;
        private PerformanceCounter _diskCounter;

        public PerformanceMetricsCollector()
        {
            InitializePerformanceCounters();
        }

        private void InitializePerformanceCounters()
        {
            try 
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
                _diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            }
            catch (Exception ex)
            {
                // Fallback to alternative measurement methods if performance counters are not available
                Console.WriteLine($"Performance counter initialization failed: {ex.Message}");
            }
        }

        public double GetCpuUsage()
        {
            return _cpuCounter?.NextValue() ?? 0;
        }

        public double GetMemoryUsage()
        {
            return _memCounter?.NextValue() ?? 0;
        }

        public double GetDiskUsage()
        {
            return _diskCounter?.NextValue() ?? 0;
        }
    }

    /// <summary>
    /// Represents comprehensive system health metrics
    /// </summary>
    public class SystemHealthMetrics
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsagePercentage { get; set; }
        public double MemoryUsagePercentage { get; set; }
        public double DiskUsagePercentage { get; set; }
        public string RuntimeVersion { get; set; }
        public string OperatingSystem { get; set; }
        public int ProcessorCount { get; set; }
        public DateTime ApplicationUptime { get; set; }
        public int ActiveUserSessions { get; set; }
        public double DatabaseConnectionPoolUtilization { get; set; }
    }

    /// <summary>
    /// Represents the overall system health status
    /// </summary>
    public class SystemHealthStatus
    {
        public DateTime Timestamp { get; set; }
        public SystemStatus OverallStatus { get; set; }
    }

    /// <summary>
    /// Comprehensive system health report
    /// </summary>
    public class SystemHealthReport
    {
        public DateTime GeneratedAt { get; set; }
        public List<SystemHealthMetrics> RecentMetrics { get; set; }
        public List<SystemHealthTrend> HealthTrends { get; set; }
        public List<string> PerformanceRecommendations { get; set; }
    }

    /// <summary>
    /// Represents historical system health trends
    /// </summary>
    public class SystemHealthTrend
    {
        public DateTime Period { get; set; }
        public double AverageCpuUsage { get; set; }
        public double AverageMemoryUsage { get; set; }
        public double AverageDiskUsage { get; set; }
    }

    /// <summary>
    /// System health status enumeration
    /// </summary>
    public enum SystemStatus
    {
        Healthy,
        Warning,
        Critical
    }

    /// <summary>
    /// Repository interface for system metrics
    /// </summary>
    public interface ISystemMetricsRepository
    {
        Task SaveSystemMetricsAsync(SystemHealthMetrics metrics);
        Task SaveSystemHealthStatusAsync(SystemHealthStatus status);
        Task<List<SystemHealthMetrics>> GetRecentSystemMetricsAsync();
        Task<List<SystemHealthTrend>> GetSystemHealthTrendsAsync();
        Task<int> GetActiveUserSessionsCountAsync();
        Task<double> GetDatabaseConnectionPoolUtilizationAsync();
        Task SendSystemHealthAlertAsync(string alertMessage);
    }
}
