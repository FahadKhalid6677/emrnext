using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EMRNext.Core.Services.EhrServices;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure.Persistence;
using EMRNext.Core.Tests.Fixtures;

namespace EMRNext.Core.Tests.Integration
{
    public class NoteTemplateServiceIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly INoteTemplateService _noteTemplateService;
        private readonly ApplicationDbContext _context;

        public NoteTemplateServiceIntegrationTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _context = fixture.Context;
            _noteTemplateService = fixture.ServiceProvider.GetRequiredService<INoteTemplateService>();
        }

        [Fact]
        public async Task CreateNoteTemplate_ValidTemplate_ShouldPersistInDatabase()
        {
            // Arrange
            var noteTemplate = CreateValidNoteTemplate();

            // Act
            var createdTemplate = await _noteTemplateService.CreateNoteTemplateAsync(noteTemplate);

            // Assert
            var savedTemplate = await _context.NoteTemplates
                .FindAsync(createdTemplate.Id);
            
            Assert.NotNull(savedTemplate);
            Assert.Equal(noteTemplate.TemplateName, savedTemplate.TemplateName);
            Assert.Equal(noteTemplate.MedicalSpecialty, savedTemplate.MedicalSpecialty);
        }

        [Fact]
        public async Task CreateNoteTemplateInstance_ValidData_ShouldPersistInDatabase()
        {
            // Arrange
            var patient = CreateTestPatient();
            var provider = CreateTestProvider();
            var template = CreateValidNoteTemplate();

            await _context.Patients.AddAsync(patient);
            await _context.Providers.AddAsync(provider);
            await _context.NoteTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var fieldValues = template.Sections
                .SelectMany(s => s.Fields)
                .ToDictionary(f => f.Id, f => "Sample Value");

            // Act
            var createdInstance = await _noteTemplateService.CreateNoteTemplateInstanceAsync(
                template.Id, 
                patient.Id, 
                provider.Id, 
                fieldValues
            );

            // Assert
            var savedInstance = await _context.NoteTemplateInstances
                .FindAsync(createdInstance.Id);
            
            Assert.NotNull(savedInstance);
            Assert.Equal(template.Id, savedInstance.NoteTemplateId);
            Assert.Equal(patient.Id, savedInstance.PatientId);
            Assert.Equal(provider.Id, savedInstance.ProviderId);
        }

        [Fact]
        public async Task GetNoteTemplatesBySpecialty_ExistingSpecialty_ShouldReturnTemplates()
        {
            // Arrange
            var specialty = "Cardiology";
            var template1 = CreateValidNoteTemplate(specialty);
            var template2 = CreateValidNoteTemplate(specialty);

            await _context.NoteTemplates.AddRangeAsync(template1, template2);
            await _context.SaveChangesAsync();

            // Act
            var templates = await _noteTemplateService.GetNoteTemplatesBySpecialtyAsync(specialty);

            // Assert
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count());
            Assert.All(templates, t => Assert.Equal(specialty, t.MedicalSpecialty));
        }

        [Fact]
        public async Task ValidateNoteTemplateInstance_RequiredFieldsMissing_ShouldReturnFalse()
        {
            // Arrange
            var template = CreateValidNoteTemplate();
            template.Sections.First().Fields.First().IsRequired = true;

            await _context.NoteTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var templateInstance = new NoteTemplateInstance
            {
                NoteTemplateId = template.Id,
                FilledFields = new List<NoteTemplateInstanceField>()
            };

            // Act
            var isValid = await _noteTemplateService.ValidateNoteTemplateInstanceAsync(templateInstance);

            // Assert
            Assert.False(isValid);
        }

        private NoteTemplate CreateValidNoteTemplate(string specialty = "General")
        {
            return new NoteTemplate
            {
                TemplateName = $"Sample Template - {Guid.NewGuid()}",
                MedicalSpecialty = specialty,
                Sections = new List<NoteTemplateSection>
                {
                    new NoteTemplateSection
                    {
                        SectionName = "Patient Information",
                        Fields = new List<NoteTemplateSectionField>
                        {
                            new NoteTemplateSectionField
                            {
                                FieldName = "Patient Name",
                                FieldType = "Text",
                                IsRequired = false
                            }
                        }
                    }
                }
            };
        }

        private Patient CreateTestPatient()
        {
            return new Patient
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Patient",
                DateOfBirth = DateTime.Now.AddYears(-30)
            };
        }

        private Provider CreateTestProvider()
        {
            return new Provider
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "Provider",
                Specialty = "General"
            };
        }
    }
}
