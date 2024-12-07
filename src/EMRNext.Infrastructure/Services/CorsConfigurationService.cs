using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors;

namespace EMRNext.Infrastructure.Services
{
    public class CorsConfigurationService
    {
        public static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                var corsSettings = configuration.GetSection("CorsSettings");
                var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new string[] { };

                options.AddPolicy("EMRNextDevPolicy", builder => builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());

                options.AddPolicy("EMRNextProdPolicy", builder => builder
                    .WithOrigins(allowedOrigins)
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    .AllowAnyHeader()
                    .SetIsOriginAllowedToAllowWildcardSubdomains());
            });
        }
    }
}
