using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using EMRNext.Core.Security;
using EMRNext.Core.Validation;
using EMRNext.Core.Exceptions;

namespace EMRNext.Core.Services.Clinical
{
    public class ClinicalDocumentationService : IClinicalDocumentationService
    {
        private readonly IRepository<ClinicalDocument> _documentRepository;
        private readonly IRepository<DocumentTemplate> _templateRepository;
        private readonly IRepository<DocumentVersion> _versionRepository;
        private readonly IUserContext _userContext;
        private readonly ILogger<ClinicalDocumentationService> _logger;
        private readonly IDocumentValidator _validator;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly ClinicalDocumentSearchService _searchService;
        private readonly ClinicalDocumentValidationService _validationService;

        public ClinicalDocumentationService(
            IRepository<ClinicalDocument> documentRepository,
            IRepository<DocumentTemplate> templateRepository,
            IRepository<DocumentVersion> versionRepository,
            IUserContext userContext,
            ILogger<ClinicalDocumentationService> logger,
            IDocumentValidator validator,
            IAuditService auditService,
            INotificationService notificationService,
            ClinicalDocumentSearchService searchService,
            ClinicalDocumentValidationService validationService)
        {
            _documentRepository = documentRepository;
            _templateRepository = templateRepository;
            _versionRepository = versionRepository;
            _userContext = userContext;
            _logger = logger;
            _validator = validator;
            _auditService = auditService;
            _notificationService = notificationService;
            _searchService = searchService;
            _validationService = validationService;
        }

        public async Task<ClinicalDocument> CreateDocumentAsync(DocumentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new clinical document for patient {PatientId}", request.PatientId);

                // Validate request and user permissions
                if (!await _userContext.HasPermissionAsync(Permission.CreateClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to create clinical documents");
                }

                var validationResult = await _validator.ValidateDocumentRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                // Create document
                var document = new ClinicalDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = request.PatientId,
                    TemplateId = request.TemplateId,
                    Title = request.Title,
                    Status = DocumentStatus.Draft,
                    CreatedBy = _userContext.CurrentUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = _userContext.CurrentUserId,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create initial version
                var version = new DocumentVersion
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = document.Id,
                    Content = request.Content,
                    Version = 1,
                    CreatedBy = _userContext.CurrentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                // Save document and version
                await _documentRepository.AddAsync(document);
                await _versionRepository.AddAsync(version);

                // Create audit trail
                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    document.Id,
                    AuditAction.Create,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Clinical document {DocumentId} created successfully", document.Id);

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating clinical document for patient {PatientId}", request.PatientId);
                throw;
            }
        }

        public async Task<ClinicalDocument> GetDocumentAsync(string id)
        {
            try
            {
                _logger.LogInformation("Retrieving clinical document {DocumentId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.ViewClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view clinical documents");
                }

                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null)
                {
                    throw new NotFoundException($"Clinical document {id} not found");
                }

                // Check if user has access to this specific document
                if (!await _userContext.CanAccessPatientDataAsync(document.PatientId))
                {
                    throw new UnauthorizedAccessException("User does not have access to this patient's documents");
                }

                // Get latest version
                var version = await _versionRepository.FindAsync(v => v.DocumentId == id)
                    .ContinueWith(t => t.Result.OrderByDescending(v => v.Version).FirstOrDefault());

