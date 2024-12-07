using System.Threading.Tasks;

namespace EMRNext.Core.Services.Interfaces
{
    /// <summary>
    /// Provides cryptographic services for secure document encryption and decryption
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the given content using a secure encryption algorithm
        /// </summary>
        /// <param name="content">The plain text content to encrypt</param>
        /// <returns>Encrypted content as a base64 encoded string</returns>
        Task<string> EncryptAsync(string content);

        /// <summary>
        /// Decrypts the given encrypted content
        /// </summary>
        /// <param name="encryptedContent">The encrypted content as a base64 encoded string</param>
        /// <returns>Decrypted plain text content</returns>
        Task<string> DecryptAsync(string encryptedContent);

        /// <summary>
        /// Generates a secure encryption key
        /// </summary>
        /// <returns>A cryptographically secure encryption key</returns>
        string GenerateEncryptionKey();

        /// <summary>
        /// Validates the integrity of an encrypted document
        /// </summary>
        /// <param name="encryptedContent">The encrypted content to validate</param>
        /// <returns>True if the content is valid, false otherwise</returns>
        bool ValidateDocumentIntegrity(string encryptedContent);
    }
}
