using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;

namespace EMRNext.Core.Infrastructure.Logging
{
    public static class LoggingConfiguration
    {
        public static IServiceCollection AddComprehensiveLogging(
            this IServiceCollection services,
            string applicationName)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.Seq("http://seq:5341")
                .WriteTo.File(new JsonFormatter(),
                    path: "logs/emrnext-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    fileSizeLimitBytes: 10 * 1024 * 1024) // 10MB
                .Filter.ByExcluding(evt => 
                    evt.Properties.ToString().Contains("password") ||
                    evt.Properties.ToString().Contains("ssn") ||
                    evt.Properties.ToString().Contains("creditcard"))
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(logger, dispose: true);
            });

            return services;
        }

        public static ILogger<T> CreateLogger<T>()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddConsole();
            });

            return loggerFactory.CreateLogger<T>();
        }
    }
}
