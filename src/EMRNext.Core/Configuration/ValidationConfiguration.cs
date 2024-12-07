using EMRNext.Core.Services;
using EMRNext.Core.Services.Integration;
using EMRNext.Core.Services.Portal;
using EMRNext.Core.Validation;
using EMRNext.Core.Validation.Rules;
using EMRNext.Core.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EMRNext.Core.Configuration
{
    public static class ValidationConfiguration
    {
        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            // Register validation service
            services.AddScoped<IValidationService, ValidationService>();

            // Register validation rules
            services.AddScoped<IValidationRule<Vital>, VitalValidationRules>();
            services.AddScoped<IValidationRule<GroupSession>, GroupSessionValidationRules>();
            services.AddScoped<IValidationRule<GroupSessionTemplate>, GroupSessionTemplateValidationRules>();
            services.AddScoped<IValidationRule<GroupSeries>, GroupSeriesValidationRules>();

            // Register Portal Services
            services.AddScoped<IPatientPortalService, PatientPortalService>();
            services.AddScoped<ISecureMessagingService, SecureMessagingService>();
            services.AddScoped<IPortalAuthenticationService, PortalAuthenticationService>();
            services.AddScoped<IAppointmentSchedulingService, AppointmentSchedulingService>();
            services.AddScoped<IGroupAppointmentService, GroupAppointmentService>();
            services.AddScoped<IGroupSeriesService, GroupSeriesService>();
            services.AddScoped<IGroupTemplateService, GroupTemplateService>();
            services.AddScoped<IWaitListService, WaitListService>();

            // Register Integration Services
            services.AddScoped<IHL7Service, HL7Service>();
            services.AddScoped<IEDIService, EDIService>();

            return services;
        }
    }
}
