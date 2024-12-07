using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EMRNext.Core.Security.Services;
using EMRNext.Core.Security.Providers;

namespace EMRNext.Core.Security.Configuration
{
    /// <summary>
    /// Centralized security configuration for the application
    /// </summary>
    public static class SecurityConfiguration
    {
        /// <summary>
        /// Configure comprehensive security services
        /// </summary>
        public static IServiceCollection AddAdvancedSecurity(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Add authorization services
            services.AddScoped<AuthorizationService>();
            services.AddScoped<TokenService>();
            services.AddScoped<EncryptionService>();

            // Configure JWT Authentication
            ConfigureJwtAuthentication(services, configuration);

            // Add additional security providers
            services.AddSingleton<AuditLogProvider>();
            services.AddSingleton<SecurityMetricsProvider>();

            return services;
        }

        /// <summary>
        /// Configure JWT Bearer Token Authentication
        /// </summary>
        private static void ConfigureJwtAuthentication(
            IServiceCollection services, 
            IConfiguration configuration)
        {
            var key = Encoding.ASCII.GetBytes(
                configuration["Jwt:SecretKey"] ?? 
                throw new InvalidOperationException("JWT Secret Key not configured")
            );

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });
        }

        /// <summary>
        /// Configure role-based authorization policies
        /// </summary>
        public static void ConfigureAuthorizationPolicies(
            this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Admin policy
                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("Administrator"));

                // Clinical staff policy
                options.AddPolicy("ClinicalAccess", policy => 
                    policy.RequireRole("Doctor", "Nurse"));

                // Patient access policy
                options.AddPolicy("PatientView", policy => 
                    policy.RequireRole("Patient", "Administrator"));

                // Custom fine-grained policies can be added here
                options.AddPolicy("RecordModification", policy => 
                    policy.RequireClaim("CanModifyRecords", "True"));
            });
        }
    }
}
