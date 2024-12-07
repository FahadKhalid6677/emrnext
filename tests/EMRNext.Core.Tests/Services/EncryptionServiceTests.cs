using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EMRNext.Core.Services;
using EMRNext.Core.Configuration;

namespace EMRNext.Core.Tests.Services
{
    public class EncryptionServiceTests
    {
        private readonly Mock<ILogger<EncryptionService>> _mockLogger;
        private readonly EncryptionConfiguration _encryptionConfig;

        public EncryptionServiceTests()
        {
            _mockLogger = new Mock<ILogger<EncryptionService>>();

            // Create a configuration with a predefined encryption key for testing
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Encryption:Key", Convert.ToBase64String(GenerateTestKey()))
            });
            var configuration = configurationBuilder.Build();

            _encryptionConfig = new EncryptionConfiguration(configuration);
        }

        [Fact]
        public async Task EncryptAndDecrypt_ValidContent_ShouldSucceed()
        {
            // Arrange
            var originalContent = "Sensitive Medical Information";
            var encryptionService = CreateEncryptionService();

            // Act
            var encryptedContent = await encryptionService.EncryptAsync(originalContent);
            var decryptedContent = await encryptionService.DecryptAsync(encryptedContent);

            // Assert
            Assert.NotEqual(originalContent, encryptedContent);
            Assert.Equal(originalContent, decryptedContent);
        }

        [Fact]
        public async Task Encrypt_EmptyContent_ShouldThrowArgumentException()
        {
            // Arrange
            var encryptionService = CreateEncryptionService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => encryptionService.EncryptAsync(string.Empty)
            );
        }

        [Fact]
        public async Task Decrypt_InvalidContent_ShouldThrowSecurityException()
        {
            // Arrange
            var encryptionService = CreateEncryptionService();
            var invalidEncryptedContent = "InvalidBase64String";

            // Act & Assert
            await Assert.ThrowsAsync<EncryptionService.SecurityException>(
                () => encryptionService.DecryptAsync(invalidEncryptedContent)
            );
        }

        [Fact]
        public void GenerateEncryptionKey_ShouldProduceDifferentKeys()
        {
            // Arrange
            var encryptionService = CreateEncryptionService();

            // Act
            var key1 = encryptionService.GenerateEncryptionKey();
            var key2 = encryptionService.GenerateEncryptionKey();

            // Assert
            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public async Task ValidateDocumentIntegrity_ValidDocument_ShouldReturnTrue()
        {
            // Arrange
            var originalContent = "Integrity Check Document";
            var encryptionService = CreateEncryptionService();

            // Act
            var encryptedContent = await encryptionService.EncryptAsync(originalContent);
            var integrityResult = encryptionService.ValidateDocumentIntegrity(encryptedContent);

            // Assert
            Assert.True(integrityResult);
        }

        [Fact]
        public void ValidateDocumentIntegrity_TamperedDocument_ShouldReturnFalse()
        {
            // Arrange
            var encryptionService = CreateEncryptionService();
            var tamperedContent = "Tampered-Base64-Content";

            // Act
            var integrityResult = encryptionService.ValidateDocumentIntegrity(tamperedContent);

            // Assert
            Assert.False(integrityResult);
        }

        [Fact]
        public void EncryptionConfiguration_DefaultSettings_ShouldHaveReasonableDefaults()
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder.Build();
            var encryptionConfig = new EncryptionConfiguration(configuration);

            // Assert
            Assert.NotNull(encryptionConfig.EncryptionKey);
            Assert.Equal("AES-256-CBC", encryptionConfig.Algorithm);
            Assert.Equal(10 * 1024 * 1024, encryptionConfig.MaxDocumentSizeBytes);
        }

        private EncryptionService CreateEncryptionService()
        {
            return new EncryptionService(_mockLogger.Object, _encryptionConfig);
        }

        /// <summary>
        /// Generates a test encryption key
        /// </summary>
        private byte[] GenerateTestKey()
        {
            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.GenerateKey();
                return aes.Key;
            }
        }
    }
}
