using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services.Interfaces;
using EMRNext.Core.Infrastructure.Persistence;
using EMRNext.Core.Tests.Fixtures;

namespace EMRNext.Core.Tests.Integration
{
    public class DocumentSecurityIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IDocumentSecurityService _documentSecurityService;

        public DocumentSecurityIntegrationTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _context = fixture.Context;
            _encryptionService = fixture.ServiceProvider.GetRequiredService<IEncryptionService>();
            _documentSecurityService = fixture.ServiceProvider.GetRequiredService<IDocumentSecurityService>();
        }

        [Fact]
        public async Task CreateSecureDocument_WithEncryption_ShouldPersistEncryptedDocument()
        {
            // Arrange
            var documentContent = "Sensitive Patient Medical Record";
            var provider = CreateTestProvider();
            await _context.Providers.AddAsync(provider);
            await _context.SaveChangesAsync();

            var secureDocument = new SecureDocument
            {
                Content = documentContent,
                DocumentType = "Medical Record",
                CreatedBy = provider.Id,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var createdDocument = await _documentSecurityService.CreateSecureDocumentAsync(secureDocument);

            // Assert
            Assert.NotNull(createdDocument);
            Assert.NotEqual(documentContent, createdDocument.EncryptedContent);
            
            var savedDocument = await _context.SecureDocuments.FindAsync(createdDocument.Id);
            Assert.NotNull(savedDocument);
            Assert.NotEqual(documentContent, savedDocument.EncryptedContent);
        }

        [Fact]
        public async Task CreateAccessPermission_ShouldRestrictDocumentAccess()
        {
            // Arrange
            var document = CreateTestSecureDocument();
            var authorizedUser = CreateTestUser();
            var unauthorizedUser = CreateTestUser();

            await _context.SecureDocuments.AddAsync(document);
            await _context.Users.AddRangeAsync(authorizedUser, unauthorizedUser);
            await _context.SaveChangesAsync();

            // Create access permission for authorized user
            var accessPermission = await _documentSecurityService.CreateAccessPermissionAsync(
                document.Id, 
                authorizedUser.Id, 
                AccessLevel.Read
            );

            // Act
            var authorizedAccess = await _documentSecurityService.AccessDocumentAsync(
                document.Id, 
                authorizedUser.Id
            );

            var unauthorizedAccess = await _documentSecurityService.AccessDocumentAsync(
                document.Id, 
                unauthorizedUser.Id
            );

            // Assert
            Assert.True(authorizedAccess);
            Assert.False(unauthorizedAccess);
        }

        [Fact]
        public async Task DocumentAccessLog_ShouldRecordAccessAttempts()
        {
            // Arrange
            var document = CreateTestSecureDocument();
            var user = CreateTestUser();

            await _context.SecureDocuments.AddAsync(document);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await _documentSecurityService.CreateAccessPermissionAsync(
                document.Id, 
                user.Id, 
                AccessLevel.Read
            );

            // Act
            await _documentSecurityService.AccessDocumentAsync(document.Id, user.Id);

            // Assert
            var accessLogs = _context.DocumentAccessLogs
                .Where(log => log.DocumentId == document.Id && log.UserId == user.Id)
                .ToList();

            Assert.NotEmpty(accessLogs);
            Assert.Single(accessLogs);
        }

        [Fact]
        public async Task DecryptDocument_WithValidPermission_ShouldSucceed()
        {
            // Arrange
            var documentContent = "Confidential Medical Information";
            var document = CreateTestSecureDocument(documentContent);
            var user = CreateTestUser();

            await _context.SecureDocuments.AddAsync(document);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await _documentSecurityService.CreateAccessPermissionAsync(
                document.Id, 
                user.Id, 
                AccessLevel.Read
            );

            // Act
            var decryptedContent = await _documentSecurityService.DecryptDocumentAsync(
                document.Id, 
                user.Id
            );

            // Assert
            Assert.Equal(documentContent, decryptedContent);
        }

        private SecureDocument CreateTestSecureDocument(string content = "Test Document")
        {
            return new SecureDocument
            {
                Id = Guid.NewGuid(),
                Content = content,
                DocumentType = "Test",
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
        }

        private User CreateTestUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                UserName = $"testuser_{Guid.NewGuid():N}",
                Email = $"testuser_{Guid.NewGuid():N}@example.com"
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
