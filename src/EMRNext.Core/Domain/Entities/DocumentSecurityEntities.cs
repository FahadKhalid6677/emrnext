using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace EMRNext.Core.Domain.Entities
{
    /// <summary>
    /// Represents a secure medical document with advanced access controls
    /// </summary>
    public class SecureDocument : BaseIntEntity
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        [StringLength(200)]
        public string DocumentName { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; }

        [Required]
        public Guid OwnerId { get; set; }
        public Patient Owner { get; set; }

        [Required]
        public string EncryptedContent { get; set; }
        public string EncryptionKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastAccessedAt { get; set; }

        [Required]
        public DocumentAccessLevel AccessLevel { get; set; }

        public List<DocumentAccessPermission> AccessPermissions { get; set; } = new List<DocumentAccessPermission>();

        /// <summary>
        /// Generates a secure encryption key
        /// </summary>
        public static string GenerateEncryptionKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[32];
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }

        /// <summary>
        /// Encrypts document content
        /// </summary>
        public void EncryptContent(string content)
        {
            if (string.IsNullOrEmpty(EncryptionKey))
                EncryptionKey = GenerateEncryptionKey();

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(EncryptionKey);
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                    EncryptedContent = Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        /// <summary>
        /// Decrypts document content
        /// </summary>
        public string DecryptContent()
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(EncryptionKey);

                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] encryptedBytes = Convert.FromBase64String(EncryptedContent);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }
    }

    /// <summary>
    /// Represents access permissions for a secure document
    /// </summary>
    public class DocumentAccessPermission : BaseIntEntity
    {
        public Guid SecureDocumentId { get; set; }
        public SecureDocument SecureDocument { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DocumentAccessLevel AccessLevel { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }

    /// <summary>
    /// Defines access levels for medical documents
    /// </summary>
    public enum DocumentAccessLevel
    {
        None = 0,
        Read = 1,
        Write = 2,
        FullAccess = 3
    }

    /// <summary>
    /// Tracks document access and audit trail
    /// </summary>
    public class DocumentAccessLog : BaseIntEntity
    {
        public Guid SecureDocumentId { get; set; }
        public SecureDocument SecureDocument { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DocumentAccessType AccessType { get; set; }

        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
        public string IPAddress { get; set; }
    }

    /// <summary>
    /// Defines types of document access
    /// </summary>
    public enum DocumentAccessType
    {
        View,
        Download,
        Edit,
        Share
    }
}
