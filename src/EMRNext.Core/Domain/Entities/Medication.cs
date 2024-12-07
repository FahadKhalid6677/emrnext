using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Medication
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Name { get; set; }
        public string GenericName { get; set; }
        public string Manufacturer { get; set; }
        public string NDCCode { get; set; }
        public string RxNorm { get; set; }
        
        // Prescription Details
        public string Strength { get; set; }
        public string Form { get; set; }
        public string Route { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Instructions { get; set; }
        public int Quantity { get; set; }
        public int Refills { get; set; }
        public bool AsNeeded { get; set; }
        public string AsNeededReason { get; set; }
        
        // Timing
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastFilledDate { get; set; }
        public DateTime? NextRefillDate { get; set; }
        
        // Status
        public string Status { get; set; }
        public string DiscontinuationReason { get; set; }
        public DateTime? DiscontinuedDate { get; set; }
        public string DiscontinuedBy { get; set; }
        
        // Prescriber Information
        public int ProviderId { get; set; }
        public string PrescriberNPI { get; set; }
        public string PrescriberDEA { get; set; }
        
        // Pharmacy Information
        public string PharmacyName { get; set; }
        public string PharmacyAddress { get; set; }
        public string PharmacyPhone { get; set; }
        public string PharmacyNCPDPID { get; set; }
        
        // Clinical Information
        public string Indication { get; set; }
        public string DiagnosisCode { get; set; }
        public bool IsHighRisk { get; set; }
        public string RiskCategory { get; set; }
        public string Warnings { get; set; }
        
        // Patient Response
        public string PatientResponse { get; set; }
        public string AdverseEffects { get; set; }
        public string Effectiveness { get; set; }
        public string ComplianceStatus { get; set; }
        
        // Insurance Information
        public string InsuranceStatus { get; set; }
        public string PriorAuthStatus { get; set; }
        public DateTime? PriorAuthExpirationDate { get; set; }
        public string CoverageNotes { get; set; }
        
        // Monitoring
        public string MonitoringRequired { get; set; }
        public string MonitoringParameters { get; set; }
        public DateTime? NextMonitoringDate { get; set; }
        public string MonitoringNotes { get; set; }
        
        // Drug Interactions
        public bool HasInteractions { get; set; }
        public string InteractionSeverity { get; set; }
        public string InteractionDetails { get; set; }
        public string InteractionMitigation { get; set; }
        
        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
