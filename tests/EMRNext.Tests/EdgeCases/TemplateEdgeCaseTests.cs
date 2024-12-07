using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EMRNext.Tests.EdgeCases
{
    public class TemplateEdgeCaseTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ITemplateManagementService _templateService;
        private readonly IVariableResolutionService _variableService;
        private readonly IClinicalDecisionSupportService _clinicalService;

        public TemplateEdgeCaseTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            var scope = _fixture.ServiceProvider.CreateScope();
            _templateService = scope.ServiceProvider.GetRequiredService<ITemplateManagementService>();
            _variableService = scope.ServiceProvider.GetRequiredService<IVariableResolutionService>();
            _clinicalService = scope.ServiceProvider.GetRequiredService<IClinicalDecisionSupportService>();
        }

        [Fact]
        public async Task Template_WithMissingClinicalData_HandlesGracefully()
        {
            // Arrange
            var template = await CreateTemplateWithClinicalDataAsync();
            var encounter = await CreateEncounterWithoutVitalsAsync();

            // Act
            var context = await BuildPartialContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Data Not Available");
            renderedContent.Should().NotContain("{{");
            renderedContent.Should().NotContain("}}");
        }

        [Fact]
        public async Task ConcurrentTemplateEditing_HandlesVersionConflicts()
        {
            // Arrange
            var template = await CreateTestTemplateAsync();
            var user1Context = new Dictionary<string, object> { { "UserId", "user1" } };
            var user2Context = new Dictionary<string, object> { { "UserId", "user2" } };

            // Act
            // Simulate concurrent edits
            var user1Task = _templateService.UpdateTemplateAsync(new ClinicalTemplate
            {
                Id = template.Id,
                Name = "Updated by User 1",
                ModifiedBy = "user1"
            });

            var user2Task = _templateService.UpdateTemplateAsync(new ClinicalTemplate
            {
                Id = template.Id,
                Name = "Updated by User 2",
                ModifiedBy = "user2"
            });

            // Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => 
                Task.WhenAll(user1Task, user2Task));
        }

        [Fact]
        public async Task Template_WithInvalidDataFormats_HandlesGracefully()
        {
            // Arrange
            var template = await CreateTemplateWithDataValidationAsync();
            var context = new Dictionary<string, object>
            {
                { "NumericField", "not a number" },
                { "DateField", "invalid date" },
                { "CodedField", "invalid code" }
            };

            // Act
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Invalid Format");
            renderedContent.Should().NotContain("{{");
        }

        [Fact]
        public async Task Template_WithConflictingInformation_HandlesAppropriately()
        {
            // Arrange
            var template = await CreateTemplateWithConflictingDataAsync();
            var encounter = await CreateEncounterWithConflictingDataAsync();

            // Act
            var context = await BuildConflictingContextAsync(encounter.Id);
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            renderedContent.Should().NotBeNull();
            renderedContent.Should().Contain("Data Conflict Detected");
            renderedContent.Should().Contain("Please Verify");
        }

        [Fact]
        public async Task LegacyDataMigration_HandlesOldFormats()
        {
            // Arrange
            var legacyTemplate = await CreateLegacyTemplateAsync();
            var context = await BuildLegacyContextAsync();

            // Act
            var migratedTemplate = await _templateService.MigrateLegacyTemplateAsync(legacyTemplate);
            var renderedContent = await _templateService.RenderTemplateAsync(migratedTemplate.Id, context);

            // Assert
            migratedTemplate.Should().NotBeNull();
            renderedContent.Should().NotBeNull();
            renderedContent.Should().NotContain("LegacyFormat");
        }

        [Fact]
        public async Task NetworkInterruption_HandlesRecoveryGracefully()
        {
            // Arrange
            var template = await CreateTestTemplateAsync();
            var context = await BuildTestContextAsync();

            // Simulate network interruption
            _fixture.SimulateNetworkInterruption();

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() =>
                _templateService.RenderTemplateAsync(template.Id, context));

            // Verify recovery after network restoration
            _fixture.RestoreNetwork();
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);
            renderedContent.Should().NotBeNull();
        }

        [Fact]
        public async Task SessionTimeout_HandlesUserStateAppropriately()
        {
            // Arrange
            var template = await CreateTestTemplateAsync();
            var context = new Dictionary<string, object>
            {
                { "SessionId", "expired_session" },
                { "UserId", "test_user" }
            };

            // Act & Assert
            await Assert.ThrowsAsync<SessionExpiredException>(() =>
                _templateService.UpdateTemplateAsync(template));

            // Verify session renewal
            context["SessionId"] = "new_session";
            var updatedTemplate = await _templateService.UpdateTemplateAsync(template);
            updatedTemplate.Should().NotBeNull();
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

            template = await _templateService.CreateTemplateAsync(template);

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Vital Signs",
                Content = @"
                    BP: {{Vitals.BloodPressure ?? 'Data Not Available'}}
                    HR: {{Vitals.HeartRate ?? 'Data Not Available'}}
                    Temp: {{Vitals.Temperature ?? 'Data Not Available'}}
                ",
                OrderIndex = 1
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateTemplateWithDataValidationAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Data Validation Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "test_user"
            };

            template = await _templateService.CreateTemplateAsync(template);

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Validated Data",
                Content = @"
                    Numeric: {{NumericField ?? 'Invalid Format'}}
                    Date: {{DateField ?? 'Invalid Format'}}
                    Code: {{CodedField ?? 'Invalid Format'}}
                ",
                OrderIndex = 1
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateTemplateWithConflictingDataAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Conflicting Data Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "test_user"
            };

            template = await _templateService.CreateTemplateAsync(template);

            await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Medication Reconciliation",
                Content = @"
                    {{#if MedicationConflict}}
                        <div class='alert'>Data Conflict Detected</div>
                        <div>Please Verify: {{ConflictDetails}}</div>
                    {{/if}}
                ",
                OrderIndex = 1
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateLegacyTemplateAsync()
        {
            // Simulate a legacy template format
            return new ClinicalTemplate
            {
                Name = "Legacy Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                Content = "LegacyFormat:{{OldVariable}}",
                Version = 1,
                CreatedBy = "legacy_system"
            };
        }

        private async Task<Encounter> CreateEncounterWithoutVitalsAsync()
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

        private async Task<Encounter> CreateEncounterWithConflictingDataAsync()
        {
            var encounter = await CreateEncounterWithoutVitalsAsync();

            // Add conflicting medications
            var medications = new[]
            {
                new Medication
                {
                    EncounterId = encounter.Id,
                    PatientId = encounter.Patient.Id,
                    Name = "Medication A",
                    Dose = "10mg",
                    Source = "EHR"
                },
                new Medication
                {
                    EncounterId = encounter.Id,
                    PatientId = encounter.Patient.Id,
                    Name = "Medication A",
                    Dose = "20mg",
                    Source = "Claims"
                }
            };

            _fixture.CreateContext().Medications.AddRange(medications);
            await _fixture.CreateContext().SaveChangesAsync();
            return encounter;
        }

        private async Task<Dictionary<string, object>> BuildPartialContextAsync(int encounterId)
        {
            return new Dictionary<string, object>
            {
                { "EncounterId", encounterId },
                // Vitals intentionally omitted
            };
        }

        private async Task<Dictionary<string, object>> BuildConflictingContextAsync(int encounterId)
        {
            var medications = await _fixture.CreateContext().Medications
                .Where(m => m.EncounterId == encounterId)
                .ToListAsync();

            return new Dictionary<string, object>
            {
                { "EncounterId", encounterId },
                { "MedicationConflict", true },
                { "ConflictDetails", "Medication A: Dose conflict (10mg vs 20mg)" },
                { "Medications", medications }
            };
        }

        private async Task<Dictionary<string, object>> BuildLegacyContextAsync()
        {
            return new Dictionary<string, object>
            {
                { "OldVariable", "Legacy Value" },
                { "LegacyFormat", true }
            };
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
