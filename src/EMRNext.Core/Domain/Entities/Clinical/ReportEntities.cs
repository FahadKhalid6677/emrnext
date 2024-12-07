using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities.Clinical
{
    public class ReportTemplateEntity : BaseEntity
    {
        public string Name { get; set; }
        public string Type { get; set; } // Lab, Imaging, Combined
        public string Format { get; set; } // PDF, HTML, HL7
        public string TemplateContent { get; set; }
        public string HeaderTemplate { get; set; }
        public string FooterTemplate { get; set; }
        public string Stylesheet { get; set; }
        public string ScriptContent { get; set; }
        public bool IsActive { get; set; }
        public string Version { get; set; }
        public DateTime EffectiveDate { get; set; }
        public virtual ICollection<ReportMappingEntity> Mappings { get; set; }
    }

    public class ReportMappingEntity : BaseEntity
    {
        public int TemplateId { get; set; }
        public virtual ReportTemplateEntity Template { get; set; }
        public string SourceField { get; set; }
        public string TargetField { get; set; }
        public string TransformationType { get; set; }
        public string TransformationRule { get; set; }
        public int DisplayOrder { get; set; }
        public string FormattingRule { get; set; }
        public string ValidationRule { get; set; }
    }

    public class ClinicalReportEntity : BaseEntity
    {
        public string ReportType { get; set; }
        public int PatientId { get; set; }
        public int? OrderId { get; set; }
        public string Status { get; set; } // Draft, Final, Amended, Corrected
        public DateTime GeneratedDateTime { get; set; }
        public int GeneratedById { get; set; }
        public string DocumentPath { get; set; }
        public string Format { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public string SignatureStatus { get; set; }
        public DateTime? SignedDateTime { get; set; }
        public int? SignedById { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedDateTime { get; set; }
        public virtual ICollection<ReportDistributionEntity> Distributions { get; set; }
        public virtual ICollection<ReportAuditEntity> AuditTrail { get; set; }
    }

    public class ReportDistributionEntity : BaseEntity
    {
        public int ReportId { get; set; }
        public virtual ClinicalReportEntity Report { get; set; }
        public string DistributionType { get; set; } // Portal, Email, Fax, Print
        public string RecipientType { get; set; } // Provider, Patient, External
        public string RecipientId { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? DeliveredDateTime { get; set; }
        public string DeliveryDetails { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? LastAttemptDateTime { get; set; }
        public string ErrorDetails { get; set; }
    }

    public class ReportAuditEntity : BaseEntity
    {
        public int ReportId { get; set; }
        public virtual ClinicalReportEntity Report { get; set; }
        public string Action { get; set; }
        public DateTime ActionDateTime { get; set; }
        public int ActionById { get; set; }
        public string ActionDetails { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }

    public class ReportAccessControlEntity : BaseEntity
    {
        public int ReportId { get; set; }
        public virtual ClinicalReportEntity Report { get; set; }
        public string AccessType { get; set; } // Role, User, Department
        public string AccessId { get; set; }
        public string Permissions { get; set; } // View, Print, Download, Share
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveUntil { get; set; }
        public string GrantedBy { get; set; }
        public string Reason { get; set; }
    }

    public class ReportAnnotationEntity : BaseEntity
    {
        public int ReportId { get; set; }
        public virtual ClinicalReportEntity Report { get; set; }
        public int AuthorId { get; set; }
        public DateTime AnnotationDateTime { get; set; }
        public string AnnotationType { get; set; }
        public string Content { get; set; }
        public string Location { get; set; }
        public bool IsPrivate { get; set; }
        public virtual ICollection<ReportAnnotationReplyEntity> Replies { get; set; }
    }

    public class ReportAnnotationReplyEntity : BaseEntity
    {
        public int AnnotationId { get; set; }
        public virtual ReportAnnotationEntity Annotation { get; set; }
        public int AuthorId { get; set; }
        public DateTime ReplyDateTime { get; set; }
        public string Content { get; set; }
    }

    public class ReportReleaseRuleEntity : BaseEntity
    {
        public string ReportType { get; set; }
        public string Condition { get; set; }
        public string RequiredSignoffs { get; set; }
        public bool RequiresCriticalReview { get; set; }
        public string AutoReleaseCondition { get; set; }
        public string BlockingCondition { get; set; }
        public string NotificationRules { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
    }
}
