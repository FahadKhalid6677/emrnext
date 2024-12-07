using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EMRNext.Web.Configuration
{
    public static class SecurityHeadersConfig
    {
        public static IServiceCollection AddSecurityHeadersConfiguration(this IServiceCollection services)
        {
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            return services;
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                // Content Security Policy
                context.Response.Headers.Add("Content-Security-Policy", 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self' https://fonts.gstatic.com; " +
                    "connect-src 'self' https://api.emrnext.com; " +
                    "frame-ancestors 'none'; " +
                    "form-action 'self';");

                // X-Content-Type-Options
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                // X-Frame-Options
                context.Response.Headers.Add("X-Frame-Options", "DENY");

                // X-XSS-Protection
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

                // Referrer-Policy
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

                // Permissions-Policy
                context.Response.Headers.Add("Permissions-Policy",
                    "accelerometer=(), " +
                    "camera=(), " +
                    "geolocation=(), " +
                    "gyroscope=(), " +
                    "magnetometer=(), " +
                    "microphone=(), " +
                    "payment=(), " +
                    "usb=()");

                // Strict-Transport-Security (HSTS)
                context.Response.Headers.Add("Strict-Transport-Security", 
                    "max-age=31536000; includeSubDomains; preload");

                // Cache-Control
                if (!context.Response.Headers.ContainsKey("Cache-Control"))
                {
                    context.Response.Headers.Add("Cache-Control", "no-store, max-age=0");
                }

                // Clear potentially sensitive headers
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-Powered-By");

                await next();
            });

            return app;
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomSecurityHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }

    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers for API endpoints
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.OnStarting(() =>
                {
                    // API-specific security headers
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Add("X-Frame-Options", "DENY");
                    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                    
                    // Add CORS headers if needed
                    if (context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                    {
                        context.Response.Headers.Add("Vary", "Origin");
                    }

                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }
    }
}
