using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;
using System.Net;

namespace EMRNext.Core.Services
{
    public interface IDocumentSecurityService
    {
        Task<SecureDocument> CreateSecureDocumentAsync(
            Guid ownerId, 
            string documentName, 
            string documentType, 
            string content, 
            DocumentAccessLevel accessLevel);

        Task<string> AccessDocumentAsync(
            Guid documentId, 
            Guid userId, 
            string ipAddress);

        Task<bool> UpdateDocumentAccessPermissionAsync(
            Guid documentId, 
            Guid userId, 
            DocumentAccessLevel accessLevel, 
            DateTime? expirationDate = null);

        Task<IEnumerable<SecureDocument>> GetUserAccessibleDocumentsAsync(Guid userId);
    }

    public class DocumentSecurityService : IDocumentSecurityService
    {
        private readonly ILogger<DocumentSecurityService> _logger;
        private readonly IGenericRepository<SecureDocument> _documentRepository;
        private readonly IGenericRepository<DocumentAccessPermission> _accessPermissionRepository;
        private readonly IGenericRepository<DocumentAccessLog> _accessLogRepository;
        private readonly IGenericRepository<Patient> _patientRepository;

        public DocumentSecurityService(
            ILogger<DocumentSecurityService> logger,
            IGenericRepository<SecureDocument> documentRepository,
            IGenericRepository<DocumentAccessPermission> accessPermissionRepository,
            IGenericRepository<DocumentAccessLog> accessLogRepository,
            IGenericRepository<Patient> patientRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            _accessPermissionRepository = accessPermissionRepository ?? throw new ArgumentNullException(nameof(accessPermissionRepository));
            _accessLogRepository = accessLogRepository ?? throw new ArgumentNullException(nameof(accessLogRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
        }

        public async Task<SecureDocument> CreateSecureDocumentAsync(
            Guid ownerId, 
            string documentName, 
            string documentType, 
            string content, 
            DocumentAccessLevel accessLevel)
        {
            // Validate owner
            var owner = await _patientRepository.GetByIdAsync(ownerId);
            if (owner == null)
                throw new ArgumentException("Invalid owner", nameof(ownerId));

            // Create secure document
            var secureDocument = new SecureDocument
            {
                DocumentId = Guid.NewGuid(),
                DocumentName = documentName,
                DocumentType = documentType,
                OwnerId = ownerId,
                AccessLevel = accessLevel
            };

            // Encrypt content
            secureDocument.EncryptContent(content);

            // Add owner's full access permission
            secureDocument.AccessPermissions.Add(new DocumentAccessPermission
            {
                UserId = ownerId,
                AccessLevel = DocumentAccessLevel.FullAccess
            });

            await _documentRepository.AddAsync(secureDocument);
            return secureDocument;
        }

        public async Task<string> AccessDocumentAsync(
            Guid documentId, 
            Guid userId, 
            string ipAddress)
        {
            // Find document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException("Document not found");

            // Check access permission
            var accessPermission = document.AccessPermissions
                .FirstOrDefault(p => p.UserId == userId);

            if (accessPermission == null || 
                accessPermission.AccessLevel < DocumentAccessLevel.Read ||
                (accessPermission.ExpirationDate.HasValue && 
                 accessPermission.ExpirationDate < DateTime.UtcNow))
            {
                _logger.LogWarning($"Unauthorized access attempt for document {documentId}");
                throw new UnauthorizedAccessException("Access denied");
            }

            // Log access
            await LogDocumentAccessAsync(documentId, userId, DocumentAccessType.View, ipAddress);

            // Update last accessed time
            document.LastAccessedAt = DateTime.UtcNow;
            await _documentRepository.UpdateAsync(document);

            // Decrypt and return content
            return document.DecryptContent();
        }

        public async Task<bool> UpdateDocumentAccessPermissionAsync(
            Guid documentId, 
            Guid userId, 
            DocumentAccessLevel accessLevel, 
            DateTime? expirationDate = null)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException("Document not found");

            var existingPermission = document.AccessPermissions
                .FirstOrDefault(p => p.UserId == userId);

            if (existingPermission == null)
            {
                existingPermission = new DocumentAccessPermission
                {
                    SecureDocumentId = documentId,
                    UserId = userId,
                    AccessLevel = accessLevel,
                    ExpirationDate = expirationDate
                };
                document.AccessPermissions.Add(existingPermission);
            }
            else
            {
                existingPermission.AccessLevel = accessLevel;
                existingPermission.ExpirationDate = expirationDate;
            }

            await _accessPermissionRepository.AddOrUpdateAsync(existingPermission);
            return true;
        }

        public async Task<IEnumerable<SecureDocument>> GetUserAccessibleDocumentsAsync(Guid userId)
        {
            return await _documentRepository.FindAsync(
                doc => doc.AccessPermissions.Any(p => 
                    p.UserId == userId && 
                    p.AccessLevel >= DocumentAccessLevel.Read &&
                    (p.ExpirationDate == null || p.ExpirationDate >= DateTime.UtcNow)
                )
            );
        }

        private async Task LogDocumentAccessAsync(
            Guid documentId, 
            Guid userId, 
            DocumentAccessType accessType, 
            string ipAddress)
        {
            var accessLog = new DocumentAccessLog
            {
                SecureDocumentId = documentId,
                UserId = userId,
                AccessType = accessType,
                IPAddress = ipAddress
            };

            await _accessLogRepository.AddAsync(accessLog);
        }
    }
}
