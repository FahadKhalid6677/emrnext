using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Prescription
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? EncounterId { get; set; }
        public int? PharmacyId { get; set; }

        // Medication Details
        public string DrugName { get; set; }
        public string DrugCode { get; set; } // NDC, RxNorm
        public string DrugForm { get; set; } // tablet, capsule, liquid, etc.
        public decimal Strength { get; set; }
        public string StrengthUnit { get; set; }
        public string Route { get; set; } // oral, topical, etc.
        
        // Dosage Instructions
        public decimal Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public string Directions { get; set; } // SIG
        public string DosingInstructions { get; set; }
        public decimal? DoseAmount { get; set; }
        public string DoseUnit { get; set; }
        public string Frequency { get; set; }
        public string TimingInstructions { get; set; }
        public string AdditionalInstructions { get; set; }
        public bool AsNeeded { get; set; }
        public string AsNeededReason { get; set; }

        // Prescription Details
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DaysSupply { get; set; }
        public int NumberOfRefills { get; set; }
        public int RefillsRemaining { get; set; }
        public DateTime? LastRefillDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Status { get; set; } // Active, Completed, Discontinued
        public string PrescriptionType { get; set; } // New, Refill, Transfer
        public bool IsPrn { get; set; }
        public bool IsControlled { get; set; }
        public string ControlledSubstanceSchedule { get; set; }

        // Dispensing Instructions
        public bool SubstitutionAllowed { get; set; }
        public string SubstitutionReason { get; set; }
        public string PharmacyNotes { get; set; }
        public bool IsDispensedInHouse { get; set; }

        // Clinical Details
        public string Indication { get; set; }
        public string ClinicalNotes { get; set; }
        public string PatientInstructions { get; set; }
        public string Warnings { get; set; }

        // E-Prescribing
        public bool IsERx { get; set; }
        public string ERxTransactionId { get; set; }
        public DateTime? ERxSubmissionDate { get; set; }
        public string ERxStatus { get; set; }
        public string ERxErrorMessage { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Encounter Encounter { get; set; }
        public virtual Pharmacy Pharmacy { get; set; }
    }
}
