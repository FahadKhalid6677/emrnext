using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace EMRNext.API.Middleware
{
    public class FhirMiddleware
    {
        private readonly RequestDelegate _next;

        public FhirMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/fhir"))
            {
                // Add FHIR-specific headers
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                
                // Set FHIR content type if not specified
                if (!context.Response.Headers.ContainsKey("Content-Type"))
                {
                    context.Response.Headers.Add("Content-Type", "application/fhir+json");
                }

                // Handle FHIR-specific requirements
                if (context.Request.Method == "GET")
                {
                    // Handle _summary parameter
                    var summary = context.Request.Query["_summary"].ToString();
                    if (!string.IsNullOrEmpty(summary))
                    {
                        context.Items["FhirSummary"] = summary;
                    }

                    // Handle _elements parameter
                    var elements = context.Request.Query["_elements"].ToString();
                    if (!string.IsNullOrEmpty(elements))
                    {
                        context.Items["FhirElements"] = elements.Split(',');
                    }
                }
            }

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Convert exceptions to FHIR OperationOutcome
                if (context.Request.Path.StartsWithSegments("/api/fhir"))
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/fhir+json";

                    var operationOutcome = new
                    {
                        resourceType = "OperationOutcome",
                        issue = new[]
                        {
                            new
                            {
                                severity = "error",
                                code = "processing",
                                diagnostics = ex.Message
                            }
                        }
                    };

                    await context.Response.WriteAsJsonAsync(operationOutcome);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
