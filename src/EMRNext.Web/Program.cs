using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EMRNext.Infrastructure.Data;
using EMRNext.Infrastructure;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using EMRNext.Core.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using EMRNext.Infrastructure.Configuration;

namespace EMRNext.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Use centralized startup configuration
        StartupConfigurationService.ConfigureServices(builder.Services, builder.Configuration);

        // Add AutoMapper
        builder.Services.AddAutoMapper(typeof(GroupSeriesMappingProfile).Assembly);

        // Add API versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        // Add API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "EMRNext API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Build the application
        var app = builder.Build();

        // Seed initial data
        using (var scope = app.Services.CreateScope())
        {
            var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await dataSeeder.SeedAsync();
        }

        // Configure the HTTP request pipeline
        StartupConfigurationService.ConfigurePipeline(app, builder.Environment);

        // Run the application
        app.Run();
    }
}
