using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Services.EhrServices;

namespace EMRNext.Core.Configuration
{
    /// <summary>
    /// Configuration for Electronic Health Records (EHR) services
    /// </summary>
    public static class EhrConfiguration
    {
        /// <summary>
        /// Add EHR-related services to the dependency injection container
        /// </summary>
        public static IServiceCollection ConfigureEhrServices(this IServiceCollection services)
        {
            // Register EHR-specific services
            services.AddScoped<IMedicalChartService, MedicalChartService>();

            // Register Genetic Risk Assessment Services
            services.AddScoped<GeneticRiskAssessmentService>();
            services.AddScoped<IGenericRepository<FamilyMedicalHistory>, GenericRepository<FamilyMedicalHistory>>();
            services.AddScoped<IGenericRepository<GeneticRiskProgressTracking>, GenericRepository<GeneticRiskProgressTracking>>();

            // Register Document Security Services
            services.AddScoped<IDocumentSecurityService, DocumentSecurityService>();
            services.AddScoped<IGenericRepository<SecureDocument>, GenericRepository<SecureDocument>>();
            services.AddScoped<IGenericRepository<DocumentAccessPermission>, GenericRepository<DocumentAccessPermission>>();
            services.AddScoped<IGenericRepository<DocumentAccessLog>, GenericRepository<DocumentAccessLog>>();

            // Register Encryption Services
            services.AddSingleton<EncryptionConfiguration>();
            services.AddScoped<IEncryptionService, EncryptionService>();

            // Register Adaptive Role-Based Access Control Services
            services.AddScoped<IAdaptiveRoleService, AdaptiveRoleService>();
            services.AddScoped<IGenericRepository<AdaptiveRole>, GenericRepository<AdaptiveRole>>();
            services.AddScoped<IGenericRepository<AdaptivePermission>, GenericRepository<AdaptivePermission>>();

            // Register FHIR Interoperability Services
            services.AddScoped<IFhirTransformationService, FhirTransformationService>();
            services.AddScoped<IGenericRepository<FhirResource>, GenericRepository<FhirResource>>();
            services.AddScoped<IGenericRepository<FhirMappingRule>, GenericRepository<FhirMappingRule>>();

            // Register Predictive Health Analytics Services
            services.AddScoped<IPredictiveHealthAnalyticsService, PredictiveHealthAnalyticsService>();
            services.AddScoped<IGenericRepository<PatientHealthProfile>, GenericRepository<PatientHealthProfile>>();
            services.AddScoped<IGenericRepository<HealthTrajectory>, GenericRepository<HealthTrajectory>>();

            // Clinical Decision Support Services
            services.AddScoped<IAdvancedClinicalDecisionSupportService, AdvancedClinicalDecisionSupportService>();
            services.AddScoped<IGenericRepository<ClinicalGuideline>, GenericRepository<ClinicalGuideline>>();

            // Clinical Guideline Services
            services.AddScoped<IClinicalGuidelineRepository, ClinicalGuidelineRepository>();
            services.AddTransient<IHostedService, ClinicalGuidelineSeederService>();

            // User Experience and Accessibility Services
            services.AddScoped<IAccessibilityService, AccessibilityService>();
            services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
            services.Configure<UserExperienceConfiguration>(
                Configuration.GetSection("UserExperienceConfiguration"));

            // Compliance and Audit Services
            services.AddScoped<IComplianceAuditRepository, ComplianceAuditRepository>();
            services.AddScoped<IComplianceAuditService, ComplianceAuditService>();
            services.Configure<ComplianceConfiguration>(
                Configuration.GetSection("ComplianceConfiguration"));

            // System Health and Performance Monitoring
            services.AddSingleton<ISystemMetricsRepository, SystemMetricsRepository>();
            services.AddHostedService<SystemHealthMonitorService>();
            services.Configure<SystemHealthConfiguration>(
                Configuration.GetSection("SystemHealthConfiguration"));

            // Advanced Testing Framework
            services.AddScoped<AdvancedTestingFramework>();
            services.AddSingleton<ITestReportRepository, TestReportRepository>();

            return services;
        }
    }
}
