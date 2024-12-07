using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Clinical;
using EMRNext.Core.Models.Laboratory;

namespace EMRNext.Core.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentEntity> UploadDocumentAsync(DocumentRequest request);
        Task<DocumentEntity> GetDocumentAsync(int documentId);
        Task<byte[]> GetDocumentContentAsync(int documentId);
        Task<bool> DeleteDocumentAsync(int documentId);
        Task<bool> ArchiveDocumentAsync(int documentId);
        Task<bool> UpdateMetadataAsync(int documentId, Dictionary<string, string> metadata);
        Task<IEnumerable<DocumentEntity>> GetPatientDocumentsAsync(int patientId);
        Task<IEnumerable<DocumentEntity>> GetEncounterDocumentsAsync(int encounterId);
        Task<IEnumerable<DocumentEntity>> GetOrderDocumentsAsync(int orderId);
        Task<bool> ValidateDocumentAsync(DocumentRequest request);
        Task<string> GenerateDocumentUrlAsync(int documentId, TimeSpan? expiry = null);
        Task<bool> UpdateSecurityLevelAsync(int documentId, string securityLevel);
        Task<bool> GrantAccessAsync(int documentId, int userId, string permission);
        Task<bool> RevokeAccessAsync(int documentId, int userId);
    }
}
