using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Clinical
{
    public interface IClinicalDocumentationService
    {
        Task<ClinicalDocument> CreateDocumentAsync(DocumentRequest request);
        Task<ClinicalDocument> GetDocumentAsync(string id);
        Task<IEnumerable<ClinicalDocument>> GetPatientDocumentsAsync(string patientId);
        Task<ClinicalDocument> UpdateDocumentAsync(string id, DocumentRequest request);
        Task<bool> DeleteDocumentAsync(string id);
        Task<IEnumerable<DocumentTemplate>> GetTemplatesAsync(string specialtyId);
    }
}