                document.CurrentVersion = version;

                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    id,
                    AuditAction.View,
                    _userContext.CurrentUserId);

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clinical document {DocumentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ClinicalDocument>> GetPatientDocumentsAsync(string patientId)
        {
            try
            {
                _logger.LogInformation("Retrieving clinical documents for patient {PatientId}", patientId);

                if (!await _userContext.HasPermissionAsync(Permission.ViewClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view clinical documents");
                }

                if (!await _userContext.CanAccessPatientDataAsync(patientId))
                {
                    throw new UnauthorizedAccessException("User does not have access to this patient's documents");
                }

                var documents = await _documentRepository.FindAsync(d => d.PatientId == patientId);

                // Get latest versions for all documents
                foreach (var document in documents)
                {
                    var version = await _versionRepository.FindAsync(v => v.DocumentId == document.Id)
                        .ContinueWith(t => t.Result.OrderByDescending(v => v.Version).FirstOrDefault());
                    document.CurrentVersion = version;
                }

                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    patientId,
                    AuditAction.List,
                    _userContext.CurrentUserId);

                return documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving clinical documents for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<ClinicalDocument> UpdateDocumentAsync(string id, DocumentRequest request)
        {
            try
            {
                _logger.LogInformation("Updating clinical document {DocumentId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.UpdateClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to update clinical documents");
                }

                var existingDocument = await _documentRepository.GetByIdAsync(id);
                if (existingDocument == null)
                {
                    throw new NotFoundException($"Clinical document {id} not found");
                }

                // Validate update permissions
                if (!await _userContext.CanAccessPatientDataAsync(existingDocument.PatientId))
                {
                    throw new UnauthorizedAccessException("User does not have access to update this document");
                }

                var validationResult = await _validator.ValidateDocumentUpdateAsync(request, existingDocument);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                // Determine version change type
                var latestVersion = await _versionRepository.FindAsync(v => v.DocumentId == id)
                    .ContinueWith(t => t.Result.OrderByDescending(v => v.Version).FirstOrDefault());

                var changeType = DetermineVersionChangeType(latestVersion?.Content, request.Content);

                // Create new document version
                var newVersion = new DocumentVersion
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = id,
                    Content = request.Content,
                    Version = latestVersion?.Version + 1 ?? 1,
                    CreatedBy = _userContext.CurrentUserId,
                    CreatedAt = DateTime.UtcNow,
                    ChangeType = changeType,
                    ChangeDescription = request.ChangeDescription
                };

                // Update document metadata
                existingDocument.Status = request.Status ?? existingDocument.Status;
                existingDocument.UpdatedBy = _userContext.CurrentUserId;
                existingDocument.UpdatedAt = DateTime.UtcNow;

                // Save updates
                await _versionRepository.AddAsync(newVersion);
                await _documentRepository.UpdateAsync(existingDocument);

                // Create audit trail
                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    id,
                    AuditAction.Update,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Clinical document {DocumentId} updated successfully", id);

                existingDocument.CurrentVersion = newVersion;
                return existingDocument;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating clinical document {DocumentId}", id);
                throw;
            }
        }

        private VersionChangeType DetermineVersionChangeType(string oldContent, string newContent)
        {
            if (string.IsNullOrEmpty(oldContent))
                return VersionChangeType.Initial;

            var contentSimilarity = CalculateContentSimilarity(oldContent, newContent);

            return contentSimilarity switch
            {
                > 0.9 => VersionChangeType.Minor,
                > 0.5 => VersionChangeType.Major,
                _ => VersionChangeType.Correction
            };
        }

        private double CalculateContentSimilarity(string oldContent, string newContent)
        {
            // Implement a basic similarity calculation
            // This is a placeholder and should be replaced with a more sophisticated algorithm
            var oldWords = oldContent.Split(' ');
            var newWords = newContent.Split(' ');

            var commonWords = oldWords.Intersect(newWords).Count();
            var totalWords = oldWords.Length + newWords.Length;

            return 2.0 * commonWords / totalWords;
        }

        public async Task<DocumentSignature> SignDocumentAsync(string documentId, SignatureRequest request)
        {
            try
            {
                _logger.LogInformation("Signing clinical document {DocumentId}", documentId);

                if (!await _userContext.HasPermissionAsync(Permission.SignClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to sign clinical documents");
                }

                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                {
                    throw new NotFoundException($"Clinical document {documentId} not found");
                }

                // Validate signature request
                var validationResult = await _validator.ValidateSignatureRequestAsync(request, document);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var signature = new DocumentSignature
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = documentId,
                    SignedBy = _userContext.CurrentUserId,
                    SignedAt = DateTime.UtcNow,
                    Type = request.SignatureType,
                    Comments = request.Comments
                };

                // Add signature to document
                document.Signatures ??= new List<DocumentSignature>();
                document.Signatures.Add(signature);

                // Update document status if all required signatures are complete
                if (IsDocumentFullySigned(document))
                {
                    document.Status = DocumentStatus.Signed;
                }

                await _documentRepository.UpdateAsync(document);

                // Create audit trail
                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    documentId,
                    AuditAction.Sign,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Clinical document {DocumentId} signed successfully", documentId);

                return signature;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing clinical document {DocumentId}", documentId);
                throw;
            }
        }

        private bool IsDocumentFullySigned(ClinicalDocument document)
        {
            // Implement logic to determine if all required signatures are present
            // This would depend on your specific organizational requirements
            return document.Signatures?.Count >= 2; // Example: requires at least 2 signatures
        }

        public async Task<DocumentAmendment> AmendDocumentAsync(string documentId, AmendmentRequest request)
        {
            try
            {
                _logger.LogInformation("Amending clinical document {DocumentId}", documentId);

                if (!await _userContext.HasPermissionAsync(Permission.AmendClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to amend clinical documents");
                }

                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                {
                    throw new NotFoundException($"Clinical document {documentId} not found");
                }

                var latestVersion = await _versionRepository.FindAsync(v => v.DocumentId == documentId)
                    .ContinueWith(t => t.Result.OrderByDescending(v => v.Version).FirstOrDefault());

                var amendment = new DocumentAmendment
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = documentId,
                    AmendedBy = _userContext.CurrentUserId,
                    AmendedAt = DateTime.UtcNow,
                    OriginalContent = latestVersion?.Content,
                    AmendedContent = request.AmendedContent,
                    Reason = request.Reason
                };

                // Add amendment to document
                document.Amendments ??= new List<DocumentAmendment>();
                document.Amendments.Add(amendment);
                document.Status = DocumentStatus.Amended;

                await _documentRepository.UpdateAsync(document);

                // Create new document version for the amendment
                var newVersion = new DocumentVersion
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = documentId,
                    Content = request.AmendedContent,
                    Version = latestVersion?.Version + 1 ?? 1,
                    CreatedBy = _userContext.CurrentUserId,
                    CreatedAt = DateTime.UtcNow,
                    ChangeType = VersionChangeType.Correction,
                    ChangeDescription = $"Amendment: {request.Reason}"
                };

                await _versionRepository.AddAsync(newVersion);

                // Create audit trail
                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    documentId,
                    AuditAction.Amend,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Clinical document {DocumentId} amended successfully", documentId);

                return amendment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error amending clinical document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<DocumentCollaboration> InviteCollaboratorAsync(string documentId, CollaborationRequest request)
        {
            try
            {
                _logger.LogInformation("Inviting collaborator to document {DocumentId}", documentId);

                if (!await _userContext.HasPermissionAsync(Permission.CollaborateClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to invite collaborators");
                }

                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                {
                    throw new NotFoundException($"Clinical document {documentId} not found");
                }

                var collaboration = new DocumentCollaboration
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = documentId,
                    InvitedUserId = request.CollaboratorUserId,
                    Status = CollaborationStatus.Invited,
                    InvitedAt = DateTime.UtcNow
                };

                // Add collaboration to document
                document.Collaborations ??= new List<DocumentCollaboration>();
                document.Collaborations.Add(collaboration);

                await _documentRepository.UpdateAsync(document);

                // Send collaboration invitation notification
                await _notificationService.SendCollaborationInvitationAsync(collaboration);

                // Create audit trail
                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    documentId,
                    AuditAction.Collaborate,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Collaborator invited to document {DocumentId} successfully", documentId);

                return collaboration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting collaborator to document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting clinical document {DocumentId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.DeleteClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to delete clinical documents");
                }

                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null)
                {
                    throw new NotFoundException($"Clinical document {id} not found");
                }

                if (!await _userContext.CanAccessPatientDataAsync(document.PatientId))
                {
                    throw new UnauthorizedAccessException("User does not have access to this patient's documents");
                }

                // Soft delete document
                document.IsDeleted = true;
                document.DeletedBy = _userContext.CurrentUserId;
                document.DeletedAt = DateTime.UtcNow;

                await _documentRepository.UpdateAsync(document);

                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    id,
                    AuditAction.Delete,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Clinical document {DocumentId} deleted successfully", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting clinical document {DocumentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentTemplate>> GetTemplatesAsync(string specialtyId)
        {
            try
            {
                _logger.LogInformation("Retrieving document templates for specialty {SpecialtyId}", specialtyId);

                if (!await _userContext.HasPermissionAsync(Permission.ViewDocumentTemplates))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view document templates");
                }

                var templates = await _templateRepository.FindAsync(t => t.SpecialtyId == specialtyId && !t.IsDeleted);

                await _auditService.CreateAuditAsync(
                    EntityType.DocumentTemplate,
                    specialtyId,
                    AuditAction.List,
                    _userContext.CurrentUserId);

                return templates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document templates for specialty {SpecialtyId}", specialtyId);
                throw;
            }
        }

        public async Task<DocumentSearchResult> SearchDocumentsAsync(DocumentSearchCriteria criteria)
        {
            return await _searchService.SearchDocumentsAsync(criteria);
        }

        public async Task<DocumentComplianceReport> GenerateComplianceReportAsync(DocumentComplianceCriteria criteria)
        {
            return await _searchService.GenerateComplianceReportAsync(criteria);
        }

        public async Task<DocumentValidationResult> ValidateDocumentAsync(DocumentRequest request)
        {
            return await _validationService.ValidateDocumentRequestAsync(request);
        }

        public async Task<DocumentValidationResult> ValidateDocumentUpdateAsync(
            DocumentRequest request, 
            ClinicalDocument existingDocument)
        {
            return await _validationService.ValidateDocumentUpdateAsync(request, existingDocument);
        }

        public async Task<DocumentValidationResult> ValidateSignatureRequestAsync(
            SignatureRequest request, 
            ClinicalDocument document)
        {
            return await _validationService.ValidateSignatureRequestAsync(request, document);
        }

        public async Task<string> ExportDocumentAsync(string documentId, DocumentExportFormat format)
        {
            try
            {
                _logger.LogInformation("Exporting document {DocumentId} in {Format} format", documentId, format);

                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                {
                    throw new NotFoundException($"Clinical document {documentId} not found");
                }

                // Validate export permissions
                if (!await _userContext.HasPermissionAsync(Permission.ExportClinicalDocument))
                {
                    throw new UnauthorizedAccessException("User does not have permission to export clinical documents");
                }

                string exportedContent = format switch
                {
                    DocumentExportFormat.PDF => await ExportToPdfAsync(document),
                    DocumentExportFormat.HTML => await ExportToHtmlAsync(document),
                    DocumentExportFormat.DOCX => await ExportToDocxAsync(document),
                    _ => throw new NotSupportedException($"Export format {format} is not supported")
                };

                // Create audit trail for document export
                await _auditService.CreateAuditAsync(
                    EntityType.ClinicalDocument,
                    documentId,
                    AuditAction.Export,
                    _userContext.CurrentUserId
                );

                return exportedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting document {DocumentId}", documentId);
                throw;
            }
        }

        private async Task<string> ExportToPdfAsync(ClinicalDocument document)
        {
            // Implement PDF export logic
            // This would typically involve using a PDF generation library
            throw new NotImplementedException("PDF export is not yet implemented");
        }

        private async Task<string> ExportToHtmlAsync(ClinicalDocument document)
        {
            // Generate HTML representation of the document
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>{document.Title}</title>
                </head>
                <body>
                    <h1>{document.Title}</h1>
                    <p>Created: {document.CreatedAt}</p>
                    <div>{document.CurrentVersion?.Content}</div>
                </body>
                </html>
            ";
        }

        private async Task<string> ExportToDocxAsync(ClinicalDocument document)
        {
            // Implement DOCX export logic
            // This would typically involve using a DOCX generation library
            throw new NotImplementedException("DOCX export is not yet implemented");
        }
    }

    public enum DocumentExportFormat
    {
        PDF,
        HTML,
        DOCX
    }
}
