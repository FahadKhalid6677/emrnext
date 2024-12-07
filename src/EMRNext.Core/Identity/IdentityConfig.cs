using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EMRNext.Core.Identity
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddEMRIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings - HIPAA compliant
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

                // Session settings
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddPasswordValidator<HIPAAPasswordValidator<ApplicationUser>>();

            // Add custom password expiration policy
            services.Configure<PasswordExpirationOptions>(options =>
            {
                options.PasswordExpirationDays = 90;
                options.PasswordExpirationWarningDays = 14;
            });

            return services;
        }
    }

    public class PasswordExpirationOptions
    {
        public int PasswordExpirationDays { get; set; }
        public int PasswordExpirationWarningDays { get; set; }
    }
}
