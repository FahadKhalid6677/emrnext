using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Services.Clinical
{
    public class ClinicalDocumentValidationService : IDocumentValidator
    {
        private readonly IRepository<DocumentTemplate> _templateRepository;
        private readonly IRepository<ClinicalDocument> _documentRepository;
        private readonly ILogger<ClinicalDocumentValidationService> _logger;

        public ClinicalDocumentValidationService(
            IRepository<DocumentTemplate> templateRepository,
            IRepository<ClinicalDocument> documentRepository,
            ILogger<ClinicalDocumentValidationService> logger)
        {
            _templateRepository = templateRepository;
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public async Task<DocumentValidationResult> ValidateDocumentRequestAsync(DocumentRequest request)
        {
            var validationResult = new DocumentValidationResult();

            // Validate Patient ID
            if (string.IsNullOrWhiteSpace(request.PatientId))
            {
                validationResult.AddError("PatientId", "Patient ID is required");
            }

            // Validate Template
            var template = await ValidateDocumentTemplate(request.TemplateId);
            if (template == null)
            {
                validationResult.AddError("TemplateId", "Invalid or inactive document template");
            }

            // Validate Title
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                validationResult.AddError("Title", "Document title is required");
            }
            else if (request.Title.Length > 255)
            {
                validationResult.AddError("Title", "Document title cannot exceed 255 characters");
            }

            // Validate Content
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                validationResult.AddError("Content", "Document content is required");
            }
            else
            {
                var contentValidationResult = ValidateDocumentContent(request.Content, template);
                if (!contentValidationResult.IsValid)
                {
                    validationResult.Errors.AddRange(contentValidationResult.Errors);
                }
            }

            return validationResult;
        }

        public async Task<DocumentValidationResult> ValidateDocumentUpdateAsync(
            DocumentRequest request, 
            ClinicalDocument existingDocument)
        {
            var validationResult = await ValidateDocumentRequestAsync(request);

            // Additional update-specific validations
            if (existingDocument.Status == DocumentStatus.Signed)
            {
                validationResult.AddError("Status", "Cannot modify a signed document");
            }

            // Check for significant content changes
            var contentSimilarity = CalculateContentSimilarity(
                existingDocument.CurrentVersion?.Content, 
                request.Content
            );

            if (contentSimilarity < 0.1) // Less than 10% similar
            {
                validationResult.AddWarning(
                    "Content", 
                    "Significant content changes detected. Ensure this is intentional."
                );
            }

            return validationResult;
        }

        public async Task<DocumentValidationResult> ValidateSignatureRequestAsync(
            SignatureRequest request, 
            ClinicalDocument document)
        {
            var validationResult = new DocumentValidationResult();

            // Validate document status
            if (document.Status == DocumentStatus.Signed)
            {
                validationResult.AddError("Status", "Document is already fully signed");
            }

            // Validate signature type
            if (!Enum.IsDefined(typeof(SignatureType), request.SignatureType))
            {
                validationResult.AddError("SignatureType", "Invalid signature type");
            }

            // Check for duplicate signatures
            if (document.Signatures?.Any(s => s.SignedBy == request.SignedBy) == true)
            {
                validationResult.AddError("SignedBy", "User has already signed this document");
            }

            return validationResult;
        }

        private async Task<DocumentTemplate> ValidateDocumentTemplate(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return null;
            }

            var template = await _templateRepository.GetByIdAsync(templateId);
            return template?.IsActive == true ? template : null;
        }

        private DocumentValidationResult ValidateDocumentContent(
            string content, 
            DocumentTemplate template)
        {
            var validationResult = new DocumentValidationResult();

            // Basic content length validation
            if (content.Length < 50)
            {
                validationResult.AddError("Content", "Document content is too short");
            }

            if (content.Length > 10000)
            {
                validationResult.AddError("Content", "Document content exceeds maximum length");
            }

            // Template-specific content validation
            if (template != null)
            {
                var templateValidationResult = ValidateContentAgainstTemplate(content, template);
                if (!templateValidationResult.IsValid)
                {
                    validationResult.Errors.AddRange(templateValidationResult.Errors);
                }
            }

            // Check for potentially sensitive information
            var sensitiveInfoResult = DetectSensitiveInformation(content);
            if (!sensitiveInfoResult.IsValid)
            {
                validationResult.Warnings.AddRange(sensitiveInfoResult.Warnings);
            }

            return validationResult;
        }

        private DocumentValidationResult ValidateContentAgainstTemplate(
            string content, 
            DocumentTemplate template)
        {
            var validationResult = new DocumentValidationResult();

            // Example: Validate required sections based on template
            var requiredSections = ExtractRequiredSections(template.TemplateContent);
            foreach (var section in requiredSections)
            {
                if (!content.Contains(section, StringComparison.OrdinalIgnoreCase))
                {
                    validationResult.AddError(
                        "Content", 
                        $"Missing required section: {section}"
                    );
                }
            }

            return validationResult;
        }

        private List<string> ExtractRequiredSections(string templateContent)
        {
            // This is a simplified implementation
            // In a real-world scenario, you'd have a more sophisticated 
            // method of extracting required sections
            return new List<string>
            {
                "Patient History",
                "Current Symptoms",
                "Physical Examination",
                "Assessment",
                "Plan"
            };
        }

        private DocumentValidationResult DetectSensitiveInformation(string content)
        {
            var validationResult = new DocumentValidationResult();

            // Regex patterns for detecting potentially sensitive information
            var sensitivePatterns = new[]
            {
                @"\b\d{3}-\d{2}-\d{4}\b", // SSN
                @"\b\d{16}\b", // Credit Card
                @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b" // Email
            };

            foreach (var pattern in sensitivePatterns)
            {
                var matches = Regex.Matches(content, pattern);
                if (matches.Count > 0)
                {
                    validationResult.AddWarning(
                        "SensitiveInfo", 
                        $"Potential sensitive information detected matching pattern: {pattern}"
                    );
                }
            }

            return validationResult;
        }

        private double CalculateContentSimilarity(string oldContent, string newContent)
        {
            if (string.IsNullOrEmpty(oldContent) || string.IsNullOrEmpty(newContent))
                return 1.0;

            var oldWords = oldContent.Split(' ');
            var newWords = newContent.Split(' ');

            var commonWords = oldWords.Intersect(newWords).Count();
            var totalWords = oldWords.Length + newWords.Length;

            return 2.0 * commonWords / totalWords;
        }
    }

    public class DocumentValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<ValidationError> Errors { get; } = new List<ValidationError>();
        public List<ValidationWarning> Warnings { get; } = new List<ValidationWarning>();

        public void AddError(string field, string message)
        {
            Errors.Add(new ValidationError(field, message));
        }

        public void AddWarning(string field, string message)
        {
            Warnings.Add(new ValidationWarning(field, message));
        }
    }

    public class ValidationError
    {
        public string Field { get; }
        public string Message { get; }

        public ValidationError(string field, string message)
        {
            Field = field;
            Message = message;
        }
    }

    public class ValidationWarning
    {
        public string Field { get; }
        public string Message { get; }

        public ValidationWarning(string field, string message)
        {
            Field = field;
            Message = message;
        }
    }

    public interface IDocumentValidator
    {
        Task<DocumentValidationResult> ValidateDocumentRequestAsync(DocumentRequest request);
        Task<DocumentValidationResult> ValidateDocumentUpdateAsync(
            DocumentRequest request, 
            ClinicalDocument existingDocument
        );
        Task<DocumentValidationResult> ValidateSignatureRequestAsync(
            SignatureRequest request, 
            ClinicalDocument document
        );
    }
}
