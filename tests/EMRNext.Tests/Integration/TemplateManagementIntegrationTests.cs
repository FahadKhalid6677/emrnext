using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace EMRNext.Tests.Integration
{
    public class TemplateManagementIntegrationTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ITemplateManagementService _templateService;
        private readonly IClinicalDecisionSupportService _clinicalService;
        private readonly IVariableResolutionService _variableService;

        public TemplateManagementIntegrationTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            var scope = _fixture.ServiceProvider.CreateScope();
            _templateService = scope.ServiceProvider.GetRequiredService<ITemplateManagementService>();
            _clinicalService = scope.ServiceProvider.GetRequiredService<IClinicalDecisionSupportService>();
            _variableService = scope.ServiceProvider.GetRequiredService<IVariableResolutionService>();
        }

        [Fact]
        public async Task CompleteTemplateWorkflow_Success()
        {
            // 1. Create Template
            var template = await CreateTestTemplateAsync();
            template.Should().NotBeNull();
            template.Id.Should().BeGreaterThan(0);

            // 2. Add Sections
            var section = await AddTestSectionAsync(template.Id);
            section.Should().NotBeNull();
            section.Id.Should().BeGreaterThan(0);

            // 3. Add Fields
            var field = await AddTestFieldAsync(section.Id);
            field.Should().NotBeNull();
            field.Id.Should().BeGreaterThan(0);

            // 4. Add Variables
            var variable = await AddTestVariableAsync(template.Id);
            variable.Should().NotBeNull();
            variable.Id.Should().BeGreaterThan(0);

            // 5. Validate Template
            var validationResults = await _templateService.ValidateTemplateAsync(template.Id);
            validationResults.Should().BeEmpty();

            // 6. Publish Template
            var publishedTemplate = await _templateService.PublishTemplateAsync(template.Id);
            publishedTemplate.Should().NotBeNull();
            publishedTemplate.IsPublished.Should().BeTrue();

            // 7. Render Template
            var context = await CreateTestContextAsync();
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Test Patient");
        }

        [Fact]
        public async Task ClinicalIntegration_Success()
        {
            // 1. Create Clinical Rule
            var rule = await CreateTestClinicalRuleAsync();
            rule.Should().NotBeNull();
            rule.Id.Should().BeGreaterThan(0);

            // 2. Create Template with Clinical Integration
            var template = await CreateClinicalTemplateAsync(rule.Id);
            template.Should().NotBeNull();
            template.Id.Should().BeGreaterThan(0);

            // 3. Create Test Encounter
            var encounter = await CreateTestEncounterAsync();
            encounter.Should().NotBeNull();
            encounter.Id.Should().BeGreaterThan(0);

            // 4. Get Applicable Templates
            var templates = await _templateService.GetTemplatesForEncounterAsync(encounter.Id);
            templates.Should().NotBeNull();
            templates.Should().Contain(t => t.Id == template.Id);

            // 5. Evaluate Clinical Rules
            var evaluation = await _clinicalService.EvaluateRuleAsync(rule.Id, encounter.Id);
            evaluation.Should().NotBeNull();
            evaluation.IsValid.Should().BeTrue();

            // 6. Render Template with Clinical Data
            var context = await CreateClinicalContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Clinical Alert");
        }

        [Fact]
        public async Task PerformanceTest_ConcurrentAccess()
        {
            // 1. Create Test Template
            var template = await CreateTestTemplateAsync();
            template.Should().NotBeNull();

            // 2. Create Multiple Contexts
            var contexts = await CreateTestContextsAsync(10);

            // 3. Concurrent Rendering
            var tasks = new List<Task<string>>();
            foreach (var context in contexts)
            {
                tasks.Add(_templateService.RenderTemplateAsync(template.Id, context));
            }

            // 4. Wait for All Tasks
            var results = await Task.WhenAll(tasks);

            // 5. Verify Results
            results.Should().NotBeNull();
            results.Should().HaveCount(10);
            results.Should().AllSatisfy(content => content.Should().NotBeEmpty());
        }

        [Fact]
        public async Task SecurityTest_AccessControl()
        {
            // 1. Create Template with Restrictions
            var template = await CreateRestrictedTemplateAsync();
            template.Should().NotBeNull();

            // 2. Test Authorized Access
            var authorizedContext = new Dictionary<string, object>
            {
                { "UserId", "authorized_user" },
                { "Role", "physician" }
            };

            var authorizedResult = await _templateService.HasAccessAsync(
                template.Id, "authorized_user", "view");
            authorizedResult.Should().BeTrue();

            // 3. Test Unauthorized Access
            var unauthorizedContext = new Dictionary<string, object>
            {
                { "UserId", "unauthorized_user" },
                { "Role", "nurse" }
            };

            var unauthorizedResult = await _templateService.HasAccessAsync(
                template.Id, "unauthorized_user", "edit");
            unauthorizedResult.Should().BeFalse();
        }

        private async Task<ClinicalTemplate> CreateTestTemplateAsync()
        {
            return await _templateService.CreateTemplateAsync(new ClinicalTemplate
            {
                Name = "Test Template",
                Description = "Test Description",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "test_user"
            });
        }

        private async Task<TemplateSection> AddTestSectionAsync(int templateId)
        {
            return await _templateService.AddSectionAsync(templateId, new TemplateSection
            {
                Name = "Test Section",
                Description = "Test Section Description",
                Content = "Patient Name: {{PatientName}}",
                OrderIndex = 1
            });
        }

        private async Task<TemplateField> AddTestFieldAsync(int sectionId)
        {
            return await _templateService.AddFieldAsync(sectionId, new TemplateField
            {
                Name = "PatientName",
                Label = "Patient Name",
                FieldType = "text",
                IsRequired = true,
                OrderIndex = 1
            });
        }

        private async Task<TemplateVariable> AddTestVariableAsync(int templateId)
        {
            return await _templateService.AddVariableAsync(templateId, new TemplateVariable
            {
                Name = "PatientName",
                VariableType = "Patient",
                DataType = "string",
                SourceType = "Database",
                AutoResolve = true
            });
        }

        private async Task<Dictionary<string, object>> CreateTestContextAsync()
        {
            return new Dictionary<string, object>
            {
                { "PatientName", "Test Patient" },
                { "EncounterId", 1 },
                { "UserId", "test_user" }
            };
        }

        private async Task<ClinicalRule> CreateTestClinicalRuleAsync()
        {
            return await _clinicalService.CreateRuleAsync(new ClinicalRule
            {
                Name = "Test Rule",
                Description = "Test Rule Description",
                Condition = "age > 65",
                Recommendation = "Consider fall risk assessment",
                Severity = "Warning"
            });
        }

        private async Task<ClinicalTemplate> CreateClinicalTemplateAsync(int ruleId)
        {
            var template = await CreateTestTemplateAsync();
            template.EnableDecisionSupport = true;
            template.AssociatedRules = $"[{ruleId}]";
            return await _templateService.UpdateTemplateAsync(template);
        }

        private async Task<Encounter> CreateTestEncounterAsync()
        {
            var context = _fixture.CreateContext();
            var encounter = new Encounter
            {
                EncounterDate = DateTime.UtcNow,
                EncounterType = "Office Visit",
                Provider = new Provider
                {
                    FirstName = "Test",
                    LastName = "Provider",
                    Specialty = "Internal Medicine"
                },
                Patient = new Patient
                {
                    FirstName = "Test",
                    LastName = "Patient",
                    DateOfBirth = DateTime.UtcNow.AddYears(-70)
                }
            };

            context.Encounters.Add(encounter);
            await context.SaveChangesAsync();
            return encounter;
        }

        private async Task<Dictionary<string, object>> CreateClinicalContextAsync(int encounterId)
        {
            var context = await CreateTestContextAsync();
            context["EncounterId"] = encounterId;
            return context;
        }

        private async Task<List<Dictionary<string, object>>> CreateTestContextsAsync(int count)
        {
            var contexts = new List<Dictionary<string, object>>();
            for (int i = 0; i < count; i++)
            {
                contexts.Add(await CreateTestContextAsync());
            }
            return contexts;
        }

        private async Task<ClinicalTemplate> CreateRestrictedTemplateAsync()
        {
            var template = await CreateTestTemplateAsync();
            template.AllowedRoles = "[\"physician\"]";
            return await _templateService.UpdateTemplateAsync(template);
        }
    }

    public class TestDatabaseFixture : IDisposable
    {
        private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=EMRNextTestDb;Trusted_Connection=True;MultipleActiveResultSets=true";
        public IServiceProvider ServiceProvider { get; }

        public TestDatabaseFixture()
        {
            var services = new ServiceCollection();

            services.AddDbContext<EMRDbContext>(options =>
                options.UseSqlServer(ConnectionString));

            services.AddScoped<ITemplateManagementService, TemplateManagementService>();
            services.AddScoped<IClinicalDecisionSupportService, ClinicalDecisionSupportService>();
            services.AddScoped<IVariableResolutionService, VariableResolutionService>();
            services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<INotificationService, NotificationService>();

            ServiceProvider = services.BuildServiceProvider();

            CreateDatabase();
        }

        public EMRDbContext CreateContext()
            => ServiceProvider.GetRequiredService<EMRDbContext>();

        private void CreateDatabase()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            using var context = CreateContext();
            context.Database.EnsureDeleted();
        }
    }
}
