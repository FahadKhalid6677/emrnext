using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EMRNext.Infrastructure.Configuration
{
    public static class EnvironmentConfig
    {
        public static IConfigurationRoot BuildConfiguration(string environment = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("EMRNEXT_")
                .AddUserSecrets<Startup>(optional: true);

            if (environment?.ToLower() == "development")
            {
                builder.AddUserSecrets<Startup>();
            }

            return builder.Build();
        }
    }
}
