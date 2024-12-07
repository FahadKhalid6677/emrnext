using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Pharmacy
{
    public class Prescription : AuditableEntity
    {
        public int Id { get; set; }
        public string PrescriptionNumber { get; set; }
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }
        public int ProviderId { get; set; }
        public virtual Provider Provider { get; set; }
        public int? EncounterId { get; set; }
        public virtual Encounter Encounter { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string DiagnosisCodes { get; set; }
        public string ClinicalNotes { get; set; }
        public bool IsControlled { get; set; }
        public bool RequiresApproval { get; set; }
        public string ApprovalStatus { get; set; }
        public int? ApproverId { get; set; }
        public virtual Provider Approver { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int? PharmacyId { get; set; }
        public virtual Pharmacy Pharmacy { get; set; }
        public string ExternalRxId { get; set; }
        public virtual ICollection<PrescriptionItem> Items { get; set; }
        public virtual ICollection<PrescriptionAlert> Alerts { get; set; }
        public virtual ICollection<PrescriptionDocument> Documents { get; set; }
    }

    public class PrescriptionItem : AuditableEntity
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public virtual Prescription Prescription { get; set; }
        public int MedicationId { get; set; }
        public virtual Medication Medication { get; set; }
        public string Dosage { get; set; }
        public string Route { get; set; }
        public string Frequency { get; set; }
        public string Instructions { get; set; }
        public int Quantity { get; set; }
        public int DaysSupply { get; set; }
        public int Refills { get; set; }
        public int RefillsRemaining { get; set; }
        public DateTime? LastFilledDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPrn { get; set; }
        public string PrnInstructions { get; set; }
        public bool RequiresCompounding { get; set; }
        public string CompoundingInstructions { get; set; }
        public bool HasInteractions { get; set; }
        public virtual ICollection<DrugInteraction> Interactions { get; set; }
        public virtual ICollection<PrescriptionFill> Fills { get; set; }
    }

    public class Medication : AuditableEntity
    {
        public int Id { get; set; }
        public string NDCCode { get; set; }
        public string Name { get; set; }
        public string GenericName { get; set; }
        public string Manufacturer { get; set; }
        public string DrugClass { get; set; }
        public string Form { get; set; }
        public string Strength { get; set; }
        public string Unit { get; set; }
        public bool IsControlled { get; set; }
        public string ControlledSubstanceClass { get; set; }
        public bool RequiresCompounding { get; set; }
        public bool IsActive { get; set; }
        public decimal AWP { get; set; }
        public virtual ICollection<DrugInteraction> Interactions { get; set; }
        public virtual ICollection<MedicationAlert> Alerts { get; set; }
    }

    public class DrugInteraction : AuditableEntity
    {
        public int Id { get; set; }
        public int MedicationId { get; set; }
        public virtual Medication Medication { get; set; }
        public int InteractingMedicationId { get; set; }
        public virtual Medication InteractingMedication { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string ClinicalEffects { get; set; }
        public string Mechanism { get; set; }
        public string Management { get; set; }
        public string References { get; set; }
    }

    public class PrescriptionFill : AuditableEntity
    {
        public int Id { get; set; }
        public int PrescriptionItemId { get; set; }
        public virtual PrescriptionItem PrescriptionItem { get; set; }
        public DateTime FillDate { get; set; }
        public int Quantity { get; set; }
        public int DaysSupply { get; set; }
        public string Status { get; set; }
        public int? PharmacyId { get; set; }
        public virtual Pharmacy Pharmacy { get; set; }
        public string PharmacistNotes { get; set; }
        public string DispensingInstructions { get; set; }
        public virtual ICollection<PrescriptionFillDocument> Documents { get; set; }
    }

    public class Pharmacy : AuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NPI { get; set; }
        public string NCPDPID { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string InterfaceType { get; set; }
        public string ConnectionDetails { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
        public virtual ICollection<PrescriptionFill> Fills { get; set; }
    }

    public class PrescriptionAlert : AuditableEntity
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public virtual Prescription Prescription { get; set; }
        public string AlertType { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public string AcknowledgedBy { get; set; }
    }

    public class MedicationAlert : AuditableEntity
    {
        public int Id { get; set; }
        public int MedicationId { get; set; }
        public virtual Medication Medication { get; set; }
        public string AlertType { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class PrescriptionDocument : AuditableEntity
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public virtual Prescription Prescription { get; set; }
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
    }

    public class PrescriptionFillDocument : AuditableEntity
    {
        public int Id { get; set; }
        public int PrescriptionFillId { get; set; }
        public virtual PrescriptionFill PrescriptionFill { get; set; }
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
    }
}
