using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities
{
    public class Encounter
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public int? SupervisorId { get; set; }
        public int FacilityId { get; set; }
        public int? BillingFacilityId { get; set; }

        // Encounter Details
        public DateTime Date { get; set; }
        public string ClassCode { get; set; } // AMB (ambulatory), EMER (emergency), etc.
        public string Type { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string ChiefComplaint { get; set; }
        public string Sensitivity { get; set; } // Privacy/security level
        public string ReferralSource { get; set; }
        public string BillingNote { get; set; }
        public int? AppointmentCategoryId { get; set; }
        
        // Clinical Documentation
        public string SubjectiveNotes { get; set; }
        public string ObjectiveNotes { get; set; }
        public string AssessmentNotes { get; set; }
        public string PlanNotes { get; set; }
        public string AdditionalNotes { get; set; }

        // Discharge
        public string DischargeDisposition { get; set; }
        public DateTime? DischargeDate { get; set; }
        public string DischargeNotes { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Provider Supervisor { get; set; }
        public virtual Facility Facility { get; set; }
        public virtual Facility BillingFacility { get; set; }
        public virtual ICollection<Diagnosis> Diagnoses { get; set; }
        public virtual ICollection<Procedure> Procedures { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
        public virtual ICollection<OrderRequest> Orders { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Vital> Vitals { get; set; }
        public virtual ICollection<BillingClaim> BillingClaims { get; set; }
    }
}
