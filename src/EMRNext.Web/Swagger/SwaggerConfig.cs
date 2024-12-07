using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace EMRNext.Web.Swagger
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EMRNext API",
                    Version = "v1",
                    Description = "EMRNext Electronic Medical Records System API",
                    Contact = new OpenApiContact
                    {
                        Name = "EMRNext Support",
                        Email = "support@emrnext.com",
                        Url = new Uri("https://emrnext.com/support")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "EMRNext License",
                        Url = new Uri("https://emrnext.com/license")
                    }
                });

                // Add JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

                // Add XML Comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Add response examples
                c.ExampleFilters();

                // Add operation filters
                c.OperationFilter<AddRequiredHeaderParameter>();
                c.OperationFilter<AddPaginationParameters>();

                // Add custom schema filters
                c.SchemaFilter<EnumSchemaFilter>();
                c.SchemaFilter<RequiredSchemaFilter>();

                // Configure document filters
                c.DocumentFilter<VersionDocumentFilter>();
                c.DocumentFilter<BasePathDocumentFilter>();

                // Add global parameters
                c.AddGlobalParameters();
            });

            // Add custom example providers
            services.AddSwaggerExamplesFromAssemblyOf<Startup>();

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" }
                    };
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api-docs/v1/swagger.json", "EMRNext API V1");
                c.RoutePrefix = "api-docs";
                c.DocumentTitle = "EMRNext API Documentation";
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1);
                c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();
                
                // Add custom CSS
                c.InjectStylesheet("/swagger-ui/custom.css");
                
                // Add custom JavaScript
                c.InjectJavascript("/swagger-ui/custom.js");
            });

            return app;
        }

        private static void AddGlobalParameters(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key for application identification",
                Name = "X-API-Key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
    }
}
