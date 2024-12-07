using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EMRNext.Core.Authorization
{
    public static class EMRAuthorizationPolicies
    {
        public const string ViewClinicalData = "ViewClinicalData";
        public const string ModifyClinicalData = "ModifyClinicalData";
        public const string ViewSchedule = "ViewSchedule";
        public const string ModifySchedule = "ModifySchedule";
        public const string ViewBilling = "ViewBilling";
        public const string ModifyBilling = "ModifyBilling";
        public const string AdminAccess = "AdminAccess";

        public static void AddEMRPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(ViewClinicalData, policy =>
                    policy.RequireRole("Physician", "Nurse", "MedicalAssistant")
                          .RequireClaim("ClinicalAccess", "true"));

                options.AddPolicy(ModifyClinicalData, policy =>
                    policy.RequireRole("Physician")
                          .RequireClaim("ClinicalAccess", "true")
                          .RequireClaim("ModifyAccess", "true"));

                options.AddPolicy(ViewSchedule, policy =>
                    policy.RequireRole("Physician", "Nurse", "MedicalAssistant", "Receptionist")
                          .RequireClaim("SchedulingAccess", "true"));

                options.AddPolicy(ModifySchedule, policy =>
                    policy.RequireRole("Physician", "Receptionist")
                          .RequireClaim("SchedulingAccess", "true")
                          .RequireClaim("ModifyAccess", "true"));

                options.AddPolicy(ViewBilling, policy =>
                    policy.RequireRole("BillingStaff", "Admin")
                          .RequireClaim("BillingAccess", "true"));

                options.AddPolicy(ModifyBilling, policy =>
                    policy.RequireRole("BillingStaff")
                          .RequireClaim("BillingAccess", "true")
                          .RequireClaim("ModifyAccess", "true"));

                options.AddPolicy(AdminAccess, policy =>
                    policy.RequireRole("Admin")
                          .RequireClaim("AdminAccess", "true"));
            });
        }
    }
}
