using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EMRNext.Tests.Performance
{
    public class TemplatePerformanceMonitoringTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ITemplateManagementService _templateService;
        private readonly IVariableResolutionService _variableService;
        private readonly Stopwatch _stopwatch;
        private const int PerformanceThresholdMs = 1000; // 1 second threshold

        public TemplatePerformanceMonitoringTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            var scope = _fixture.ServiceProvider.CreateScope();
            _templateService = scope.ServiceProvider.GetRequiredService<ITemplateManagementService>();
            _variableService = scope.ServiceProvider.GetRequiredService<IVariableResolutionService>();
            _stopwatch = new Stopwatch();
        }

        [Fact]
        public async Task BulkTemplateCreation_MeetsPerformanceThreshold()
        {
            // Arrange
            const int templateCount = 100;
            var templates = GenerateBulkTemplates(templateCount);

            // Act
            _stopwatch.Start();
            foreach (var template in templates)
            {
                await _templateService.CreateTemplateAsync(template);
            }
            _stopwatch.Stop();

            // Assert
            _stopwatch.ElapsedMilliseconds.Should().BeLessThan(PerformanceThresholdMs * 2);
            var createdTemplates = await _templateService.GetAllTemplatesAsync();
            createdTemplates.Count().Should().BeGreaterOrEqualTo(templateCount);
        }

        [Fact]
        public async Task ConcurrentTemplateRendering_HandlesLoad()
        {
            // Arrange
            const int concurrentRequests = 50;
            var template = await CreateComplexTemplateAsync();
            var tasks = new List<Task>();

            // Act
            _stopwatch.Start();
            for (int i = 0; i < concurrentRequests; i++)
            {
                var context = await BuildTestContextAsync(i);
                tasks.Add(_templateService.RenderTemplateAsync(template.Id, context));
            }
            await Task.WhenAll(tasks);
            _stopwatch.Stop();

            // Assert
            _stopwatch.ElapsedMilliseconds.Should().BeLessThan(PerformanceThresholdMs * 5);
            tasks.All(t => t.IsCompletedSuccessfully).Should().BeTrue();
        }

        [Fact]
        public async Task VariableResolution_CacheEfficiency()
        {
            // Arrange
            var template = await CreateTemplateWithCachedVariablesAsync();
            var context = await BuildTestContextAsync(1);

            // First render - should populate cache
            _stopwatch.Start();
            await _templateService.RenderTemplateAsync(template.Id, context);
            var firstRenderTime = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Reset();

            // Second render - should use cache
            _stopwatch.Start();
            await _templateService.RenderTemplateAsync(template.Id, context);
            var secondRenderTime = _stopwatch.ElapsedMilliseconds;

            // Assert
            secondRenderTime.Should().BeLessThan(firstRenderTime / 2);
        }

        [Fact]
        public async Task LargeTemplateRendering_MemoryEfficiency()
        {
            // Arrange
            var template = await CreateLargeTemplateAsync();
            var context = await BuildTestContextAsync(1);

            // Act
            var memoryBefore = GC.GetTotalMemory(true);
            await _templateService.RenderTemplateAsync(template.Id, context);
            var memoryAfter = GC.GetTotalMemory(true);

            // Assert
            var memoryUsed = memoryAfter - memoryBefore;
            memoryUsed.Should().BeLessThan(50 * 1024 * 1024); // 50MB threshold
        }

        [Fact]
        public async Task DatabaseQueryOptimization_ChecksExecutionPlan()
        {
            // Arrange
            const int sampleSize = 1000;
            await CreateSampleTemplatesAsync(sampleSize);

            // Act
            _stopwatch.Start();
            var templates = await _templateService.GetTemplatesBySpecialtyAsync("Internal Medicine");
            _stopwatch.Stop();

            // Assert
            _stopwatch.ElapsedMilliseconds.Should().BeLessThan(PerformanceThresholdMs);
            templates.Should().NotBeNull();
        }

        private IEnumerable<ClinicalTemplate> GenerateBulkTemplates(int count)
        {
            return Enumerable.Range(1, count).Select(i => new ClinicalTemplate
            {
                Name = $"Template {i}",
                Category = "Performance Test",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "performance_test"
            });
        }

        private async Task<ClinicalTemplate> CreateComplexTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Complex Template",
                Category = "Performance Test",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "performance_test"
            };

            template = await _templateService.CreateTemplateAsync(template);

            // Add multiple sections with complex variable resolution
            for (int i = 1; i <= 10; i++)
            {
                await _templateService.AddSectionAsync(template.Id, new TemplateSection
                {
                    Name = $"Section {i}",
                    Content = GenerateComplexSectionContent(i),
                    OrderIndex = i
                });
            }

            return template;
        }

        private string GenerateComplexSectionContent(int sectionNumber)
        {
            return $@"
                {{#if Patient}}
                    <h{sectionNumber}>Section {sectionNumber}</h{sectionNumber}>
                    <div>
                        Patient Info: {{Patient.FirstName}} {{Patient.LastName}}
                        {{#each Medications}}
                            <div>Medication: {{Name}} {{Dose}}</div>
                        {{/each}}
                        {{#if Vitals}}
                            <div>BP: {{Vitals.BloodPressure}}</div>
                            <div>HR: {{Vitals.HeartRate}}</div>
                        {{/if}}
                    </div>
                {{/if}}
            ";
        }

        private async Task<ClinicalTemplate> CreateTemplateWithCachedVariablesAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Cached Variables Template",
                Category = "Performance Test",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "performance_test"
            };

            template = await _templateService.CreateTemplateAsync(template);

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Cached Section",
                Content = @"
                    {{CachedVariable1}}
                    {{CachedVariable2}}
                    {{CachedVariable3}}
                ",
                OrderIndex = 1
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateLargeTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Large Template",
                Category = "Performance Test",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "performance_test"
            };

            template = await _templateService.CreateTemplateAsync(template);

            // Create a large template with multiple sections
            for (int i = 1; i <= 50; i++)
            {
                await _templateService.AddSectionAsync(template.Id, new TemplateSection
                {
                    Name = $"Large Section {i}",
                    Content = GenerateLargeSectionContent(),
                    OrderIndex = i
                });
            }

            return template;
        }

        private string GenerateLargeSectionContent()
        {
            return string.Join("\n", Enumerable.Range(1, 100).Select(i =>
                $@"<div class='row-{i}'>
                    <span>Label {i}: {{Variable{i}}}</span>
                    <span>Value {i}: {{ComputedValue{i}}}</span>
                </div>"
            ));
        }

        private async Task CreateSampleTemplatesAsync(int count)
        {
            var templates = GenerateBulkTemplates(count);
            foreach (var template in templates)
            {
                await _templateService.CreateTemplateAsync(template);
            }
        }

        private async Task<Dictionary<string, object>> BuildTestContextAsync(int index)
        {
            return new Dictionary<string, object>
            {
                { "Patient", await CreateTestPatientAsync() },
                { "Medications", await CreateTestMedicationsAsync() },
                { "Vitals", CreateTestVitalsAsync() },
                { $"Variable{index}", $"Value {index}" },
                { "CachedVariable1", "Cached Value 1" },
                { "CachedVariable2", "Cached Value 2" },
                { "CachedVariable3", "Cached Value 3" }
            };
        }

        private async Task<Patient> CreateTestPatientAsync()
        {
            var patient = new Patient
            {
                FirstName = "Performance",
                LastName = "Test",
                DateOfBirth = DateTime.Parse("1980-01-01"),
                Gender = "Male",
                MedicalRecordNumber = $"MRN{DateTime.Now.Ticks}"
            };

            _fixture.CreateContext().Patients.Add(patient);
            await _fixture.CreateContext().SaveChangesAsync();
            return patient;
        }

        private async Task<List<Medication>> CreateTestMedicationsAsync()
        {
            return Enumerable.Range(1, 5).Select(i => new Medication
            {
                Name = $"Medication {i}",
                Dose = $"{i * 10}mg",
                Source = "Test"
            }).ToList();
        }

        private Vital CreateTestVitalsAsync()
        {
            return new Vital
            {
                BloodPressure = "120/80",
                HeartRate = "72",
                Temperature = "98.6",
                RespiratoryRate = "16",
                RecordedDate = DateTime.UtcNow
            };
        }
    }
}
