using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace EMRNext.API.Configuration
{
    public static class HealthCheckConfiguration
    {
        public static IServiceCollection AddComprehensiveHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHealthChecks()
                // Database health check
                .AddSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    name: "database",
                    tags: new[] { "db", "sql", "sqlserver" })
                
                // Redis health check
                .AddRedis(
                    configuration["Redis:Configuration"],
                    name: "redis",
                    tags: new[] { "cache", "redis" })
                
                // Custom health checks
                .AddCheck("DiskSpace", new DiskSpaceHealthCheck())
                .AddCheck("Memory", new MemoryHealthCheck())
                .AddCheck("CPU", new CpuHealthCheck());

            return services;
        }
    }

    public class DiskSpaceHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var driveInfo = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(Environment.CurrentDirectory));
            var freeSpace = driveInfo.AvailableFreeSpace / 1024 / 1024 / 1024; // Convert to GB

            if (freeSpace < 1) // Less than 1GB
            {
                return Task.FromResult(HealthCheckResult.Unhealthy($"Low disk space: {freeSpace}GB remaining"));
            }
            
            return Task.FromResult(HealthCheckResult.Healthy($"Disk space: {freeSpace}GB remaining"));
        }
    }

    public class MemoryHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var totalMemory = GC.GetTotalMemory(false) / 1024 / 1024; // Convert to MB
            
            if (totalMemory > 1024) // More than 1GB
            {
                return Task.FromResult(HealthCheckResult.Degraded($"High memory usage: {totalMemory}MB"));
            }
            
            return Task.FromResult(HealthCheckResult.Healthy($"Memory usage: {totalMemory}MB"));
        }
    }

    public class CpuHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpuUsage = process.TotalProcessorTime.TotalMilliseconds / 
                          (Environment.ProcessorCount * process.UpTime.TotalMilliseconds) * 100;

            if (cpuUsage > 80) // More than 80%
            {
                return Task.FromResult(HealthCheckResult.Degraded($"High CPU usage: {cpuUsage:F1}%"));
            }
            
            return Task.FromResult(HealthCheckResult.Healthy($"CPU usage: {cpuUsage:F1}%"));
        }
    }
}
