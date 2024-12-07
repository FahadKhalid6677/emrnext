using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Interfaces;
using System.Security.Claims;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EMRNext.Infrastructure.Security
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggingService _loggingService;
        private readonly SecurityOptions _options;

        public SecurityMiddleware(
            RequestDelegate next,
            ILoggingService loggingService,
            SecurityOptions options)
        {
            _next = next;
            _loggingService = loggingService;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Add security headers
                AddSecurityHeaders(context);

                // Log request
                await LogRequestAsync(context);

                // Enable request body rewind
                context.Request.EnableBuffering();

                // Create a response body stream wrapper
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Continue processing
                await _next(context);

                // Log response
                await LogResponseAsync(context, responseBody, originalBodyStream, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await LogErrorAsync(context, ex, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var headers = context.Response.Headers;

            // HIPAA Security Headers
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["X-XSS-Protection"] = "1; mode=block";
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            headers["Content-Security-Policy"] = BuildContentSecurityPolicy();
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Feature-Policy"] = BuildFeaturePolicy();
            headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "0";

            // Remove potentially dangerous headers
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            headers.Remove("X-AspNet-Version");
        }

        private string BuildContentSecurityPolicy()
        {
            return "default-src 'self'; " +
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
        }

        private string BuildFeaturePolicy()
        {
            return "camera 'none'; " +
                   "microphone 'none'; " +
                   "geolocation 'none'; " +
                   "payment 'none'; " +
                   "usb 'none'; " +
                   "vr 'none'; " +
                   "accelerometer 'none'; " +
                   "ambient-light-sensor 'none'; " +
                   "autoplay 'none'; " +
                   "encrypted-media 'none'; " +
                   "gyroscope 'none'; " +
                   "magnetometer 'none'; " +
                   "midi 'none'; " +
                   "picture-in-picture 'none'; " +
                   "speaker 'none'; " +
                   "sync-xhr 'none'; " +
                   "fullscreen 'self'";
        }

        private async Task LogRequestAsync(HttpContext context)
        {
            var request = context.Request;
            var requestBody = string.Empty;

            // Read and log request body for specific content types
            if (IsLoggableContentType(request.ContentType))
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                // Sanitize sensitive data
                requestBody = SanitizeSensitiveData(requestBody);
            }

            var logData = new
            {
                Timestamp = DateTime.UtcNow,
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                RequestMethod = request.Method,
                RequestPath = request.Path,
                RequestQueryString = request.QueryString.ToString(),
                RequestHeaders = GetSanitizedHeaders(request.Headers),
                RequestBody = requestBody,
                UserIdentity = context.User?.Identity?.Name,
                UserIp = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = request.Headers["User-Agent"].ToString()
            };

            await _loggingService.LogAuditAsync(
                "HttpRequest",
                "Security",
                context.TraceIdentifier,
                JsonConvert.SerializeObject(logData)
            );
        }

        private async Task LogResponseAsync(HttpContext context, MemoryStream responseBody, Stream originalBodyStream, long elapsedMs)
        {
            responseBody.Position = 0;
            var responseContent = string.Empty;

            if (IsLoggableContentType(context.Response.ContentType))
            {
                using var reader = new StreamReader(responseBody, Encoding.UTF8);
                responseContent = await reader.ReadToEndAsync();
                responseContent = SanitizeSensitiveData(responseContent);
            }

            var logData = new
            {
                Timestamp = DateTime.UtcNow,
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                ResponseStatusCode = context.Response.StatusCode,
                ResponseHeaders = GetSanitizedHeaders(context.Response.Headers),
                ResponseBody = responseContent,
                ElapsedMilliseconds = elapsedMs
            };

            await _loggingService.LogAuditAsync(
                "HttpResponse",
                "Security",
                context.TraceIdentifier,
                JsonConvert.SerializeObject(logData)
            );

            // Copy the response to the original stream
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task LogErrorAsync(HttpContext context, Exception ex, long elapsedMs)
        {
            var logData = new
            {
                Timestamp = DateTime.UtcNow,
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                Exception = new
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source
                },
                ElapsedMilliseconds = elapsedMs
            };

            await _loggingService.LogAuditAsync(
                "HttpError",
                "Security",
                context.TraceIdentifier,
                JsonConvert.SerializeObject(logData)
            );
        }

        private bool IsLoggableContentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType)) return false;

            contentType = contentType.ToLower();
            return contentType.Contains("application/json") ||
                   contentType.Contains("application/xml") ||
                   contentType.Contains("text/plain") ||
                   contentType.Contains("text/xml");
        }

        private IDictionary<string, string> GetSanitizedHeaders(IHeaderDictionary headers)
        {
            var sanitizedHeaders = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                if (!_options.SensitiveHeaders.Contains(header.Key.ToLower()))
                {
                    sanitizedHeaders[header.Key] = header.Value.ToString();
                }
            }
            return sanitizedHeaders;
        }

        private string SanitizeSensitiveData(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;

            foreach (var pattern in _options.SensitiveDataPatterns)
            {
                content = pattern.Regex.Replace(content, pattern.Replacement);
            }

            return content;
        }
    }

    public class SecurityOptions
    {
        public HashSet<string> SensitiveHeaders { get; set; } = new HashSet<string>
        {
            "authorization",
            "cookie",
            "x-api-key",
            "x-csrf-token"
        };

        public List<SensitiveDataPattern> SensitiveDataPatterns { get; set; } = new List<SensitiveDataPattern>
        {
            new SensitiveDataPattern(@"\b\d{3}-\d{2}-\d{4}\b", "***-**-****"), // SSN
            new SensitiveDataPattern(@"\b\d{16}\b", "****-****-****-****"), // Credit Card
            new SensitiveDataPattern(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", "[EMAIL]"), // Email
            new SensitiveDataPattern(@"\b\d{10}\b", "(***)***-****"), // Phone Number
            new SensitiveDataPattern(@"\b\d{5}(-\d{4})?\b", "*****"), // Zip Code
            new SensitiveDataPattern(@"password[\"']?\s*[:=]\s*[\"']?[^\"',}\s]+[\"']?", "password: [REDACTED]") // Passwords
        };
    }

    public class SensitiveDataPattern
    {
        public System.Text.RegularExpressions.Regex Regex { get; }
        public string Replacement { get; }

        public SensitiveDataPattern(string pattern, string replacement)
        {
            Regex = new System.Text.RegularExpressions.Regex(pattern);
            Replacement = replacement;
        }
    }
}
