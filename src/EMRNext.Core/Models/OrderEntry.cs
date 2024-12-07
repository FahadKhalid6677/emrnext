using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models
{
    public class ClinicalOrder
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public string OrderedBy { get; set; }
        public DateTime OrderedAt { get; set; }
        public OrderType Type { get; set; }
        public OrderPriority Priority { get; set; }
        public OrderStatus Status { get; set; }
        public string DiagnosisId { get; set; }
        public string Notes { get; set; }

        // Polymorphic order details
        public object OrderDetails { get; set; }

        // Tracking and compliance
        public List<OrderAction> ActionHistory { get; set; }
        public OrderComplianceMetrics ComplianceMetrics { get; set; }
    }

    // Specific Order Types
    public class MedicationOrder : ClinicalOrder
    {
        public string MedicationId { get; set; }
        public decimal Dosage { get; set; }
        public DosageUnit DosageUnit { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public List<MedicationInteraction> PotentialInteractions { get; set; }
    }

    public class DiagnosticTestOrder : ClinicalOrder
    {
        public string TestCode { get; set; }
        public DiagnosticTestType TestType { get; set; }
        public string SpecimenType { get; set; }
        public bool FastingRequired { get; set; }
        public DateTime? ScheduledFor { get; set; }
    }

    public class ProcedureOrder : ClinicalOrder
    {
        public string ProcedureCode { get; set; }
        public ProcedureType ProcedureType { get; set; }
        public string BodySite { get; set; }
        public bool RequiresConsent { get; set; }
        public List<PreProcedureInstruction> PreInstructions { get; set; }
        public List<PostProcedureInstruction> PostInstructions { get; set; }
    }

    public class ReferralOrder : ClinicalOrder
    {
        public string SpecialtyId { get; set; }
        public string ReferredToProviderId { get; set; }
        public ReferralUrgency Urgency { get; set; }
        public string ReferralReason { get; set; }
        public List<string> AttachedDocumentIds { get; set; }
    }

    // Supporting Structures
    public class OrderAction
    {
        public Guid Id { get; set; }
        public OrderActionType Type { get; set; }
        public string PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
        public string Notes { get; set; }
    }

    public class OrderComplianceMetrics
    {
        public bool IsTimelySigned { get; set; }
        public bool IsCompletedOnTime { get; set; }
        public TimeSpan? CompletionDelay { get; set; }
        public decimal ComplianceScore { get; set; }
    }

    public class MedicationInteraction
    {
        public string MedicationId { get; set; }
        public InteractionSeverity Severity { get; set; }
        public string InteractionDescription { get; set; }
    }

    public class PreProcedureInstruction
    {
        public string Description { get; set; }
        public bool IsMandatory { get; set; }
    }

    public class PostProcedureInstruction
    {
        public string Description { get; set; }
        public RecoveryPrecaution Precaution { get; set; }
    }

    // Enumerations
    public enum OrderType
    {
        Medication,
        DiagnosticTest,
        Procedure,
        Referral,
        LabTest,
        Imaging,
        Consultation
    }

    public enum OrderPriority
    {
        Routine,
        Urgent,
        Stat,
        Scheduled,
        PreOp
    }

    public enum OrderStatus
    {
        Pending,
        Approved,
        InProgress,
        Completed,
        Cancelled,
        OnHold,
        Rejected
    }

    public enum DosageUnit
    {
        Milligrams,
        Milliliters,
        Tablets,
        Capsules,
        Drops,
        Units
    }

    public enum DurationUnit
    {
        Days,
        Weeks,
        Months
    }

    public enum DiagnosticTestType
    {
        BloodTest,
        Urinalysis,
        Imaging,
        Biopsy,
        Genetic,
        Microbiological
    }

    public enum ProcedureType
    {
        Surgical,
        Diagnostic,
        Therapeutic,
        Preventive
    }

    public enum ReferralUrgency
    {
        Routine,
        Urgent,
        Emergency
    }

    public enum OrderActionType
    {
        Created,
        Updated,
        Approved,
        Rejected,
        Completed,
        Cancelled
    }

    public enum InteractionSeverity
    {
        Low,
        Moderate,
        High,
        Contraindicated
    }

    public enum RecoveryPrecaution
    {
        None,
        BedRest,
        LimitedActivity,
        SpecialCare
    }
}
