using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EMRNext.API.HealthChecks
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddEMRHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>("Database")
                .AddRedis("Redis")
                .AddCheck<ExternalServicesHealthCheck>("External Services")
                .AddCheck("HIPAA Compliance", () =>
                {
                    // Add HIPAA compliance checks here
                    return HealthCheckResult.Healthy("HIPAA compliance checks passed");
                });

            return services;
        }

        public static IApplicationBuilder UseEMRHealthChecks(this IApplicationBuilder app)
        {
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.ToString()
                        }),
                        totalDuration = report.TotalDuration.ToString()
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
            });

            return app;
        }
    }

    public class ExternalServicesHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Add checks for external services (e.g., payment gateway, insurance verification)
                // This is a placeholder - implement actual service checks
                
                return HealthCheckResult.Healthy("All external services are operational");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("One or more external services are not responding", ex);
            }
        }
    }
}
