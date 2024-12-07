using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;

namespace EMRNext.Tests.Security
{
    public class TemplateSecurityTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly ITemplateManagementService _templateService;
        private readonly ISecurityService _securityService;

        public TemplateSecurityTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
            var scope = _fixture.ServiceProvider.CreateScope();
            _templateService = scope.ServiceProvider.GetRequiredService<ITemplateManagementService>();
            _securityService = scope.ServiceProvider.GetRequiredService<ISecurityService>();
        }

        [Fact]
        public async Task Template_AccessControl_EnforcesPermissions()
        {
            // Arrange
            var template = await CreateRestrictedTemplateAsync();
            
            // Test authorized access
            var authorizedUser = new TestUser
            {
                Id = "auth_user",
                Role = "physician",
                Department = "Internal Medicine"
            };

            // Test unauthorized access
            var unauthorizedUser = new TestUser
            {
                Id = "unauth_user",
                Role = "nurse",
                Department = "Pediatrics"
            };

            // Act & Assert
            // Authorized user should have access
            var authorizedAccess = await _templateService.HasAccessAsync(
                template.Id, authorizedUser.Id, "view");
            authorizedAccess.Should().BeTrue();

            // Unauthorized user should not have access
            var unauthorizedAccess = await _templateService.HasAccessAsync(
                template.Id, unauthorizedUser.Id, "edit");
            unauthorizedAccess.Should().BeFalse();
        }

        [Fact]
        public async Task Template_PHIFields_AreEncrypted()
        {
            // Arrange
            var template = await CreateTemplateWithPHIAsync();
            var context = await CreateTestContextWithPHIAsync();

            // Act
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            // PHI fields should be encrypted in storage
            var templateFromDb = await _templateService.GetTemplateByIdAsync(template.Id);
            foreach (var section in templateFromDb.Sections)
            {
                foreach (var field in section.Fields)
                {
                    if (IsPHIField(field))
                    {
                        field.IsEncrypted.Should().BeTrue();
                    }
                }
            }

            // PHI should be properly decrypted in rendered content
            renderedContent.Should().Contain("XXXX"); // Masked SSN
            renderedContent.Should().NotContain(context["SSN"].ToString());
        }

        [Fact]
        public async Task Template_AuditLogging_TracksAccess()
        {
            // Arrange
            var template = await CreateTestTemplateAsync();
            var user = new TestUser { Id = "test_user", Role = "physician" };

            // Act
            await _templateService.GetTemplateByIdAsync(template.Id);
            await _templateService.RenderTemplateAsync(template.Id, new Dictionary<string, object>());

            // Assert
            var auditLogs = await _securityService.GetAuditLogsAsync(
                entityType: "Template",
                entityId: template.Id.ToString());

            auditLogs.Should().NotBeEmpty();
            auditLogs.Should().Contain(log => log.Action == "View");
            auditLogs.Should().Contain(log => log.Action == "Render");
        }

        [Fact]
        public async Task Template_VersionControl_EnforcesAccess()
        {
            // Arrange
            var template = await CreateTestTemplateAsync();
            var authorizedUser = new TestUser { Id = "auth_user", Role = "physician" };
            var unauthorizedUser = new TestUser { Id = "unauth_user", Role = "nurse" };

            // Act & Assert
            // Authorized user can create new version
            var authorizedVersion = await _templateService.CreateVersionAsync(template.Id);
            authorizedVersion.Should().NotBeNull();
            authorizedVersion.Version.Should().Be(2);

            // Unauthorized user cannot create new version
            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _templateService.CreateVersionAsync(template.Id));
        }

        [Fact]
        public async Task Template_DataProtection_SecuresTransmission()
        {
            // Arrange
            var template = await CreateTemplateWithSensitiveDataAsync();
            var context = await CreateTestContextWithSensitiveDataAsync();

            // Act
            var renderedContent = await _templateService.RenderTemplateAsync(template.Id, context);

            // Assert
            // Verify sensitive data is properly protected
            renderedContent.Should().NotContain("password");
            renderedContent.Should().NotContain("secret");
            renderedContent.Should().Contain("***"); // Masked sensitive data
        }

        [Fact]
        public async Task Template_HIPAA_ComplianceValidation()
        {
            // Arrange
            var template = await CreateHIPAATemplateAsync();

            // Act
            var validationResults = await _templateService.ValidateTemplateAsync(template.Id);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().ContainKey("HIPAA");
            
            // Check specific HIPAA requirements
            var hipaaValidation = validationResults["HIPAA"];
            hipaaValidation.Should().Contain(v => v.Contains("minimum necessary"));
            hipaaValidation.Should().Contain(v => v.Contains("encryption"));
            hipaaValidation.Should().Contain(v => v.Contains("audit logging"));
        }

        private async Task<ClinicalTemplate> CreateRestrictedTemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "Restricted Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                AllowedRoles = "[\"physician\"]",
                AllowedDepartments = "[\"Internal Medicine\"]",
                CreatedBy = "test_user"
            };

            return await _templateService.CreateTemplateAsync(template);
        }

        private async Task<ClinicalTemplate> CreateTemplateWithPHIAsync()
        {
            var template = await CreateTestTemplateAsync();
            
            var section = await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Patient Information",
                OrderIndex = 1
            });

            await _templateService.AddFieldAsync(section.Id, new TemplateField
            {
                Name = "SSN",
                Label = "Social Security Number",
                FieldType = "ssn",
                IsEncrypted = true,
                IsPHI = true,
                OrderIndex = 1
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateTemplateWithSensitiveDataAsync()
        {
            var template = await CreateTestTemplateAsync();
            
            var section = await _templateService.AddSectionAsync(template.Id, new TemplateSection
            {
                Name = "Sensitive Information",
                OrderIndex = 1
            });

            await _templateService.AddFieldAsync(section.Id, new TemplateField
            {
                Name = "Password",
                Label = "Password",
                FieldType = "password",
                IsEncrypted = true,
                IsSensitive = true,
                OrderIndex = 1
            });

            return template;
        }

        private async Task<ClinicalTemplate> CreateHIPAATemplateAsync()
        {
            var template = new ClinicalTemplate
            {
                Name = "HIPAA Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                RequiresHIPAACompliance = true,
                CreatedBy = "test_user"
            };

            return await _templateService.CreateTemplateAsync(template);
        }

        private async Task<Dictionary<string, object>> CreateTestContextWithPHIAsync()
        {
            return new Dictionary<string, object>
            {
                { "SSN", "123-45-6789" },
                { "DOB", "1980-01-01" },
                { "Address", "123 Main St" }
            };
        }

        private async Task<Dictionary<string, object>> CreateTestContextWithSensitiveDataAsync()
        {
            return new Dictionary<string, object>
            {
                { "Password", "secret123" },
                { "ApiKey", "abc123xyz" },
                { "SecretQuestion", "Mother's maiden name" }
            };
        }

        private bool IsPHIField(TemplateField field)
        {
            var phiTypes = new[] { "ssn", "mrn", "dob", "address", "phone" };
            return phiTypes.Contains(field.FieldType.ToLower());
        }

        private async Task<ClinicalTemplate> CreateTestTemplateAsync()
        {
            return await _templateService.CreateTemplateAsync(new ClinicalTemplate
            {
                Name = "Test Template",
                Category = "Progress Note",
                SpecialtyType = "Internal Medicine",
                CreatedBy = "test_user"
            });
        }

        private class TestUser
        {
            public string Id { get; set; }
            public string Role { get; set; }
            public string Department { get; set; }
        }
    }
}
