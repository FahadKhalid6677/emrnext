using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EMRNext.Infrastructure.Data;

namespace EMRNext.Infrastructure.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddApplicationDatabase(
            this IServiceCollection services,
            DatabaseSettings settings)
        {
            services.AddSingleton(settings);

            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                ConfigureDbContext(options, settings);
            });

            services.AddScoped<DatabaseInitializer>();

            return services;
        }

        private static void ConfigureDbContext(DbContextOptionsBuilder options, DatabaseSettings settings)
        {
            switch (settings.Provider?.ToLower())
            {
                case "sqlite":
                    options.UseSqlite(
                        settings.ConnectionString,
                        x => ConfigureMigrations(x, settings));
                    break;

                case "inmemory":
                    options.UseInMemoryDatabase("EMRNextDb");
                    break;

                case "postgresql":
                    options.UseNpgsql(
                        settings.ConnectionString,
                        x => ConfigureMigrations(x, settings));
                    break;

                default:
                    options.UseSqlServer(
                        settings.ConnectionString,
                        x => ConfigureMigrations(x, settings));
                    break;
            }

            if (settings.EnableDetailedErrors)
            {
                options.EnableDetailedErrors();
            }

            if (settings.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }

            options.EnableServiceProviderCaching();
        }

        private static void ConfigureMigrations<T>(T builder, DatabaseSettings settings) where T : Microsoft.EntityFrameworkCore.Infrastructure.IRelationalDbContextOptionsBuilderInfrastructure
        {
            var assembly = settings.MigrationsAssembly ?? typeof(ApplicationDbContext).Assembly.GetName().Name;
            ((dynamic)builder).MigrationsAssembly(assembly);

            if (settings.CommandTimeout > 0)
            {
                ((dynamic)builder).CommandTimeout(settings.CommandTimeout);
            }

            if (settings.MaxRetryCount > 0)
            {
                ((dynamic)builder).EnableRetryOnFailure(
                    maxRetryCount: settings.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(settings.MaxRetryDelay),
                    errorNumbersToAdd: null);
            }
        }
    }
}
