using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Helmet.AspNetCore;

namespace EMRNext.Web
{
    public static class SecurityConfiguration
    {
        public static void ConfigureNetworkSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("EMRNextPolicy", builder =>
                {
                    builder
                        .WithOrigins(
                            "https://emrnext.com", 
                            "https://www.emrnext.com", 
                            "https://railway.app"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // Configure Forwarded Headers for Railway deployment
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = 
                    ForwardedHeaders.XForwardedFor | 
                    ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // JWT Authentication
            var jwtKey = configuration["JWT:SecretKey"];
            var jwtIssuer = configuration["JWT:Issuer"];
            var jwtAudience = configuration["JWT:Audience"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)
                        ),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // Add Helmet for additional security headers
            services.AddHelmet(options =>
            {
                options.ContentSecurityPolicy = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';";
                options.ReferrerPolicy = "strict-origin-when-cross-origin";
                options.XssProtection = XssProtectionOption.Block;
                options.FrameOptions = FrameOptionsOption.Deny;
            });
        }

        public static void UseNetworkSecurity(this IApplicationBuilder app)
        {
            // Use forwarded headers
            app.UseForwardedHeaders();

            // Use CORS
            app.UseCors("EMRNextPolicy");

            // Use authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Use Helmet middleware
            app.UseHelmet();
        }
    }
}
