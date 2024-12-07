using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Entities.Clinical
{
    public class AllergyEntity : BaseEntity
    {
        public int PatientId { get; set; }
        public string Type { get; set; } // Drug, Food, Environmental
        public string Allergen { get; set; }
        public string Reaction { get; set; }
        public string Severity { get; set; } // Mild, Moderate, Severe
        public DateTime OnsetDate { get; set; }
        public bool IsActive { get; set; }
        public string Source { get; set; } // Patient, Provider, External
        public string VerificationStatus { get; set; } // Unconfirmed, Confirmed, Refuted
        public int? VerifiedByProviderId { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string ClinicalNotes { get; set; }
        public virtual ICollection<AllergyInteractionEntity> Interactions { get; set; }
    }

    public class DrugClassEntity : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TherapeuticCategory { get; set; }
        public bool RequiresMonitoring { get; set; }
        public string CrossReactivityGroup { get; set; }
        public virtual ICollection<DrugEntity> Drugs { get; set; }
        public virtual ICollection<DrugClassInteractionEntity> ClassInteractions { get; set; }
    }

    public class DrugEntity : BaseEntity
    {
        public string GenericName { get; set; }
        public string BrandNames { get; set; }
        public int DrugClassId { get; set; }
        public virtual DrugClassEntity DrugClass { get; set; }
        public string Strength { get; set; }
        public string Form { get; set; }
        public string Route { get; set; }
        public bool IsControlled { get; set; }
        public string MonitoringRequirements { get; set; }
        public virtual ICollection<DrugInteractionEntity> DrugInteractions { get; set; }
        public virtual ICollection<AllergyInteractionEntity> AllergyInteractions { get; set; }
    }

    public class DrugInteractionEntity : BaseEntity
    {
        public int Drug1Id { get; set; }
        public int Drug2Id { get; set; }
        public virtual DrugEntity Drug1 { get; set; }
        public virtual DrugEntity Drug2 { get; set; }
        public string SeverityLevel { get; set; }
        public string InteractionMechanism { get; set; }
        public string ClinicalEffects { get; set; }
        public string ManagementStrategy { get; set; }
        public string EvidenceLevel { get; set; }
        public string References { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class DrugClassInteractionEntity : BaseEntity
    {
        public int Class1Id { get; set; }
        public int Class2Id { get; set; }
        public virtual DrugClassEntity Class1 { get; set; }
        public virtual DrugClassEntity Class2 { get; set; }
        public string SeverityLevel { get; set; }
        public string InteractionMechanism { get; set; }
        public string ClinicalEffects { get; set; }
        public string ManagementStrategy { get; set; }
        public string EvidenceLevel { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class AllergyInteractionEntity : BaseEntity
    {
        public int AllergyId { get; set; }
        public int DrugId { get; set; }
        public virtual AllergyEntity Allergy { get; set; }
        public virtual DrugEntity Drug { get; set; }
        public string CrossReactivityRisk { get; set; }
        public string ClinicalEvidence { get; set; }
        public string RecommendedAction { get; set; }
        public bool RequiresOverride { get; set; }
        public string OverrideRequirements { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class AllergyOverrideEntity : BaseEntity
    {
        public int AllergyId { get; set; }
        public int DrugId { get; set; }
        public int ProviderId { get; set; }
        public int PatientId { get; set; }
        public DateTime OverrideDate { get; set; }
        public string Reason { get; set; }
        public string AlternativeConsidered { get; set; }
        public string RiskMitigationPlan { get; set; }
        public string PatientConsentNotes { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public virtual AllergyEntity Allergy { get; set; }
        public virtual DrugEntity Drug { get; set; }
    }
}
