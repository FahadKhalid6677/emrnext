using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Net;

namespace EMRNext.API.Middleware
{
    public class SecurityEnhancementMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SecurityEnhancementOptions _options;

        public SecurityEnhancementMiddleware(RequestDelegate next, SecurityEnhancementOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Rate limiting check
            if (!await CheckRateLimit(context))
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            // Add security headers
            AddSecurityHeaders(context);

            // Input validation
            if (!ValidateInput(context))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Invalid input detected.");
                return;
            }

            await _next(context);
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;

            // HIPAA Security Headers
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            headers["Content-Security-Policy"] = "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "media-src 'self'; " +
                "object-src 'none'; " +
                "frame-src 'none'; " +
                "worker-src 'self'; " +
                "frame-ancestors 'none'; " +
                "form-action 'self'; " +
                "base-uri 'self'; " +
                "manifest-src 'self'";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Feature-Policy"] = "camera 'none'; " +
                "microphone 'none'; " +
                "geolocation 'none'; " +
                "payment 'none'";
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";

            // Remove potentially dangerous headers
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            headers.Remove("X-AspNet-Version");
        }

        private async Task<bool> CheckRateLimit(HttpContext context)
        {
            var key = context.Request.Headers["X-API-Key"].ToString();
            if (string.IsNullOrEmpty(key))
            {
                key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }

            // Implement rate limiting logic here
            return await Task.FromResult(true); // Placeholder
        }

        private bool ValidateInput(HttpContext context)
        {
            // Check for common injection patterns
            var input = context.Request.QueryString.Value;
            if (input != null)
            {
                var dangerous = new[]
                {
                    @"<script",
                    @"javascript:",
                    @"vbscript:",
                    @"onload=",
                    @"onerror=",
                    @"SELECT.*FROM",
                    @"DELETE.*FROM",
                    @"INSERT.*INTO",
                    @"DROP.*TABLE",
                    @"UNION.*SELECT"
                };

                foreach (var pattern in dangerous)
                {
                    if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class SecurityEnhancementOptions
    {
        public int MaxRequestsPerMinute { get; set; } = 60;
        public bool EnableStrictValidation { get; set; } = true;
        public List<string> AllowedOrigins { get; set; } = new List<string>();
    }
}
