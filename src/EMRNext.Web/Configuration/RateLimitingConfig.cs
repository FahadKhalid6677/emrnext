using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace EMRNext.Web.Configuration
{
    public static class RateLimitingConfig
    {
        public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    // Get client identifier (IP or API key)
                    var clientId = GetClientIdentifier(context.Request);

                    // Define rate limit based on client type
                    var limit = IsPrivilegedClient(context.Request) ? 1000 : 100;

                    return RateLimitPartition.GetFixedWindowLimiter(clientId, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = limit,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // Add specific endpoint policies
                options.AddPolicy("protected_api", context =>
                {
                    var clientId = GetClientIdentifier(context.Request);
                    return RateLimitPartition.GetFixedWindowLimiter(clientId, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 50,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // Configure on-rejected behavior
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "Too many requests",
                            retryAfter = retryAfter.TotalSeconds,
                            message = $"Please try again after {retryAfter.TotalSeconds} seconds"
                        }, token);
                    }
                };
            });

            return services;
        }

        private static string GetClientIdentifier(HttpRequest request)
        {
            // Try to get API key first
            var apiKey = request.Headers["X-API-Key"].ToString();
            if (!string.IsNullOrEmpty(apiKey))
                return $"api_key_{apiKey}";

            // Fall back to IP address
            var ip = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return $"ip_{ip}";
        }

        private static bool IsPrivilegedClient(HttpRequest request)
        {
            var apiKey = request.Headers["X-API-Key"].ToString();
            // Implement your logic to determine if this is a privileged client
            // For example, check against a list of premium API keys
            return !string.IsNullOrEmpty(apiKey) && IsPremiumApiKey(apiKey);
        }

        private static bool IsPremiumApiKey(string apiKey)
        {
            // Implement your logic to validate premium API keys
            // This could involve checking against a database or cache
            throw new NotImplementedException();
        }
    }

    public static class RateLimitingMiddleware
    {
        public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder app)
        {
            app.UseRateLimiter();

            // Add custom middleware for rate limit monitoring
            app.Use(async (context, next) =>
            {
                var endpoint = context.GetEndpoint();
                if (endpoint?.Metadata?.GetMetadata<EnableRateLimitingAttribute>() != null)
                {
                    // Log rate limit usage
                    await LogRateLimitUsage(context);
                }
                await next();
            });

            return app;
        }

        private static async Task LogRateLimitUsage(HttpContext context)
        {
            // Implement rate limit usage logging
            // This could involve metrics collection, monitoring, etc.
            await Task.CompletedTask;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class EnableRateLimitingAttribute : Attribute
    {
        public string PolicyName { get; }

        public EnableRateLimitingAttribute(string policyName = "default")
        {
            PolicyName = policyName;
        }
    }
}
