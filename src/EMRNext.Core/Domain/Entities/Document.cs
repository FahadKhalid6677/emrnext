using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public int PatientId { get; set; }
        public int? EncounterId { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string StoragePath { get; set; }
        public string Status { get; set; }
        public bool IsConfidential { get; set; }
        public string Source { get; set; }
        public string Author { get; set; }
        public DateTime DocumentDate { get; set; }
        public string Facility { get; set; }

        // Document Metadata
        public string Department { get; set; }
        public string Specialty { get; set; }
        public string ServiceType { get; set; }
        public string Keywords { get; set; }
        public string Language { get; set; }
        public string Version { get; set; }

        // Document Security
        public string SecurityLevel { get; set; }
        public string AccessRestrictions { get; set; }
        public string EncryptionType { get; set; }
        public bool RequiresSignature { get; set; }
        public DateTime? SignedDate { get; set; }
        public string SignedBy { get; set; }

        // Document Processing
        public bool IsScanned { get; set; }
        public bool IsIndexed { get; set; }
        public bool IsOCRProcessed { get; set; }
        public string OCRStatus { get; set; }
        public string OCRConfidence { get; set; }
        public string OCRText { get; set; }

        // Document Workflow
        public string WorkflowStatus { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; }
        public string ReviewStatus { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string ReviewNotes { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Encounter Encounter { get; set; }
    }
}
