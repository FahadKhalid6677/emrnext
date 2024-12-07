using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Laboratory
{
    public class LabOrder : AuditableEntity
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }
        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
        public int? EncounterId { get; set; }
        public virtual Encounter Encounter { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? CollectionDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string ClinicalNotes { get; set; }
        public string DiagnosisCodes { get; set; }
        public bool IsFasting { get; set; }
        public string SpecimenType { get; set; }
        public string CollectionSite { get; set; }
        public string CollectionMethod { get; set; }
        public int? ExternalLabId { get; set; }
        public virtual ExternalLab ExternalLab { get; set; }
        public string ExternalOrderId { get; set; }
        public bool RequiresApproval { get; set; }
        public string ApprovalStatus { get; set; }
        public int? ApproverId { get; set; }
        public virtual Provider Approver { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string InsuranceAuthorizationNumber { get; set; }
        public virtual ICollection<LabOrderItem> OrderItems { get; set; }
        public virtual ICollection<LabResult> Results { get; set; }
        public virtual ICollection<LabOrderDocument> Documents { get; set; }
        public virtual ICollection<LabOrderAlert> Alerts { get; set; }
    }

    public class LabOrderItem : AuditableEntity
    {
        public int Id { get; set; }
        public int LabOrderId { get; set; }
        public virtual LabOrder LabOrder { get; set; }
        public int LabTestId { get; set; }
        public virtual LabTest LabTest { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string SpecialInstructions { get; set; }
        public DateTime? ExpectedResultDate { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
        public string ResultStatus { get; set; }
        public virtual ICollection<LabResult> Results { get; set; }
    }

    public class LabTest : AuditableEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string SpecimenType { get; set; }
        public string MethodologyType { get; set; }
        public string Department { get; set; }
        public int ProcessingTime { get; set; }
        public bool RequiresFasting { get; set; }
        public string SpecialInstructions { get; set; }
        public decimal Cost { get; set; }
        public string CPTCode { get; set; }
        public string LOINCCode { get; set; }
        public string ReferenceRangeLogic { get; set; }
        public string CriticalRangeLogic { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<LabTestComponent> Components { get; set; }
        public virtual ICollection<LabOrderItem> OrderItems { get; set; }
    }

    public class LabTestComponent : AuditableEntity
    {
        public int Id { get; set; }
        public int LabTestId { get; set; }
        public virtual LabTest LabTest { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public string DataType { get; set; }
        public string ReferenceRange { get; set; }
        public string CriticalRange { get; set; }
        public string LOINCCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool IsCalculated { get; set; }
        public string CalculationFormula { get; set; }
        public virtual ICollection<LabResultValue> ResultValues { get; set; }
    }

    public class LabResult : AuditableEntity
    {
        public int Id { get; set; }
        public int LabOrderId { get; set; }
        public virtual LabOrder LabOrder { get; set; }
        public int LabOrderItemId { get; set; }
        public virtual LabOrderItem LabOrderItem { get; set; }
        public DateTime ResultDate { get; set; }
        public string Status { get; set; }
        public string PerformingLab { get; set; }
        public string PerformingTechnician { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string Comments { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
        public bool RequiresRepeat { get; set; }
        public string RepeatReason { get; set; }
        public virtual ICollection<LabResultValue> ResultValues { get; set; }
        public virtual ICollection<LabResultDocument> Documents { get; set; }
        public virtual ICollection<LabResultAlert> Alerts { get; set; }
    }

    public class LabResultValue : AuditableEntity
    {
        public int Id { get; set; }
        public int LabResultId { get; set; }
        public virtual LabResult LabResult { get; set; }
        public int LabTestComponentId { get; set; }
        public virtual LabTestComponent LabTestComponent { get; set; }
        public string Value { get; set; }
        public string Units { get; set; }
        public string ReferenceRange { get; set; }
        public string Flag { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
        public string Comments { get; set; }
        public string PreviousValue { get; set; }
        public DateTime? PreviousValueDate { get; set; }
        public string TrendIndicator { get; set; }
    }

    public class ExternalLab : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string InterfaceType { get; set; }
        public string ConnectionDetails { get; set; }
        public string Credentials { get; set; }
        public bool IsActive { get; set; }
        public string SupportContact { get; set; }
        public virtual ICollection<LabOrder> Orders { get; set; }
    }

    public class LabOrderDocument : AuditableEntity
    {
        public int Id { get; set; }
        public int LabOrderId { get; set; }
        public virtual LabOrder LabOrder { get; set; }
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
    }

    public class LabResultDocument : AuditableEntity
    {
        public int Id { get; set; }
        public int LabResultId { get; set; }
        public virtual LabResult LabResult { get; set; }
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
    }

    public class LabOrderAlert : AuditableEntity
    {
        public int Id { get; set; }
        public int LabOrderId { get; set; }
        public virtual LabOrder LabOrder { get; set; }
        public string AlertType { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public string AcknowledgedBy { get; set; }
    }

    public class LabResultAlert : AuditableEntity
    {
        public int Id { get; set; }
        public int LabResultId { get; set; }
        public virtual LabResult LabResult { get; set; }
        public string AlertType { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public string AcknowledgedBy { get; set; }
    }
}
