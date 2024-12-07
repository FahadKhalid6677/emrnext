using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Services.Clinical
{
    public class ClinicalDocumentSearchService
    {
        private readonly IRepository<ClinicalDocument> _documentRepository;
        private readonly IUserContext _userContext;
        private readonly ILogger<ClinicalDocumentSearchService> _logger;

        public ClinicalDocumentSearchService(
            IRepository<ClinicalDocument> documentRepository,
            IUserContext userContext,
            ILogger<ClinicalDocumentSearchService> logger)
        {
            _documentRepository = documentRepository;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<DocumentSearchResult> SearchDocumentsAsync(DocumentSearchCriteria criteria)
        {
            try
            {
                _logger.LogInformation("Performing advanced document search");

                // Validate user permissions
                if (!await _userContext.HasPermissionAsync(Permission.SearchClinicalDocuments))
                {
                    throw new UnauthorizedAccessException("User does not have permission to search clinical documents");
                }

                // Start with base query
                var query = await _documentRepository.GetAllAsync();

                // Apply filters
                query = ApplyPatientFilter(query, criteria.PatientId);
                query = ApplyDateRangeFilter(query, criteria.StartDate, criteria.EndDate);
                query = ApplyDocumentTypeFilter(query, criteria.DocumentTypes);
                query = ApplyStatusFilter(query, criteria.Statuses);
                query = ApplyCreatedByFilter(query, criteria.CreatedByUserId);
                query = ApplyTemplateFilter(query, criteria.TemplateIds);

                // Apply full-text search
                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    query = ApplyFullTextSearch(query, criteria.SearchText);
                }

                // Sort results
                query = ApplySorting(query, criteria.SortBy, criteria.SortDescending);

                // Pagination
                var totalCount = query.Count();
                var pagedResults = query
                    .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .ToList();

                // Fetch latest versions for each document
                await PopulateLatestVersions(pagedResults);

                return new DocumentSearchResult
                {
                    Documents = pagedResults,
                    TotalCount = totalCount,
                    PageNumber = criteria.PageNumber,
                    PageSize = criteria.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing document search");
                throw;
            }
        }

        private IQueryable<ClinicalDocument> ApplyPatientFilter(
            IQueryable<ClinicalDocument> query, 
            string patientId)
        {
            return string.IsNullOrWhiteSpace(patientId) 
                ? query 
                : query.Where(d => d.PatientId == patientId);
        }

        private IQueryable<ClinicalDocument> ApplyDateRangeFilter(
            IQueryable<ClinicalDocument> query, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            if (startDate.HasValue)
                query = query.Where(d => d.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(d => d.CreatedAt <= endDate.Value);

            return query;
        }

        private IQueryable<ClinicalDocument> ApplyDocumentTypeFilter(
            IQueryable<ClinicalDocument> query, 
            List<DocumentType> documentTypes)
        {
            return documentTypes?.Any() == true 
                ? query.Where(d => documentTypes.Contains(d.Type)) 
                : query;
        }

        private IQueryable<ClinicalDocument> ApplyStatusFilter(
            IQueryable<ClinicalDocument> query, 
            List<DocumentStatus> statuses)
        {
            return statuses?.Any() == true 
                ? query.Where(d => statuses.Contains(d.Status)) 
                : query;
        }

        private IQueryable<ClinicalDocument> ApplyCreatedByFilter(
            IQueryable<ClinicalDocument> query, 
            string createdByUserId)
        {
            return string.IsNullOrWhiteSpace(createdByUserId) 
                ? query 
                : query.Where(d => d.CreatedBy == createdByUserId);
        }

        private IQueryable<ClinicalDocument> ApplyTemplateFilter(
            IQueryable<ClinicalDocument> query, 
            List<string> templateIds)
        {
            return templateIds?.Any() == true 
                ? query.Where(d => templateIds.Contains(d.TemplateId)) 
                : query;
        }

        private IQueryable<ClinicalDocument> ApplyFullTextSearch(
            IQueryable<ClinicalDocument> query, 
            string searchText)
        {
            // This is a basic implementation. In a production system, 
            // you would use a more sophisticated full-text search solution
            return query.Where(d => 
                d.Title.Contains(searchText) ||
                d.CurrentVersion.Content.Contains(searchText)
            );
        }

        private IQueryable<ClinicalDocument> ApplySorting(
            IQueryable<ClinicalDocument> query, 
            DocumentSortField sortBy, 
            bool sortDescending)
        {
            return sortBy switch
            {
                DocumentSortField.CreatedDate => sortDescending 
                    ? query.OrderByDescending(d => d.CreatedAt)
                    : query.OrderBy(d => d.CreatedAt),
                DocumentSortField.Title => sortDescending 
                    ? query.OrderByDescending(d => d.Title)
                    : query.OrderBy(d => d.Title),
                DocumentSortField.Status => sortDescending 
                    ? query.OrderByDescending(d => d.Status)
                    : query.OrderBy(d => d.Status),
                _ => query
            };
        }

        private async Task PopulateLatestVersions(List<ClinicalDocument> documents)
        {
            // Fetch latest versions for all documents in a single query
            var documentIds = documents.Select(d => d.Id).ToList();
            var latestVersions = await _documentRepository.GetLatestVersionsForDocuments(documentIds);

            foreach (var document in documents)
            {
                document.CurrentVersion = latestVersions.FirstOrDefault(v => v.DocumentId == document.Id);
            }
        }

        public async Task<DocumentComplianceReport> GenerateComplianceReportAsync(DocumentComplianceCriteria criteria)
        {
            try
            {
                _logger.LogInformation("Generating document compliance report");

                // Validate user permissions for compliance reporting
                if (!await _userContext.HasPermissionAsync(Permission.ViewComplianceReports))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view compliance reports");
                }

                var documents = await _documentRepository.GetAllAsync();

                // Apply compliance filters
                documents = documents.Where(d => 
                    d.CreatedAt >= criteria.StartDate && 
                    d.CreatedAt <= criteria.EndDate
                );

                var complianceReport = new DocumentComplianceReport
                {
                    StartDate = criteria.StartDate,
                    EndDate = criteria.EndDate,
                    TotalDocuments = documents.Count(),
                    CompletedDocuments = documents.Count(d => d.Status == DocumentStatus.Signed),
                    InProgressDocuments = documents.Count(d => d.Status == DocumentStatus.InReview),
                    DocumentTypeBreakdown = documents
                        .GroupBy(d => d.Type)
                        .Select(g => new DocumentTypeCompliance
                        {
                            DocumentType = g.Key,
                            TotalCount = g.Count(),
                            CompletedCount = g.Count(d => d.Status == DocumentStatus.Signed)
                        })
                        .ToList(),
                    SignatureComplianceRate = CalculateSignatureComplianceRate(documents)
                };

                return complianceReport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating document compliance report");
                throw;
            }
        }

        private double CalculateSignatureComplianceRate(IEnumerable<ClinicalDocument> documents)
        {
            var totalDocuments = documents.Count();
            var fullySignedDocuments = documents.Count(d => 
                d.Status == DocumentStatus.Signed && 
                d.Signatures?.Count >= 2 // Assuming at least 2 signatures are required
            );

            return totalDocuments > 0 
                ? (double)fullySignedDocuments / totalDocuments * 100 
                : 0;
        }
    }

    // Supporting models for advanced search and compliance reporting
    public class DocumentSearchCriteria
    {
        public string PatientId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<DocumentType> DocumentTypes { get; set; }
        public List<DocumentStatus> Statuses { get; set; }
        public string CreatedByUserId { get; set; }
        public List<string> TemplateIds { get; set; }
        public string SearchText { get; set; }
        public DocumentSortField SortBy { get; set; } = DocumentSortField.CreatedDate;
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class DocumentSearchResult
    {
        public List<ClinicalDocument> Documents { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class DocumentComplianceCriteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DocumentComplianceReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDocuments { get; set; }
        public int CompletedDocuments { get; set; }
        public int InProgressDocuments { get; set; }
        public List<DocumentTypeCompliance> DocumentTypeBreakdown { get; set; }
        public double SignatureComplianceRate { get; set; }
    }

    public class DocumentTypeCompliance
    {
        public DocumentType DocumentType { get; set; }
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
    }

    public enum DocumentSortField
    {
        CreatedDate,
        Title,
        Status
    }
}
