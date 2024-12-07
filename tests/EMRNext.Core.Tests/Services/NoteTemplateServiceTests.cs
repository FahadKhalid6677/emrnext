using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;
using EMRNext.Core.Services.EhrServices;

namespace EMRNext.Core.Tests.Services
{
    public class NoteTemplateServiceTests
    {
        private readonly Mock<ILogger<NoteTemplateService>> _mockLogger;
        private readonly Mock<IGenericRepository<NoteTemplate>> _mockTemplateRepository;
        private readonly Mock<IGenericRepository<NoteTemplateInstance>> _mockInstanceRepository;
        private readonly Mock<IGenericRepository<Patient>> _mockPatientRepository;
        private readonly Mock<IGenericRepository<Provider>> _mockProviderRepository;

        public NoteTemplateServiceTests()
        {
            _mockLogger = new Mock<ILogger<NoteTemplateService>>();
            _mockTemplateRepository = new Mock<IGenericRepository<NoteTemplate>>();
            _mockInstanceRepository = new Mock<IGenericRepository<NoteTemplateInstance>>();
            _mockPatientRepository = new Mock<IGenericRepository<Patient>>();
            _mockProviderRepository = new Mock<IGenericRepository<Provider>>();
        }

        [Fact]
        public async Task CreateNoteTemplate_ValidTemplate_Succeeds()
        {
            // Arrange
            var noteTemplate = CreateValidNoteTemplate();
            _mockTemplateRepository
                .Setup(repo => repo.AddAsync(It.IsAny<NoteTemplate>()))
                .Returns(Task.CompletedTask);

            var service = CreateNoteTemplateService();

            // Act
            var result = await service.CreateNoteTemplateAsync(noteTemplate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(noteTemplate.TemplateName, result.TemplateName);
        }

        [Fact]
        public async Task CreateNoteTemplate_InvalidTemplate_ThrowsException()
        {
            // Arrange
            var invalidTemplate = new NoteTemplate(); // Empty template
            var service = CreateNoteTemplateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateNoteTemplateAsync(invalidTemplate)
            );
        }

        [Fact]
        public async Task GetNoteTemplatesBySpecialty_ExistingSpecialty_ReturnsTemplates()
        {
            // Arrange
            var specialty = "Cardiology";
            var templates = new List<NoteTemplate>
            {
                CreateValidNoteTemplate(specialty),
                CreateValidNoteTemplate(specialty)
            };

            _mockTemplateRepository
                .Setup(repo => repo.FindAsync(It.IsAny<Func<NoteTemplate, bool>>()))
                .ReturnsAsync(templates);

            var service = CreateNoteTemplateService();

            // Act
            var result = await service.GetNoteTemplatesBySpecialtyAsync(specialty);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, template => 
                Assert.Equal(specialty, template.MedicalSpecialty));
        }

        [Fact]
        public async Task CreateNoteTemplateInstance_ValidData_Succeeds()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var providerId = Guid.NewGuid();

            var template = CreateValidNoteTemplate();
            template.Id = templateId;

            var patient = new Patient { Id = patientId };
            var provider = new Provider { Id = providerId };

            _mockTemplateRepository
                .Setup(repo => repo.GetByIdAsync(templateId))
                .ReturnsAsync(template);

            _mockPatientRepository
                .Setup(repo => repo.GetByIdAsync(patientId))
                .ReturnsAsync(patient);

            _mockProviderRepository
                .Setup(repo => repo.GetByIdAsync(providerId))
                .ReturnsAsync(provider);

            var service = CreateNoteTemplateService();

            var fieldValues = template.Sections
                .SelectMany(s => s.Fields)
                .ToDictionary(f => f.Id, f => "Sample Value");

            // Act
            var result = await service.CreateNoteTemplateInstanceAsync(
                templateId, 
                patientId, 
                providerId, 
                fieldValues
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(templateId, result.NoteTemplateId);
            Assert.Equal(patientId, result.PatientId);
            Assert.Equal(providerId, result.ProviderId);
        }

        [Fact]
        public async Task ValidateNoteTemplateInstance_RequiredFieldsMissing_ReturnsFalse()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var template = CreateValidNoteTemplate();
            template.Id = templateId;
            template.Sections.First().Fields.First().IsRequired = true;

            _mockTemplateRepository
                .Setup(repo => repo.GetByIdAsync(templateId))
                .ReturnsAsync(template);

            var service = CreateNoteTemplateService();

            var templateInstance = new NoteTemplateInstance
            {
                NoteTemplateId = templateId,
                FilledFields = new List<NoteTemplateInstanceField>()
            };

            // Act
            var result = await service.ValidateNoteTemplateInstanceAsync(templateInstance);

            // Assert
            Assert.False(result);
        }

        private NoteTemplateService CreateNoteTemplateService()
        {
            return new NoteTemplateService(
                _mockLogger.Object,
                _mockTemplateRepository.Object,
                _mockInstanceRepository.Object,
                _mockPatientRepository.Object,
                _mockProviderRepository.Object
            );
        }

        private NoteTemplate CreateValidNoteTemplate(string specialty = "General")
        {
            return new NoteTemplate
            {
                TemplateName = "Sample Template",
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
    }
}
