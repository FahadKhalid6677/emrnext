using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Base;
using EMRNext.Core.Domain.Enums;

namespace EMRNext.Core.Domain.Entities
{
    /// <summary>
    /// Represents a comprehensive compliance audit record with advanced tracking capabilities
    /// </summary>
    public class ComplianceAudit : BaseEntity
    {
        /// <summary>
        /// Unique identifier for the compliance audit
        /// </summary>
        public Guid AuditId { get; set; }

        /// <summary>
        /// Type of compliance audit
        /// </summary>
        public ComplianceAuditType AuditType { get; set; }

        /// <summary>
        /// Specific regulatory standard or framework
        /// </summary>
        public string RegulatoryStandard { get; set; }

        /// <summary>
        /// Timestamp when the audit was initiated
        /// </summary>
        public DateTime AuditStartTime { get; set; }

        /// <summary>
        /// Timestamp when the audit was completed
        /// </summary>
        public DateTime? AuditEndTime { get; set; }

        /// <summary>
        /// Overall compliance status
        /// </summary>
        public ComplianceStatus OverallStatus { get; set; }

        /// <summary>
        /// Severity of compliance findings
        /// </summary>
        public ComplianceSeverity Severity { get; set; }

        /// <summary>
        /// User or system component that triggered the audit
        /// </summary>
        public string AuditInitiatedBy { get; set; }

        /// <summary>
        /// Detailed audit findings and observations
        /// </summary>
        public List<ComplianceFinding> Findings { get; set; } = new List<ComplianceFinding>();

        /// <summary>
        /// Recommended corrective actions
        /// </summary>
        public List<string> RecommendedActions { get; set; } = new List<string>();

        /// <summary>
        /// Attachments or supporting documentation
        /// </summary>
        public List<string> SupportingDocuments { get; set; } = new List<string>();

        /// <summary>
        /// Calculates the compliance score based on findings
        /// </summary>
        public double CalculateComplianceScore()
        {
            if (Findings == null || Findings.Count == 0)
                return 100.0;

            var criticalFindings = Findings.Count(f => f.Severity == ComplianceSeverity.Critical);
            var highFindings = Findings.Count(f => f.Severity == ComplianceSeverity.High);
            var mediumFindings = Findings.Count(f => f.Severity == ComplianceSeverity.Medium);

            // Weighted scoring mechanism
            double score = 100.0 - (criticalFindings * 30.0 + highFindings * 15.0 + mediumFindings * 5.0);
            return Math.Max(0, score);
        }

        /// <summary>
        /// Generates a comprehensive audit report
        /// </summary>
        public string GenerateAuditReport()
        {
            return $"Audit Report for {RegulatoryStandard}\n" +
                   $"Audit Type: {AuditType}\n" +
                   $"Start Time: {AuditStartTime}\n" +
                   $"End Time: {AuditEndTime}\n" +
                   $"Overall Status: {OverallStatus}\n" +
                   $"Compliance Score: {CalculateComplianceScore():F2}%\n" +
                   $"Total Findings: {Findings?.Count ?? 0}\n" +
                   $"Recommended Actions: {string.Join(", ", RecommendedActions)}";
        }
    }

    /// <summary>
    /// Represents a specific compliance finding during an audit
    /// </summary>
    public class ComplianceFinding
    {
        /// <summary>
        /// Unique identifier for the finding
        /// </summary>
        public Guid FindingId { get; set; }

        /// <summary>
        /// Description of the compliance issue
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Severity of the finding
        /// </summary>
        public ComplianceSeverity Severity { get; set; }

        /// <summary>
        /// Specific section or requirement violated
        /// </summary>
        public string ViolatedSection { get; set; }

        /// <summary>
        /// Recommended corrective action
        /// </summary>
        public string RecommendedAction { get; set; }

        /// <summary>
        /// Evidence or documentation supporting the finding
        /// </summary>
        public List<string> SupportingEvidence { get; set; } = new List<string>();
    }

    /// <summary>
    /// Repository interface for managing compliance audits
    /// </summary>
    public interface IComplianceAuditRepository : IGenericRepository<ComplianceAudit>
    {
        /// <summary>
        /// Find audits by regulatory standard
        /// </summary>
        Task<IEnumerable<ComplianceAudit>> FindByRegulatoryStandardAsync(string standard);

        /// <summary>
        /// Get audits with findings above a certain severity
        /// </summary>
        Task<IEnumerable<ComplianceAudit>> GetAuditsWithHighSeverityFindingsAsync();

        /// <summary>
        /// Calculate overall compliance score across all audits
        /// </summary>
        Task<double> CalculateOverallComplianceScoreAsync();
    }
}
