using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;
using EMRNext.Core.Security.Services;

namespace EMRNext.Core.Tests.Security
{
    public class SecurityServicesTests
    {
        private readonly Mock<ILogger<AuthorizationService>> _mockLogger;
        private readonly Mock<IGenericRepository<User>> _mockUserRepository;
        private readonly Mock<IGenericRepository<Role>> _mockRoleRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public SecurityServicesTests()
        {
            _mockLogger = new Mock<ILogger<AuthorizationService>>();
            _mockUserRepository = new Mock<IGenericRepository<User>>();
            _mockRoleRepository = new Mock<IGenericRepository<Role>>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup mock configuration for JWT
            _mockConfiguration
                .Setup(c => c["Jwt:SecretKey"])
                .Returns("TestSecretKeyForJWTTokenGeneration123!@#");
            _mockConfiguration
                .Setup(c => c["Jwt:Issuer"])
                .Returns("EMRNextTestIssuer");
            _mockConfiguration
                .Setup(c => c["Jwt:Audience"])
                .Returns("EMRNextTestAudience");
        }

        [Fact]
        public async Task AuthorizationService_ValidateResourceAccess_Succeeds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var adminRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Administrator",
                Permissions = new List<Permission>
                {
                    new Permission
                    {
                        ResourceType = "Patient",
                        AllowedOperations = new List<string> { "Read", "Write", "Update", "Delete" }
                    }
                }
            };

            var user = new User
            {
                Id = userId,
                Username = "admin",
                Roles = new List<Role> { adminRole }
            };

            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var authService = new AuthorizationService(
                _mockLogger.Object, 
                _mockUserRepository.Object,
                _mockRoleRepository.Object
            );

            // Act
            var result = await authService.AuthorizeResourceAccessAsync(
                userId, 
                "Patient", 
                "Write"
            );

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TokenService_GenerateAndValidateToken_Succeeds()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@emrnext.com",
                Roles = new List<Role>
                {
                    new Role { Name = "User" }
                }
            };

            var tokenService = new TokenService(_mockConfiguration.Object);

            // Act
            var token = tokenService.GenerateJwtToken(user);
            var principal = tokenService.ValidateToken(token);

            // Assert
            Assert.NotNull(token);
            Assert.NotNull(principal);
            Assert.Contains(principal.Claims, 
                c => c.Type == System.Security.Claims.ClaimTypes.Name 
                     && c.Value == user.Username);
        }

        [Fact]
        public void EncryptionService_EncryptAndDecrypt_Succeeds()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<EncryptionService>>();
            var mockConfig = new Mock<IConfiguration>();
            mockConfig
                .Setup(c => c["Encryption:SecretKey"])
                .Returns(Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes("TestEncryptionKey123!@#")
                ));

            var encryptionService = new EncryptionService(
                mockConfig.Object, 
                mockLogger.Object
            );

            var originalText = "Sensitive Patient Data";

            // Act
            var encryptedText = encryptionService.Encrypt(originalText);
            var decryptedText = encryptionService.Decrypt(encryptedText);

            // Assert
            Assert.NotEqual(originalText, encryptedText);
            Assert.Equal(originalText, decryptedText);
        }

        [Fact]
        public void EncryptionService_PasswordHashing_Succeeds()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<EncryptionService>>();
            var mockConfig = new Mock<IConfiguration>();
            mockConfig
                .Setup(c => c["Encryption:SecretKey"])
                .Returns(Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes("TestEncryptionKey123!@#")
                ));

            var encryptionService = new EncryptionService(
                mockConfig.Object, 
                mockLogger.Object
            );

            var password = "SecurePassword123!@#";

            // Act
            var hashedPassword = encryptionService.HashPassword(password);
            var verificationResult1 = encryptionService.VerifyPassword(password, hashedPassword);
            var verificationResult2 = encryptionService.VerifyPassword("WrongPassword", hashedPassword);

            // Assert
            Assert.NotNull(hashedPassword);
            Assert.True(verificationResult1);
            Assert.False(verificationResult2);
        }
    }
}
