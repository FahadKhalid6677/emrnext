using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace EMRNext.Tests.Performance
{
    public class TemplatePerformanceTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ITemplateManagementService _templateService;
        private readonly IVariableResolutionService _variableService;
        private const int PerformanceThresholdMs = 1000; // 1 second threshold

        public TemplatePerformanceTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            var scope = _fixture.ServiceProvider.CreateScope();
            _templateService = scope.ServiceProvider.GetRequiredService<ITemplateManagementService>();
            _variableService = scope.ServiceProvider.GetRequiredService<IVariableResolutionService>();
        }

        [Fact]
        public async Task TemplateRendering_Performance_UnderThreshold()
        {
            // Arrange
            var template = await CreateComplexTemplateAsync();
            var context = await CreateTestContextAsync();
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var result = await _templateService.RenderTemplateAsync(template.Id, context);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(PerformanceThresholdMs);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public async Task ConcurrentRendering_Performance_UnderThreshold(int concurrentRequests)
        {
            // Arrange
            var template = await CreateComplexTemplateAsync();
            var contexts = await CreateTestContextsAsync(concurrentRequests);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var tasks = contexts.Select(context => 
                _templateService.RenderTemplateAsync(template.Id, context));
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            results.Should().HaveCount(concurrentRequests);
            results.Should().AllSatisfy(r => r.Should().NotBeNull());
            var averageTimePerRequest = stopwatch.ElapsedMilliseconds / concurrentRequests;
            averageTimePerRequest.Should().BeLessThan(PerformanceThresholdMs);
        }

        [Fact]
        public async Task VariableResolution_CacheEfficiency_Test()
        {
            // Arrange
            var variable = await CreateTestVariableAsync();
            var context = await CreateTestContextAsync();
            var iterations = 100;
            var stopwatch = new Stopwatch();

            // Act - First resolution (uncached)
            stopwatch.Start();
            var uncachedResult = await _variableService.ResolveVariableAsync(variable.Name, context);
            var uncachedTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Reset();

            // Act - Subsequent resolutions (cached)
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                await _variableService.ResolveVariableAsync(variable.Name, context);
            }
            var cachedTime = stopwatch.ElapsedMilliseconds / iterations;

            // Assert
            cachedTime.Should().BeLessThan(uncachedTime);
            cachedTime.Should().BeLessThan(50); // 50ms threshold for cached responses
        }

        [Fact]
        public async Task TemplateValidation_Performance_UnderThreshold()
        {
            // Arrange
            var template = await CreateComplexTemplateAsync();
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var validationResults = await _templateService.ValidateTemplateAsync(template.Id);
            stopwatch.Stop();

            // Assert
            validationResults.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(PerformanceThresholdMs);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public async Task TemplateSearch_Performance_UnderThreshold(int templateCount)
        {
            // Arrange
            await CreateMultipleTemplatesAsync(templateCount);
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var templates = await _templateService.GetTemplatesAsync("Progress Note", "Internal Medicine");
            stopwatch.Stop();

            // Assert
            templates.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(PerformanceThresholdMs);
        }

        private async Task<ClinicalTemplate> CreateComplexTemplateAsync()
        {
            var template = await _templateService.CreateTemplateAsync(new ClinicalTemplate
            {
                Name = "Complex Test Template",
                Description = "Performance Test Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "test_user",
                EnableDecisionSupport = true
            });

            // Add multiple sections
            for (int i = 0; i < 10; i++)
            {
                var section = await _templateService.AddSectionAsync(template.Id, new TemplateSection
                {
                    Name = $"Section {i}",
                    Content = GenerateComplexContent(),
                    OrderIndex = i
                });

                // Add multiple fields per section
                for (int j = 0; j < 5; j++)
                {
                    await _templateService.AddFieldAsync(section.Id, new TemplateField
                    {
                        Name = $"Field_{i}_{j}",
                        FieldType = "text",
                        IsRequired = true,
                        OrderIndex = j
                    });
                }
            }

            return template;
        }

        private string GenerateComplexContent()
        {
            return @"
                <div class='complex-content'>
                    <h3>{{SectionTitle}}</h3>
                    <p>Patient: {{PatientName}} (DOB: {{PatientDOB}})</p>
                    <p>Provider: {{ProviderName}}</p>
                    <div class='vitals'>
                        <p>BP: {{VitalsBP}}</p>
                        <p>HR: {{VitalsHR}}</p>
                        <p>Temp: {{VitalsTemp}}</p>
                    </div>
                    <div class='medications'>
                        {{#each Medications}}
                            <p>{{Name}} - {{Dose}} - {{Frequency}}</p>
                        {{/each}}
                    </div>
                    <div class='problems'>
                        {{#each Problems}}
                            <p>{{Description}} ({{OnsetDate}})</p>
                        {{/each}}
                    </div>
                </div>";
        }

        private async Task<TemplateVariable> CreateTestVariableAsync()
        {
            return await _templateService.AddVariableAsync(1, new TemplateVariable
            {
                Name = "TestVariable",
                VariableType = "Patient",
                DataType = "string",
                SourceType = "Database",
                EnableCache = true,
                CacheDuration = 300
            });
        }

        private async Task<Dictionary<string, object>> CreateTestContextAsync()
        {
            return new Dictionary<string, object>
            {
                { "PatientName", "Test Patient" },
                { "PatientDOB", "1980-01-01" },
                { "ProviderName", "Test Provider" },
                { "VitalsBP", "120/80" },
                { "VitalsHR", "72" },
                { "VitalsTemp", "98.6" },
                { "Medications", new[]
                    {
                        new { Name = "Med1", Dose = "10mg", Frequency = "Daily" },
                        new { Name = "Med2", Dose = "20mg", Frequency = "BID" }
                    }
                },
                { "Problems", new[]
                    {
                        new { Description = "Problem1", OnsetDate = "2023-01-01" },
                        new { Description = "Problem2", OnsetDate = "2023-02-01" }
                    }
                }
            };
        }

        private async Task<List<Dictionary<string, object>>> CreateTestContextsAsync(int count)
        {
            var contexts = new List<Dictionary<string, object>>();
            for (int i = 0; i < count; i++)
            {
                var context = await CreateTestContextAsync();
                context["UniqueId"] = i;
                contexts.Add(context);
            }
            return contexts;
        }

        private async Task CreateMultipleTemplatesAsync(int count)
        {
            for (int i = 0; i < count; i++)
            {
                await _templateService.CreateTemplateAsync(new ClinicalTemplate
                {
                    Name = $"Template {i}",
                    Category = "Progress Note",
                    SpecialtyType = "Internal Medicine",
                    CreatedBy = "test_user"
                });
            }
        }
    }
}
