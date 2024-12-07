using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EMRNext.Core.Models.Identity;

namespace EMRNext.Core.Configuration
{
    public static class AuthenticationConfig
    {
        // Configure Identity Services
        public static IServiceCollection ConfigureIdentityServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Configure Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password Requirements
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;

                // Lockout Settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User Settings
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Sign-in Requirements
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<TotpTokenProvider>("TOTP");

            // Configure Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.SlidingExpiration = true;
            })
            .AddCookie(IdentityConstants.ExternalScheme)
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme)
            .AddOpenIdConnect("oidc", options =>
            {
                // OpenID Connect Configuration
                configuration.GetSection("OpenIdConnect").Bind(options);
            });

            // Configure External Authentication Providers
            ConfigureExternalProviders(services, configuration);

            return services;
        }

        // Configure External Authentication Providers
        private static void ConfigureExternalProviders(
            IServiceCollection services, 
            IConfiguration configuration)
        {
            // Google Authentication
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            });

            // Microsoft Authentication
            services.AddAuthentication().AddMicrosoftAccount(options =>
            {
                options.ClientId = configuration["Authentication:Microsoft:ClientId"];
                options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
            });

            // Add more external providers as needed
        }

        // Custom TOTP Token Provider
        public class TotpTokenProvider : AuthTokenProvider<ApplicationUser>
        {
            public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
            {
                // Implement TOTP token generation logic
                return Task.FromResult(true);
            }

            public override Task<string> GetUserModifierAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
            {
                return Task.FromResult(user.Id);
            }

            public override Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
            {
                // Implement TOTP validation logic
                return Task.FromResult(true);
            }
        }

        // Authorization Policies
        public static IServiceCollection ConfigureAuthorizationPolicies(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Global Admin Policy
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Administrator"));

                // Clinical Staff Policy
                options.AddPolicy("ClinicalStaff", policy =>
                    policy.RequireRole("Physician", "Nurse"));

                // Patient Access Policy
                options.AddPolicy("PatientAccess", policy =>
                    policy.RequireRole("Patient"));

                // Custom Permission-Based Policies
                options.AddPolicy("CreatePatientRecord", policy =>
                    policy.RequireClaim("Permission", "CreatePatientRecord"));

                options.AddPolicy("ViewMedicalHistory", policy =>
                    policy.RequireClaim("Permission", "ViewMedicalHistory"));
            });

            return services;
        }
    }
}
