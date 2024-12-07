using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Consent
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PolicyReference { get; set; }
        public string Scope { get; set; }
        public string Category { get; set; }

        // Consent Details
        public string ConsentText { get; set; }
        public string DataElements { get; set; }
        public string Purpose { get; set; }
        public string Exceptions { get; set; }
        public string Restrictions { get; set; }

        // Verification
        public string VerificationType { get; set; }
        public string VerificationMethod { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public string VerifiedBy { get; set; }
        public string WitnessName { get; set; }
        public string WitnessSignature { get; set; }
        public DateTime? WitnessDate { get; set; }

        // Document References
        public string DocumentReference { get; set; }
        public string SignedDocumentPath { get; set; }
        public bool HasPhysicalCopy { get; set; }
        public string PhysicalLocation { get; set; }

        // Legal Information
        public string LegalAuthority { get; set; }
        public string LegalStatus { get; set; }
        public string JurisdictionCode { get; set; }
        public string LegalRestrictions { get; set; }

        // Related Parties
        public string AuthorizedParty { get; set; }
        public string AuthorizedPartyRelationship { get; set; }
        public string ConsentingParty { get; set; }
        public string ConsentingPartyRelationship { get; set; }

        // Revocation
        public bool IsRevocable { get; set; }
        public string RevocationMethod { get; set; }
        public DateTime? RevokedDate { get; set; }
        public string RevokedBy { get; set; }
        public string RevocationReason { get; set; }

        // Notifications
        public bool RequiresNotification { get; set; }
        public string NotificationRecipients { get; set; }
        public string NotificationTriggers { get; set; }
        public DateTime? LastNotificationDate { get; set; }

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
