using System;

namespace EMRNext.Core.Domain.Entities
{
    public class AdvanceDirective
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public bool IsActive { get; set; }

        // Directive Details
        public string Description { get; set; }
        public string Scope { get; set; }
        public string Instructions { get; set; }
        public string Limitations { get; set; }
        public string Preferences { get; set; }

        // Healthcare Agent/Proxy
        public string ProxyName { get; set; }
        public string ProxyRelationship { get; set; }
        public string ProxyPhone { get; set; }
        public string ProxyEmail { get; set; }
        public string ProxyAddress { get; set; }
        public bool ProxyHasFullAuthority { get; set; }
        public string ProxyRestrictions { get; set; }

        // Alternate Proxy
        public string AlternateProxyName { get; set; }
        public string AlternateProxyRelationship { get; set; }
        public string AlternateProxyContact { get; set; }

        // Documentation
        public string DocumentReference { get; set; }
        public bool HasPhysicalCopy { get; set; }
        public string PhysicalLocation { get; set; }
        public string StorageNotes { get; set; }

        // Verification
        public string VerificationType { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string VerifiedBy { get; set; }
        public string WitnessName1 { get; set; }
        public string WitnessSignature1 { get; set; }
        public string WitnessName2 { get; set; }
        public string WitnessSignature2 { get; set; }
        public string NotaryName { get; set; }
        public string NotarySignature { get; set; }
        public DateTime? NotaryDate { get; set; }
        public string NotaryCommissionExpiry { get; set; }

        // Life Support Preferences
        public bool? WantsCPR { get; set; }
        public bool? WantsIntubation { get; set; }
        public bool? WantsArtificialNutrition { get; set; }
        public bool? WantsArtificialFluids { get; set; }
        public string LifeSupportNotes { get; set; }

        // Organ Donation
        public bool IsOrganDonor { get; set; }
        public string OrganDonationPreferences { get; set; }
        public string OrganDonationRestrictions { get; set; }

        // Distribution and Access
        public string CopiesProvidedTo { get; set; }
        public string AccessRestrictions { get; set; }
        public DateTime? LastDistributionDate { get; set; }
        public string DistributionNotes { get; set; }

        // Review and Updates
        public string ReviewFrequency { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public string ReviewNotes { get; set; }
        public string LastReviewedBy { get; set; }

        // Legal Information
        public string LegalAuthority { get; set; }
        public string JurisdictionCode { get; set; }
        public string LegalRestrictions { get; set; }
        public bool IsCourtOrdered { get; set; }
        public string CourtOrderReference { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
    }
}
