using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;

namespace EMRNext.Tests.Services
{
    public class TemplateManagementServiceTests
    {
        private readonly Mock<EMRDbContext> _contextMock;
        private readonly Mock<ISecurityService> _securityServiceMock;
        private readonly Mock<IClinicalDecisionSupportService> _clinicalServiceMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly TemplateManagementService _service;

        public TemplateManagementServiceTests()
        {
            _contextMock = new Mock<EMRDbContext>();
            _securityServiceMock = new Mock<ISecurityService>();
            _clinicalServiceMock = new Mock<IClinicalDecisionSupportService>();
            _auditServiceMock = new Mock<IAuditService>();
            _notificationServiceMock = new Mock<INotificationService>();

            _service = new TemplateManagementService(
                _contextMock.Object,
                _securityServiceMock.Object,
                _clinicalServiceMock.Object,
                _auditServiceMock.Object,
                _notificationServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateTemplate_ValidTemplate_CreatesSuccessfully()
        {
            // Arrange
            var template = new ClinicalTemplate
            {
                Name = "Test Template",
                Description = "Test Description",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "test_user"
            };

            var templates = new List<ClinicalTemplate>();
            var dbSetMock = CreateDbSetMock(templates);
            _contextMock.Setup(c => c.ClinicalTemplates).Returns(dbSetMock.Object);

            // Act
            var result = await _service.CreateTemplateAsync(template);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be(1);
            result.IsActive.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            
            _auditServiceMock.Verify(
                x => x.LogActivityAsync("Template", It.IsAny<int>(), "Create", "test_user"),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTemplate_PublishedTemplate_CreatesNewVersion()
        {
            // Arrange
            var existingTemplate = new ClinicalTemplate
            {
                Id = 1,
                Name = "Original Template",
                Version = 1,
                IsPublished = true,
                CreatedBy = "test_user"
            };

            var templates = new List<ClinicalTemplate> { existingTemplate };
            var dbSetMock = CreateDbSetMock(templates);
            _contextMock.Setup(c => c.ClinicalTemplates).Returns(dbSetMock.Object);

            _securityServiceMock.Setup(s => s.HasPermissionAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var updateTemplate = new ClinicalTemplate
            {
                Id = 1,
                Name = "Updated Template",
                ModifiedBy = "test_user"
            };

            // Act
            var result = await _service.UpdateTemplateAsync(updateTemplate);

            // Assert
            result.Should().NotBeNull();
            result.Version.Should().Be(2);
            result.IsPublished.Should().BeFalse();
            result.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GetTemplatesForEncounter_WithValidCriteria_ReturnsMatchingTemplates()
        {
            // Arrange
            var encounterId = 1;
            var encounter = new Encounter
            {
                Id = encounterId,
                Provider = new Provider { Specialty = "Internal Medicine" }
            };

            var templates = new List<ClinicalTemplate>
            {
                new ClinicalTemplate
                {
                    Id = 1,
                    Name = "Template 1",
                    SpecialtyType = "Internal Medicine",
                    IsPublished = true,
                    IsActive = true
                },
                new ClinicalTemplate
                {
                    Id = 2,
                    Name = "Template 2",
                    SpecialtyType = "Internal Medicine",
                    IsPublished = false,
                    IsActive = true
                }
            };

            var encounters = new List<Encounter> { encounter };
            var encounterDbSetMock = CreateDbSetMock(encounters);
            var templateDbSetMock = CreateDbSetMock(templates);

            _contextMock.Setup(c => c.Encounters).Returns(encounterDbSetMock.Object);
            _contextMock.Setup(c => c.ClinicalTemplates).Returns(templateDbSetMock.Object);

            _clinicalServiceMock.Setup(c => c.EvaluateRuleAsync(
                It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new RuleEvaluation { IsValid = true });

            // Act
            var result = await _service.GetTemplatesForEncounterAsync(encounterId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().Contain(t => t.IsPublished && t.IsActive);
        }

        [Fact]
        public async Task RenderTemplate_WithValidData_GeneratesCorrectOutput()
        {
            // Arrange
            var templateId = 1;
            var template = new ClinicalTemplate
            {
                Id = templateId,
                Name = "Test Template",
                Sections = new List<TemplateSection>
                {
                    new TemplateSection
                    {
                        Name = "Section 1",
                        Content = "Patient Name: {{PatientName}}",
                        OrderIndex = 1
                    }
                }
            };

            var data = new Dictionary<string, object>
            {
                { "PatientName", "John Doe" }
            };

            var templates = new List<ClinicalTemplate> { template };
            var dbSetMock = CreateDbSetMock(templates);
            _contextMock.Setup(c => c.ClinicalTemplates).Returns(dbSetMock.Object);

            // Act
            var result = await _service.RenderTemplateAsync(templateId, data);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("John Doe");
        }

        [Fact]
        public async Task ValidateTemplate_WithInvalidStructure_ReturnsValidationErrors()
        {
            // Arrange
            var template = new ClinicalTemplate
            {
                // Missing required fields
                Description = "Test Description"
            };

            // Act
            var validationResults = await _service.ValidateTemplateAsync(template.Id);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().ContainKey("Structure");
            validationResults["Structure"].Should().Contain(e => e.Contains("name is required"));
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();
            
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            
            return dbSetMock;
        }
    }
}
