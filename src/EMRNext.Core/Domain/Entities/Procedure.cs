using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Procedure
    {
        public int Id { get; set; }
        public int EncounterId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? FacilityId { get; set; }

        // Procedure Details
        public string Code { get; set; } // CPT/HCPCS code
        public string CodeSystem { get; set; } // CPT, HCPCS
        public string Description { get; set; }
        public DateTime ServiceDate { get; set; }
        public string Status { get; set; } // Scheduled, In Progress, Completed, Cancelled
        public string Category { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; } // Routine, Urgent, Emergency
        public string Location { get; set; } // Body site/location
        public string Laterality { get; set; } // Left, Right, Bilateral
        
        // Clinical Details
        public string ClinicalIndication { get; set; }
        public string Technique { get; set; }
        public string Findings { get; set; }
        public string Complications { get; set; }
        public string Anesthesia { get; set; }
        public string Specimens { get; set; }
        public string PostProcedureInstructions { get; set; }

        // Authorization
        public string AuthorizationNumber { get; set; }
        public DateTime? AuthorizationDate { get; set; }
        public string AuthorizationStatus { get; set; }
        public int? AuthorizedVisits { get; set; }
        public DateTime? AuthorizationExpirationDate { get; set; }

        // Billing
        public bool IsBillable { get; set; }
        public decimal? UnitPrice { get; set; }
        public int Units { get; set; }
        public string Modifiers { get; set; }
        public bool IsOutsideLab { get; set; }
        public decimal? OutsideLabCharges { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Encounter Encounter { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Facility Facility { get; set; }
    }
}
