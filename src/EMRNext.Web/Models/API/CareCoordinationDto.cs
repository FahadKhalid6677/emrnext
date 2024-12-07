using System;
using System.Collections.Generic;

namespace EMRNext.Web.Models.API
{
    public class ReferralDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid ReferringProviderId { get; set; }
        public Guid? ReceivingProviderId { get; set; }
        public string ReferralType { get; set; }
        public string Specialty { get; set; }
        public string Reason { get; set; }
        public string ClinicalNotes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class CareTransitionDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid FromProviderId { get; set; }
        public Guid ToProviderId { get; set; }
        public string TransitionType { get; set; }
        public string Summary { get; set; }
        public List<string> KeyCommunicationPoints { get; set; }
        public List<string> MedicationChanges { get; set; }
        public string Status { get; set; }
        public DateTime TransitionDate { get; set; }
    }

    public class CareTeamDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string TeamName { get; set; }
        public List<CareTeamMemberDto> Members { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CareTeamMemberDto
    {
        public Guid Id { get; set; }
        public Guid ProviderId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Specialty { get; set; }
        public string Status { get; set; }
    }

    public class CommunicationLogDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string MessageContent { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }
}
