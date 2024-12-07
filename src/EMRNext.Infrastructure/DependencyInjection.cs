using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EMRNext.Infrastructure.Data;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Services;
using EMRNext.Core.Services.Portal;

namespace EMRNext.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<EMRNextDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(EMRNextDbContext).Assembly.FullName)));

            // Core Services
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IValidationService, ValidationService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IQualityMeasureService, QualityMeasureService>();
            services.AddScoped<IResourceManagementService, ResourceManagementService>();

            // Portal Services
            services.AddScoped<IGroupSeriesService, GroupSeriesService>();
            services.AddScoped<IGroupAppointmentService, GroupAppointmentService>();
            services.AddScoped<IVitalService, VitalService>();

            // Register DbContext
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .EnableSensitiveDataLogging(configuration.GetValue<bool>("EnableSensitiveDataLogging"))
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            return services;
        }
    }
}
