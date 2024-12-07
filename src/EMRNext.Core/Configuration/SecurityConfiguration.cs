using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EMRNext.Core.Security;

namespace EMRNext.Core.Configuration
{
    public static class SecurityConfiguration
    {
        public static IServiceCollection AddAdvancedSecurity(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Configure Authorization Service
            services.Configure<AdvancedAuthorizationService.SecurityOptions>(
                configuration.GetSection("SecurityOptions")
            );
            services.AddScoped<AdvancedAuthorizationService>();

            // Configure Data Protection Service
            services.Configure<DataProtectionService.EncryptionOptions>(
                configuration.GetSection("EncryptionOptions")
            );
            services.AddScoped<DataProtectionService>();

            // Add Additional Security Middleware
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Configure Security Headers
            services.AddHsts(options =>
            {
                options.Seconds = 31536000; // 1 year
                options.IncludeSubDomains = true;
                options.Preload = true;
            });

            // Enable Content Security Policy
            services.AddCsp(options =>
            {
                options.BlockAllMixedContent();
                options.StyleSources(s => s
                    .Self()
                    .UnsafeInline()
                );
                options.ScriptSources(s => s
                    .Self()
                    .UnsafeInline()
                );
            });

            return services;
        }

        // Security Middleware Configuration
        public static IApplicationBuilder UseAdvancedSecurityMiddleware(
            this IApplicationBuilder app)
        {
            // Add custom security middleware
            app.UseMiddleware<SecurityHeaderMiddleware>();
            app.UseMiddleware<AuditLoggingMiddleware>();
            app.UseMiddleware<RequestValidationMiddleware>();

            return app;
        }

        // Security Header Middleware
        public class SecurityHeaderMiddleware
        {
            private readonly RequestDelegate _next;

            public SecurityHeaderMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                // Add security headers
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

                await _next(context);
            }
        }

        // Audit Logging Middleware
        public class AuditLoggingMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<AuditLoggingMiddleware> _logger;

            public AuditLoggingMiddleware(
                RequestDelegate next, 
                ILogger<AuditLoggingMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                // Log request details
                _logger.LogInformation(
                    $"Request: {context.Request.Method} {context.Request.Path}"
                );

                await _next(context);
            }
        }

        // Request Validation Middleware
        public class RequestValidationMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly AdvancedAuthorizationService _authService;

            public RequestValidationMiddleware(
                RequestDelegate next, 
                AdvancedAuthorizationService authService)
            {
                _next = next;
                _authService = authService;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                // Validate request
                var authContext = new AdvancedAuthorizationService.AuthorizationContext
                {
                    User = context.User,
                    RequestedAction = context.Request.Method,
                    ResourceType = MapRequestToResourceType(context.Request.Path)
                };

                var isAuthorized = await _authService.AuthorizeAsync(authContext);

                if (!isAuthorized)
                {
                    context.Response.StatusCode = 403; // Forbidden
                    await context.Response.WriteAsync("Access Denied");
                    return;
                }

                await _next(context);
            }

            // Map request path to resource type
            private AdvancedAuthorizationService.ResourceType MapRequestToResourceType(
                PathString path)
            {
                return path.Value.ToLower() switch
                {
                    string p when p.Contains("/patient") => 
                        AdvancedAuthorizationService.ResourceType.Patient,
                    string p when p.Contains("/medical-record") => 
                        AdvancedAuthorizationService.ResourceType.MedicalRecord,
                    string p when p.Contains("/prescription") => 
                        AdvancedAuthorizationService.ResourceType.Prescription,
                    string p when p.Contains("/billing") => 
                        AdvancedAuthorizationService.ResourceType.Billing,
                    _ => AdvancedAuthorizationService.ResourceType.SystemConfiguration
                };
            }
        }
    }
}
