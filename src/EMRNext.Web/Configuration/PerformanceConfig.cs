using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using System.Linq;

namespace EMRNext.Web.Configuration
{
    public static class PerformanceConfig
    {
        public static IServiceCollection AddPerformanceConfiguration(this IServiceCollection services)
        {
            // Add Response Compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "image/svg+xml", "application/json" });
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            // Add Output Caching
            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(builder =>
                    builder.Expire(TimeSpan.FromMinutes(5)));
                
                // Cache static assets for longer
                options.AddPolicy("StaticFiles", builder =>
                    builder.Expire(TimeSpan.FromDays(30)));
                
                // Cache API responses
                options.AddPolicy("ApiEndpoints", builder =>
                    builder.Expire(TimeSpan.FromMinutes(1))
                           .SetVaryByHeader("Accept", "Accept-Encoding")
                           .Tag("api"));
            });

            return services;
        }

        public static IApplicationBuilder UsePerformanceConfiguration(this IApplicationBuilder app)
        {
            // Enable response compression
            app.UseResponseCompression();

            // Enable output caching
            app.UseOutputCache();

            // Configure static file caching
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var headers = ctx.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(30)
                    };
                }
            });

            return app;
        }
    }
}
