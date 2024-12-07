using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EMRNext.Core.Services;
using EMRNext.Core.Repositories;
using EMRNext.Core.Domain.Events;
using Microsoft.Extensions.Hosting;
using EMRNext.Core.Infrastructure.Migrations;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace EMRNext.Core.Configuration
{
    /// <summary>
    /// Centralized dependency injection configuration
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Configure core services and repositories
        /// </summary>
        public static IServiceCollection AddEMRNextCore(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Services
            services.AddScoped<IPatientService, PatientService>();
            
            // Domain Events
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Add validation
            services.AddValidatorsFromAssemblyContaining<Startup>();

            // Logging
            services.AddLogging(configure => 
            {
                configure.AddConsole();
                configure.AddDebug();
            });

            // Performance Monitoring
            services.AddSingleton<PerformanceMonitor>();

            // Register MFA Services
            services.AddScoped<MfaService>();

            // Register Clinical Decision Support Services
            services.AddScoped<ClinicalDecisionSupportService>();

            // Register FHIR Interoperability Services
            services.AddScoped<FhirConversionService>();

            // Register Performance Services
            services.AddSingleton<CacheService>();
            services.AddSingleton<ScalingConfiguration>();
            services.AddSingleton<PerformanceInterceptor>();

            // Configure distributed caching
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
                options.InstanceName = "EMRNext_";
            });

            // Configure performance tracking
            PerformanceTracking.ConfigureInterceptors(services);

            return services;
        }

        /// <summary>
        /// Configure infrastructure services
        /// </summary>
        public static IServiceCollection AddEMRNextInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Register Migration Manager
            services.AddScoped<MigrationManager>();

            // Register all data seeders
            services.AddScoped<IDataSeeder, UserSeeder>();
            services.AddScoped<IDataSeeder, PatientSeeder>();
            services.AddScoped<IDataSeeder, ClinicalReferenceSeeder>();
            services.AddScoped<IDataSeeder, GrowthStandardSeeder>();

            return services;
        }

        /// <summary>
        /// Domain event dispatcher implementation
        /// </summary>
        public class DomainEventDispatcher : IDomainEventDispatcher
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly ILogger<DomainEventDispatcher> _logger;

            public DomainEventDispatcher(
                IServiceProvider serviceProvider, 
                ILogger<DomainEventDispatcher> logger)
            {
                _serviceProvider = serviceProvider;
                _logger = logger;
            }

            public async Task DispatchAsync(DomainEvent domainEvent)
            {
                try 
                {
                    var handlerType = typeof(IDomainEventHandler<>)
                        .MakeGenericType(domainEvent.GetType());

                    var handlers = _serviceProvider
                        .GetServices(handlerType)
                        .Cast<object>();

                    foreach (var handler in handlers)
                    {
                        await (Task)handlerType
                            .GetMethod("HandleAsync")
                            .Invoke(handler, new[] { domainEvent });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error dispatching domain event");
                }
            }
        }

        // Optional: Method to run migrations on application startup
        public static async Task RunMigrationsAsync(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var migrationManager = scope.ServiceProvider.GetRequiredService<MigrationManager>();
                await migrationManager.MigrateAndSeedAsync();
            }
        }
    }
}
