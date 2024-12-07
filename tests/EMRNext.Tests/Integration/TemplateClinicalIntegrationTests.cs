using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace EMRNext.Tests.Integration
{
    public class TemplateClinicalIntegrationTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ITemplateManagementService _templateService;
        private readonly IClinicalDecisionSupportService _clinicalService;
        private readonly IFHIRService _fhirService;

        public TemplateClinicalIntegrationTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            var scope = _fixture.ServiceProvider.CreateScope();
            _templateService = scope.ServiceProvider.GetRequiredService<ITemplateManagementService>();
            _clinicalService = scope.ServiceProvider.GetRequiredService<IClinicalDecisionSupportService>();
            _fhirService = scope.ServiceProvider.GetRequiredService<IFHIRService>();
        }

        [Fact]
        public async Task Template_ClinicalRuleIntegration_Success()
        {
            // Arrange
            var template = await CreateClinicalTemplateAsync();
            var encounter = await CreateTestEncounterAsync();
            var rule = await CreateDiabetesRuleAsync();

            // Associate rule with template
            await _templateService.AssociateRulesAsync(template.Id, new List<int> { rule.Id });

            // Act
            var context = await BuildClinicalContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Diabetes Management Alert");
            renderedContent.Should().Contain("HbA1c > 7.0");
        }

        [Fact]
        public async Task Template_FHIRIntegration_Success()
        {
            // Arrange
            var template = await CreateFHIRTemplateAsync();
            var patient = await CreateTestPatientAsync();
            var fhirPatient = await _fhirService.GetPatientResourceAsync(patient.Id);

            // Act
            var context = new Dictionary<string, object>
            {
                { "PatientId", patient.Id },
                { "FHIRPatient", fhirPatient }
            };

            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain(patient.FirstName);
            renderedContent.Should().Contain(patient.LastName);
            renderedContent.Should().Contain("FHIR ID:");
        }

        [Fact]
        public async Task Template_ClinicalDataBinding_Success()
        {
            // Arrange
            var template = await CreateTemplateWithClinicalDataAsync();
            var encounter = await CreateTestEncounterWithVitalsAsync();

            // Act
            var context = await BuildClinicalContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Blood Pressure:");
            renderedContent.Should().Contain("Heart Rate:");
            renderedContent.Should().Contain("Temperature:");
        }

        [Fact]
        public async Task Template_DocumentationCompliance_Success()
        {
            // Arrange
            var template = await CreateComplianceTemplateAsync();
            var encounter = await CreateTestEncounterAsync();

            // Act
            var context = await BuildClinicalContextAsync(encounter.Id);
            var validationResults = await _templateService.ValidateTemplateAsync(template.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            validationResults.Should().BeEmpty();
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Chief Complaint");
            renderedContent.Should().Contain("History of Present Illness");
            renderedContent.Should().Contain("Review of Systems");
        }

        private async Task<ClinicalTemplate> CreateClinicalTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Diabetes Follow-up Note",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                EnableDecisionSupport = true,
                CreatedBy = "test_user"
            };

            return await _templateService.CreateTemplateAsync(template);
        }

        private async Task<ClinicalTemplate> CreateFHIRTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "FHIR-Enabled Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                EnableFHIR = true,
                CreatedBy = "test_user"
            };

            var section = await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Patient Demographics",
                Content = @"
                    Patient Name: {{Patient.name[0].given[0]}} {{Patient.name[0].family}}
                    FHIR ID: {{Patient.id}}
                    Gender: {{Patient.gender}}
                    Birth Date: {{Patient.birthDate}}
                ",
                OrderIndex = 1
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateTemplateWithClinicalDataAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Clinical Data Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "test_user"
            };

            var section = await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Vital Signs",
                Content = @"
                    Blood Pressure: {{Vitals.BloodPressure}}
                    Heart Rate: {{Vitals.HeartRate}}
                    Temperature: {{Vitals.Temperature}}
                ",
                OrderIndex = 1
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateComplianceTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Compliant Progress Note",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                RequiresCompliance = true,
                CreatedBy = "test_user"
            };

            // Add required sections for compliance
            var sections = new[]
            {
                ("Chief Complaint", 1),
                ("History of Present Illness", 2),
                ("Review of Systems", 3),
                ("Physical Examination", 4),
                ("Assessment", 5),
                ("Plan", 6)
            };

            foreach (var (name, order) in sections)
            {
                await _templateService.AddSectionAsync(template.Id, new TemplateSection
                {
                    Name = name,
                    IsRequired = true,
                    OrderIndex = order
                });
            }

            return template;
        }

        private async Task<ClinicalRule> CreateDiabetesRuleAsync()
        {
            return await _clinicalService.CreateRuleAsync(new ClinicalRule
            {
                Name = "Diabetes HbA1c Alert",
                Description = "Alert for elevated HbA1c",
                Condition = "LabResult.Type == 'HbA1c' && LabResult.Value > 7.0",
                Recommendation = "Consider adjusting diabetes management plan",
                Severity = "Warning"
            });
        }

        private async Task<Encounter> CreateTestEncounterAsync()
        {
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
                Patient = await CreateTestPatientAsync()
            };

            _fixture.CreateContext().Encounters.Add(encounter);
            await _fixture.CreateContext().SaveChangesAsync();
            return encounter;
        }

        private async Task<Encounter> CreateTestEncounterWithVitalsAsync()
        {
            var encounter = await CreateTestEncounterAsync();

            var vitals = new Vital
            {
                EncounterId = encounter.Id,
                PatientId = encounter.Patient.Id,
                BloodPressure = "120/80",
                HeartRate = 72,
                Temperature = 98.6m,
                RecordedAt = DateTime.UtcNow
            };

            _fixture.CreateContext().Vitals.Add(vitals);
            await _fixture.CreateContext().SaveChangesAsync();
            return encounter;
        }

        private async Task<Patient> CreateTestPatientAsync()
        {
            var patient = new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.Parse("1980-01-01"),
                Gender = "Male",
                MedicalRecordNumber = "MRN123"
            };

            _fixture.CreateContext().Patients.Add(patient);
            await _fixture.CreateContext().SaveChangesAsync();
            return patient;
        }

        private async Task<Dictionary<string, object>> BuildClinicalContextAsync(int encounterId)
        {
            var context = new Dictionary<string, object>();
            var encounter = await _fixture.CreateContext().Encounters
                .Include(e => e.Patient)
                .Include(e => e.Provider)
                .FirstOrDefaultAsync(e => e.Id == encounterId);

            if (encounter != null)
            {
                context["Encounter"] = encounter;
                context["Patient"] = encounter.Patient;
                context["Provider"] = encounter.Provider;

                var vitals = await _fixture.CreateContext().Vitals
                    .Where(v => v.EncounterId == encounterId)
                    .OrderByDescending(v => v.RecordedAt)
                    .FirstOrDefaultAsync();

                if (vitals != null)
                {
                    context["Vitals"] = vitals;
                }

                // Add FHIR resources
                var fhirPatient = await _fhirService.GetPatientResourceAsync(encounter.Patient.Id);
                if (fhirPatient != null)
                {
                    context["FHIRPatient"] = fhirPatient;
                }
            }

            return context;
        }
    }
}
