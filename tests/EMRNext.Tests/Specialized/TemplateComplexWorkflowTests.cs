using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace EMRNext.Tests.Specialized
{
    public class TemplateComplexWorkflowTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ITemplateManagementService _templateService;
        private readonly IClinicalDecisionSupportService _clinicalService;

        public TemplateComplexWorkflowTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            var scope = _fixture.ServiceProvider.CreateScope();
            _templateService = scope.ServiceProvider.GetRequiredService<ITemplateManagementService>();
            _clinicalService = scope.ServiceProvider.GetRequiredService<IClinicalDecisionSupportService>();
        }

        [Fact]
        public async Task ComplexSurgicalTemplate_WithNestedSections_Success()
        {
            // Arrange
            var template = await CreateComplexSurgicalTemplateAsync();
            var encounter = await CreateSurgicalEncounterAsync();

            // Act
            var context = await BuildSurgicalContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Pre-operative Assessment");
            renderedContent.Should().Contain("Surgical Procedure");
            renderedContent.Should().Contain("Post-operative Care");
            
            // Verify nested sections
            renderedContent.Should().Contain("Anesthesia Details");
            renderedContent.Should().Contain("Surgical Technique");
            renderedContent.Should().Contain("Complications");
        }

        [Fact]
        public async Task MentalHealthTemplate_WithConditionalLogic_Success()
        {
            // Arrange
            var template = await CreateMentalHealthTemplateAsync();
            var encounter = await CreateMentalHealthEncounterAsync();

            // Act
            var context = await BuildMentalHealthContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            
            // Verify conditional sections based on assessment scores
            if (context.ContainsKey("PHQ9Score") && (int)context["PHQ9Score"] > 15)
            {
                renderedContent.Should().Contain("Severe Depression Protocol");
            }
            
            if (context.ContainsKey("SuicideRisk") && (bool)context["SuicideRisk"])
            {
                renderedContent.Should().Contain("Crisis Intervention Plan");
            }
        }

        [Fact]
        public async Task PediatricGrowthChart_WithDataIntegration_Success()
        {
            // Arrange
            var template = await CreatePediatricGrowthTemplateAsync();
            var encounter = await CreatePediatricEncounterAsync();

            // Act
            var context = await BuildPediatricContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Growth Percentiles");
            renderedContent.Should().Contain("Development Milestones");
            renderedContent.Should().Contain("Immunization Status");
        }

        [Fact]
        public async Task EmergencyDepartment_WithTimeConstraints_Success()
        {
            // Arrange
            var template = await CreateEmergencyTemplateAsync();
            var encounter = await CreateEmergencyEncounterAsync();

            // Act
            var context = await BuildEmergencyContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Triage Assessment");
            renderedContent.Should().Contain("Critical Interventions");
            renderedContent.Should().Contain("Disposition Planning");
        }

        private async Task<ClinicalTemplate> CreateComplexSurgicalTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Complex Surgical Template",
                Category = "Surgery",
                SpecialtyType = "General Surgery",
                CreatedBy = "test_user"
            };

            template = await _templateService.CreateTemplateAsync(template);

            // Add main sections
            var preOpSection = await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Pre-operative Assessment",
                OrderIndex = 1,
                IsRequired = true
            });

            var surgicalSection = await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Surgical Procedure",
                OrderIndex = 2,
                IsRequired = true
            });

            var postOpSection = await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Post-operative Care",
                OrderIndex = 3,
                IsRequired = true
            });

            // Add nested sections
            await _templateService.AddSectionAsync(surgicalSection.Id, new TemplateSection
            {
                Name = "Anesthesia Details",
                OrderIndex = 1,
                ParentSectionId = surgicalSection.Id
            });

            await _templateService.AddSectionAsync(surgicalSection.Id, new TemplateSection
            {
                Name = "Surgical Technique",
                OrderIndex = 2,
                ParentSectionId = surgicalSection.Id
            });

            await _templateService.AddSectionAsync(surgicalSection.Id, new TemplateSection
            {
                Name = "Complications",
                OrderIndex = 3,
                ParentSectionId = surgicalSection.Id,
                HasConditions = true,
                DisplayConditions = "{\"ComplicationPresent\": true}"
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateMentalHealthTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Mental Health Assessment",
                Category = "Psychiatry",
                SpecialtyType = "Psychiatry",
                CreatedBy = "test_user"
            };

            template = await _templateService.CreateTemplateAsync(template);

            // Add conditional sections based on assessment scores
            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Depression Assessment",
                OrderIndex = 1,
                Content = "PHQ-9 Score: {{PHQ9Score}}"
            });

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Severe Depression Protocol",
                OrderIndex = 2,
                HasConditions = true,
                DisplayConditions = "{\"PHQ9Score\": {\"operator\": \">\", \"value\": 15}}"
            });

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Crisis Intervention Plan",
                OrderIndex = 3,
                HasConditions = true,
                DisplayConditions = "{\"SuicideRisk\": true}"
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreatePediatricGrowthTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Pediatric Growth Assessment",
                Category = "Pediatrics",
                SpecialtyType = "Pediatrics",
                CreatedBy = "test_user"
            };

            template = await _templateService.CreateTemplateAsync(template);

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Growth Measurements",
                OrderIndex = 1,
                Content = @"
                    Height: {{Height}} ({{HeightPercentile}} percentile)
                    Weight: {{Weight}} ({{WeightPercentile}} percentile)
                    BMI: {{BMI}} ({{BMIPercentile}} percentile)
                "
            });

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Development Milestones",
                OrderIndex = 2
            });

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Immunization Status",
                OrderIndex = 3
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateEmergencyTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Emergency Department Note",
                Category = "Emergency",
                SpecialtyType = "Emergency Medicine",
                CreatedBy = "test_user"
            };

            template = await _templateService.CreateTemplateAsync(template);

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Triage Assessment",
                OrderIndex = 1,
                IsRequired = true,
                Content = @"
                    Chief Complaint: {{ChiefComplaint}}
                    Triage Level: {{TriageLevel}}
                    Vital Signs: {{VitalSigns}}
                "
            });

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Critical Interventions",
                OrderIndex = 2,
                HasConditions = true,
                DisplayConditions = "{\"TriageLevel\": {\"operator\": \"<=\", \"value\": 2}}"
            });

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Disposition Planning",
                OrderIndex = 3,
                IsRequired = true
            });

            return template;
        }

        private async Task<Encounter> CreateSurgicalEncounterAsync()
        {
            var encounter = new Encounter
            {
                EncounterDate = DateTime.UtcNow,
                EncounterType = "Surgery",
                Provider = new Provider
                {
                    FirstName = "Test",
                    LastName = "Surgeon",
                    Specialty = "General Surgery"
                },
                Patient = await CreateTestPatientAsync()
            };

            _fixture.CreateContext().Encounters.Add(encounter);
            await _fixture.CreateContext().SaveChangesAsync();
            return encounter;
        }

        private async Task<Dictionary<string, object>> BuildSurgicalContextAsync(int encounterId)
        {
            var context = new Dictionary<string, object>
            {
                { "EncounterId", encounterId },
                { "ComplicationPresent", true },
                { "AnesthesiaType", "General" },
                { "SurgicalProcedure", "Appendectomy" },
                { "Complications", new[] { "Minor bleeding" } }
            };

            return context;
        }

        private async Task<Dictionary<string, object>> BuildMentalHealthContextAsync(int encounterId)
        {
            var context = new Dictionary<string, object>
            {
                { "EncounterId", encounterId },
                { "PHQ9Score", 18 },
                { "SuicideRisk", true },
                { "PriorPsychHistory", true }
            };

            return context;
        }

        private async Task<Dictionary<string, object>> BuildPediatricContextAsync(int encounterId)
        {
            var context = new Dictionary<string, object>
            {
                { "EncounterId", encounterId },
                { "Height", 100 },
                { "HeightPercentile", 75 },
                { "Weight", 20 },
                { "WeightPercentile", 50 },
                { "BMI", 18.5 },
                { "BMIPercentile", 60 }
            };

            return context;
        }

        private async Task<Dictionary<string, object>> BuildEmergencyContextAsync(int encounterId)
        {
            var context = new Dictionary<string, object>
            {
                { "EncounterId", encounterId },
                { "ChiefComplaint", "Chest Pain" },
                { "TriageLevel", 2 },
                { "VitalSigns", "BP: 160/90, HR: 110, RR: 22" }
            };

            return context;
        }

        private async Task<Patient> CreateTestPatientAsync()
        {
            var patient = new Patient
            {
                FirstName = "Test",
                LastName = "Patient",
                DateOfBirth = DateTime.Parse("1980-01-01"),
                Gender = "Male",
                MedicalRecordNumber = "MRN123"
            };

            _fixture.CreateContext().Patients.Add(patient);
            await _fixture.CreateContext().SaveChangesAsync();
            return patient;
        }
    }
}
