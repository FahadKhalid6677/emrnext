using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Versioning;
using System.Linq;

namespace EMRNext.Web.Configuration
{
    public static class ApiVersioningConfig
    {
        public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                // Specify the default API version
                options.DefaultApiVersion = new ApiVersion(1, 0);
                
                // Assume default version when version is not specified
                options.AssumeDefaultVersionWhenUnspecified = true;
                
                // Report available API versions in response headers
                options.ReportApiVersions = true;
                
                // Use version header for versioning
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("X-Version"),
                    new QueryStringApiVersionReader("api-version"),
                    new UrlSegmentApiVersionReader()
                );
            });

            services.AddVersionedApiExplorer(options =>
            {
                // Format version as 'v'major[.minor][-status]
                options.GroupNameFormat = "'v'VVV";
                
                // Substitute version in URL template
                options.SubstituteApiVersionInUrl = true;
            });

            services.ConfigureOptions<ConfigureSwaggerOptions>();

            return services;
        }
    }

    public static class ApiVersioningExtensions
    {
        public static string GetFormattedApiVersion(this ApiVersion apiVersion)
        {
            return $"v{apiVersion.ToString()}";
        }

        public static bool IsPrerelease(this ApiVersion apiVersion)
        {
            return apiVersion.Status?.ToLower() == "alpha" || 
                   apiVersion.Status?.ToLower() == "beta" || 
                   apiVersion.Status?.ToLower() == "rc";
        }

        public static bool IsDeprecated(this ApiVersion apiVersion, ApiVersion currentVersion)
        {
            return apiVersion.MajorVersion < currentVersion.MajorVersion ||
                   (apiVersion.MajorVersion == currentVersion.MajorVersion && 
                    apiVersion.MinorVersion < currentVersion.MinorVersion - 1);
        }

        public static string GetVersionStatus(this ApiVersion apiVersion)
        {
            if (string.IsNullOrEmpty(apiVersion.Status))
                return "Stable";

            return apiVersion.Status.First().ToString().ToUpper() + 
                   apiVersion.Status.Substring(1).ToLower();
        }
    }
}
