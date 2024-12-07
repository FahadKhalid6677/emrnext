using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EMRNext.Infrastructure.Data;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace EMRNext.Infrastructure.Configuration
{
    public static class StartupConfigurationService
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Database Configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString,
                    b => b.MigrationsAssembly("EMRNext.Infrastructure")));

            // Identity Configuration
            ConfigureIdentity(services);

            // CORS Configuration
            ConfigureCors(services, configuration);

            // Authentication Configuration
            ConfigureAuthentication(services, configuration);

            // Authorization Configuration
            ConfigureAuthorization(services);

            // External Services Configuration
            ConfigureExternalServices(services, configuration);

            // Dependency Injection
            RegisterDependencyInjection(services);

            // Logging Configuration
            ConfigureLogging(services);

            // Additional Service Registrations
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        private static void ConfigureIdentity(IServiceCollection services)
        {
            services.AddDefaultIdentity<IdentityUser>(options => 
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        }

        private static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                var corsSettings = configuration.GetSection("CorsSettings");
                var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() 
                    ?? new string[] { "http://localhost:3000", "https://localhost:3000" };

                options.AddPolicy("EMRNextDevPolicy", builder => builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }

        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            // Configure JWT Authentication
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // Configure Cookie Authentication as fallback
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            // Register JWT Token Service
            services.AddScoped<JwtTokenService>();
        }

        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("SystemAdmin"));
                options.AddPolicy("MedicalStaff", policy => 
                    policy.RequireRole("Physician", "Nurse"));
            });
        }

        private static void ConfigureExternalServices(IServiceCollection services, IConfiguration configuration)
        {
            // Lab Integration Service
            services.AddHttpClient<ILabInterfaceEngine, LabInterfaceEngineService>(client =>
            {
                client.BaseAddress = new Uri(configuration["ExternalServices:LabIntegration:BaseUrl"]);
                client.DefaultRequestHeaders.Add("X-API-Key", configuration["ExternalServices:LabIntegration:ApiKey"]);
            });

            // Notification Service Configuration
            services.AddSingleton<INotificationService, NotificationService>(provider =>
            {
                var mockDelivery = configuration.GetValue<bool>("ExternalServices:NotificationService:MockDelivery");
                return new NotificationService(mockDelivery);
            });
        }

        private static void RegisterDependencyInjection(IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<ILabOrderRepository, LabOrderRepository>();
            services.AddScoped<IVitalSignRepository, VitalSignRepository>();

            // Register services
            services.AddScoped<ILabOrderService, LabOrderService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IQualityService, QualityService>();
        }

        private static void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Trace);
            });
        }

        public static void ConfigurePipeline(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Run database migrations
            RunMigrations(app);

            // Development specific configuration
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EMRNext API v1"));
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors("EMRNextDevPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            // Seed initial users
            SeedUsers(app);
        }

        private static void RunMigrations(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try 
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database migration failed: {ex.Message}");
                    throw;
                }
            }
        }

        private static void SeedUsers(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Create admin role if it doesn't exist
                if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
                }

                // Create a default admin user if no users exist
                if (userManager.Users.Count() == 0)
                {
                    var adminUser = new IdentityUser 
                    { 
                        UserName = "admin", 
                        Email = "admin@emrnext.com",
                        EmailConfirmed = true 
                    };

                    var result = userManager.CreateAsync(adminUser, "EMRNext2024!").GetAwaiter().GetResult();
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
                    }
                }
            }
        }
    }
}
