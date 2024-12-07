using System;
using System.Collections.Generic;

namespace EMRNext.Core.Models
{
    public class Referral
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid ReferringProviderId { get; set; }
        public Guid? ReceivingProviderId { get; set; }
        public string ReferralType { get; set; }
        public string Specialty { get; set; }
        public string Reason { get; set; }
        public string ClinicalNotes { get; set; }
        public ReferralStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class CareTransition
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid FromProviderId { get; set; }
        public Guid ToProviderId { get; set; }
        public string TransitionType { get; set; }
        public string Summary { get; set; }
        public List<string> KeyCommunicationPoints { get; set; }
        public List<string> MedicationChanges { get; set; }
        public CareTransitionStatus Status { get; set; }
        public DateTime TransitionDate { get; set; }
    }

    public class CareTeam
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string TeamName { get; set; }
        public List<CareTeamMember> Members { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CareTeamMember
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Specialty { get; set; }
        public CareTeamMemberStatus Status { get; set; }
    }

    public class CommunicationLog
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string MessageContent { get; set; }
        public CommunicationType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }

    public enum ReferralStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public enum CareTransitionStatus
    {
        Initiated,
        InProgress,
        Completed,
        Discontinued
    }

    public enum CareTeamMemberStatus
    {
        Active,
        Inactive,
        OnLeave
    }

    public enum CommunicationType
    {
        Secure_Message,
        Consultation_Request,
        Care_Plan_Update,
        Referral_Communication
    }
}
