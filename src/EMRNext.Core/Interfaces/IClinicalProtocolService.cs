using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface IClinicalProtocolService
    {
        Task<ClinicalProtocol> GetProtocolAsync(int protocolId);
        Task<IEnumerable<ClinicalProtocol>> GetActiveProtocolsAsync();
        Task<IEnumerable<ClinicalProtocol>> GetProtocolsBySpecialtyAsync(string specialty);
        Task<ClinicalProtocol> CreateProtocolAsync(ClinicalProtocol protocol);
        Task UpdateProtocolAsync(ClinicalProtocol protocol);
        Task DeleteProtocolAsync(int protocolId);
        Task<IEnumerable<ProtocolStep>> GetProtocolStepsAsync(int protocolId);
        Task<bool> ValidateProtocolComplianceAsync(int protocolId, int patientId);
        Task<ProtocolAssessment> AssessProtocolEligibilityAsync(int protocolId, int patientId);
        Task<IEnumerable<ProtocolAlert>> GetProtocolAlertsAsync(int protocolId, int patientId);
    }

    public class ClinicalProtocol
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Specialty { get; set; }
        public string TargetCondition { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string EvidenceLevel { get; set; }
        public string References { get; set; }
        public ICollection<ProtocolStep> Steps { get; set; }
    }

    public class ProtocolStep
    {
        public int Id { get; set; }
        public int ProtocolId { get; set; }
        public int Sequence { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Criteria { get; set; }
        public string Actions { get; set; }
        public string ExpectedOutcome { get; set; }
        public int? TimeframeInDays { get; set; }
        public bool IsRequired { get; set; }
        public string AlertLevel { get; set; }
    }

    public class ProtocolAssessment
    {
        public int ProtocolId { get; set; }
        public int PatientId { get; set; }
        public bool IsEligible { get; set; }
        public string AssessmentNotes { get; set; }
        public ICollection<string> ExclusionReasons { get; set; }
        public ICollection<string> Contraindications { get; set; }
        public DateTime AssessmentDate { get; set; }
    }

    public class ProtocolAlert
    {
        public int Id { get; set; }
        public int ProtocolId { get; set; }
        public int PatientId { get; set; }
        public string AlertType { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string Resolution { get; set; }
    }
}
