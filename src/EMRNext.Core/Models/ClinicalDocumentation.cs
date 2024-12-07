using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models
{
    public class ClinicalDocument
    {
        public string Id { get; set; }
        public string PatientId { get; set; }
        public string TemplateId { get; set; }
        public string Title { get; set; }
        public DocumentType Type { get; set; }
        public DocumentStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DocumentVersion CurrentVersion { get; set; }
        public List<DocumentSignature> Signatures { get; set; }
        public List<DocumentAmendment> Amendments { get; set; }
        public List<DocumentCollaboration> Collaborations { get; set; }
    }

    public class DocumentVersion
    {
        public string Id { get; set; }
        public string DocumentId { get; set; }
        public int Version { get; set; }
        public string Content { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public VersionChangeType ChangeType { get; set; }
        public string ChangeDescription { get; set; }
    }

    public class DocumentTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DocumentType Type { get; set; }
        public string TemplateContent { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class DocumentSignature
    {
        public string Id { get; set; }
        public string DocumentId { get; set; }
        public string SignedBy { get; set; }
        public DateTime SignedAt { get; set; }
        public SignatureType Type { get; set; }
        public string Comments { get; set; }
    }

    public class DocumentAmendment
    {
        public string Id { get; set; }
        public string DocumentId { get; set; }
        public string AmendedBy { get; set; }
        public DateTime AmendedAt { get; set; }
        public string OriginalContent { get; set; }
        public string AmendedContent { get; set; }
        public string Reason { get; set; }
    }

    public class DocumentCollaboration
    {
        public string Id { get; set; }
        public string DocumentId { get; set; }
        public string InvitedUserId { get; set; }
        public CollaborationStatus Status { get; set; }
        public DateTime InvitedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }

    public enum DocumentType
    {
        ProgressNote,
        ConsultationNote,
        DischargeNote,
        OperationNote,
        ReferralNote,
        TransferNote
    }

    public enum DocumentStatus
    {
        Draft,
        InReview,
        Completed,
        Signed,
        Amended,
        Archived
    }

    public enum SignatureType
    {
        Author,
        Cosigner,
        Supervisor,
        Consultant
    }

    public enum VersionChangeType
    {
        Initial,
        Minor,
        Major,
        Correction
    }

    public enum CollaborationStatus
    {
        Invited,
        Accepted,
        Declined,
        Completed
    }
}
