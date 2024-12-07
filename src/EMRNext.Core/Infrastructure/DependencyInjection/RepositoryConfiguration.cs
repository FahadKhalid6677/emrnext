using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Domain.Repositories;
using EMRNext.Core.Infrastructure.Repositories;

namespace EMRNext.Core.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Dependency injection configuration for repositories
    /// </summary>
    public static class RepositoryConfiguration
    {
        /// <summary>
        /// Add repository services to the dependency injection container
        /// </summary>
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // Patient Repository
            services.AddScoped<IPatientRepository, PatientRepository>();

            // Prescription Repository
            services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

            // Add other repository registrations here

            return services;
        }
    }
}
