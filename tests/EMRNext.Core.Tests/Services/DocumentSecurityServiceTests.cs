using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;
using EMRNext.Core.Services;
using EMRNext.Core.Services.Interfaces;

namespace EMRNext.Core.Tests.Services
{
    public class DocumentSecurityServiceTests
    {
        private readonly Mock<ILogger<DocumentSecurityService>> _mockLogger;
        private readonly Mock<IGenericRepository<SecureDocument>> _mockDocumentRepository;
        private readonly Mock<IGenericRepository<DocumentAccessPermission>> _mockPermissionRepository;
        private readonly Mock<IGenericRepository<DocumentAccessLog>> _mockAccessLogRepository;
        private readonly Mock<IEncryptionService> _mockEncryptionService;

        public DocumentSecurityServiceTests()
        {
            _mockLogger = new Mock<ILogger<DocumentSecurityService>>();
            _mockDocumentRepository = new Mock<IGenericRepository<SecureDocument>>();
            _mockPermissionRepository = new Mock<IGenericRepository<DocumentAccessPermission>>();
            _mockAccessLogRepository = new Mock<IGenericRepository<DocumentAccessLog>>();
            _mockEncryptionService = new Mock<IEncryptionService>();
        }

        [Fact]
        public async Task CreateSecureDocument_ValidDocument_Succeeds()
        {
            // Arrange
            var document = CreateValidSecureDocument();
            var encryptedContent = "encrypted-content";

            _mockEncryptionService
                .Setup(e => e.EncryptAsync(It.IsAny<string>()))
                .ReturnsAsync(encryptedContent);

            _mockDocumentRepository
                .Setup(repo => repo.AddAsync(It.IsAny<SecureDocument>()))
                .Returns(Task.CompletedTask);

            var service = CreateDocumentSecurityService();

            // Act
            var result = await service.CreateSecureDocumentAsync(document);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(encryptedContent, result.EncryptedContent);
            _mockEncryptionService.Verify(e => e.EncryptAsync(document.Content), Times.Once);
        }

        [Fact]
        public async Task CreateAccessPermission_ValidPermission_Succeeds()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessLevel = AccessLevel.Read;

            var document = new SecureDocument { Id = documentId };
            var user = new User { Id = userId };

            _mockDocumentRepository
                .Setup(repo => repo.GetByIdAsync(documentId))
                .ReturnsAsync(document);

            _mockPermissionRepository
                .Setup(repo => repo.AddAsync(It.IsAny<DocumentAccessPermission>()))
                .Returns(Task.CompletedTask);

            var service = CreateDocumentSecurityService();

            // Act
            var result = await service.CreateAccessPermissionAsync(documentId, userId, accessLevel);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(documentId, result.DocumentId);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(accessLevel, result.AccessLevel);
        }

        [Fact]
        public async Task AccessDocument_ValidPermission_LogsAccess()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var document = new SecureDocument { Id = documentId };
            var permission = new DocumentAccessPermission 
            { 
                DocumentId = documentId, 
                UserId = userId, 
                AccessLevel = AccessLevel.Read 
            };

            _mockDocumentRepository
                .Setup(repo => repo.GetByIdAsync(documentId))
                .ReturnsAsync(document);

            _mockPermissionRepository
                .Setup(repo => repo.FindSingleAsync(It.IsAny<Func<DocumentAccessPermission, bool>>()))
                .ReturnsAsync(permission);

            _mockAccessLogRepository
                .Setup(repo => repo.AddAsync(It.IsAny<DocumentAccessLog>()))
                .Returns(Task.CompletedTask);

            var service = CreateDocumentSecurityService();

            // Act
            var result = await service.AccessDocumentAsync(documentId, userId);

            // Assert
            Assert.True(result);
            _mockAccessLogRepository.Verify(repo => repo.AddAsync(It.Is<DocumentAccessLog>(
                log => log.DocumentId == documentId && log.UserId == userId
            )), Times.Once);
        }

        [Fact]
        public async Task AccessDocument_NoPermission_Fails()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _mockDocumentRepository
                .Setup(repo => repo.GetByIdAsync(documentId))
                .ReturnsAsync(new SecureDocument { Id = documentId });

            _mockPermissionRepository
                .Setup(repo => repo.FindSingleAsync(It.IsAny<Func<DocumentAccessPermission, bool>>()))
                .ReturnsAsync((DocumentAccessPermission)null);

            var service = CreateDocumentSecurityService();

            // Act
            var result = await service.AccessDocumentAsync(documentId, userId);

            // Assert
            Assert.False(result);
        }

        private DocumentSecurityService CreateDocumentSecurityService()
        {
            return new DocumentSecurityService(
                _mockLogger.Object,
                _mockDocumentRepository.Object,
                _mockPermissionRepository.Object,
                _mockAccessLogRepository.Object,
                _mockEncryptionService.Object
            );
        }

        private SecureDocument CreateValidSecureDocument()
        {
            return new SecureDocument
            {
                Id = Guid.NewGuid(),
                Content = "Sample document content",
                DocumentType = "Medical Note",
                CreatedBy = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
