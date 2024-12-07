using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Services.Clinical;

namespace EMRNext.Core.Configuration
{
    public static class ClinicalWorkflowConfiguration
    {
        public static IServiceCollection AddClinicalWorkflowServices(
            this IServiceCollection services)
        {
            // Register Clinical Workflow Service
            services.AddScoped<ClinicalWorkflowService>();

            // Configure workflow-related options
            services.AddOptions<WorkflowOptions>()
                .Configure(options =>
                {
                    options.DefaultWorkflowTimeout = TimeSpan.FromDays(7);
                    options.AllowedWorkflowTypes = new[]
                    {
                        "MedicalConsultation",
                        "DiagnosticProcedure",
                        "Prescription"
                    };
                });

            return services;
        }

        // Workflow Configuration Options
        public class WorkflowOptions
        {
            public TimeSpan DefaultWorkflowTimeout { get; set; }
            public string[] AllowedWorkflowTypes { get; set; }
        }
    }
}
