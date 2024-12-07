using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Enums;
using EMRNext.Core.Infrastructure.Configuration;
using EMRNext.Core.Repositories;

namespace EMRNext.Core.Services.Compliance
{
    /// <summary>
    /// Advanced service for managing compliance audits and tracking
    /// </summary>
    public class ComplianceAuditService : IComplianceAuditService
    {
        private readonly IComplianceAuditRepository _auditRepository;
        private readonly ILogger<ComplianceAuditService> _logger;
        private readonly ComplianceConfiguration _complianceConfig;

        public ComplianceAuditService(
            IComplianceAuditRepository auditRepository,
            ILogger<ComplianceAuditService> logger,
            IOptions<ComplianceConfiguration> complianceConfig)
        {
            _auditRepository = auditRepository;
            _logger = logger;
            _complianceConfig = complianceConfig.Value;
        }

        /// <inheritdoc/>
        public async Task<ComplianceAudit> InitiateAuditAsync(
            ComplianceAuditType auditType, 
            string regulatoryStandard)
        {
            try 
            {
                var audit = new ComplianceAudit
                {
                    AuditId = Guid.NewGuid(),
                    AuditType = auditType,
                    RegulatoryStandard = regulatoryStandard,
                    AuditStartTime = DateTime.UtcNow,
                    AuditInitiatedBy = Environment.MachineName,
                    OverallStatus = ComplianceStatus.InProgress
                };

                await _auditRepository.AddAsync(audit);
                await _auditRepository.SaveChangesAsync();

                _logger.LogInformation(
                    $"Initiated {auditType} audit for {regulatoryStandard}");

                return audit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    $"Error initiating {auditType} audit for {regulatoryStandard}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ComplianceAudit> AddAuditFindingAsync(
            Guid auditId, 
            ComplianceFinding finding)
        {
            try 
            {
                var audit = await _auditRepository.GetByIdAsync(auditId);
                if (audit == null)
                    throw new InvalidOperationException("Audit not found");

                finding.FindingId = Guid.NewGuid();
                audit.Findings.Add(finding);

                // Update audit status based on finding severity
                UpdateAuditStatus(audit, finding);

                await _auditRepository.UpdateAsync(audit);
                await _auditRepository.SaveChangesAsync();

                _logger.LogWarning(
                    $"Added {finding.Severity} finding to audit {auditId}: {finding.Description}");

                return audit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    $"Error adding finding to audit {auditId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ComplianceAudit> CompleteAuditAsync(Guid auditId)
        {
            try 
            {
                var audit = await _auditRepository.GetByIdAsync(auditId);
                if (audit == null)
                    throw new InvalidOperationException("Audit not found");

                audit.AuditEndTime = DateTime.UtcNow;
                audit.OverallStatus = DetermineOverallComplianceStatus(audit);

                await _auditRepository.UpdateAsync(audit);
                await _auditRepository.SaveChangesAsync();

                _logger.LogInformation(
                    $"Completed audit {auditId} with status {audit.OverallStatus}");

                return audit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    $"Error completing audit {auditId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ComplianceReport> GenerateComplianceReportAsync()
        {
            try 
            {
                var audits = await _auditRepository.GetAllAsync();
                var overallScore = await _auditRepository.CalculateOverallComplianceScoreAsync();

                return new ComplianceReport
                {
                    GeneratedAt = DateTime.UtcNow,
                    OverallComplianceScore = overallScore,
                    TotalAudits = audits.Count(),
                    AuditsSummary = audits.Select(a => new AuditSummary
                    {
                        AuditId = a.AuditId,
                        RegulatoryStandard = a.RegulatoryStandard,
                        Status = a.OverallStatus,
                        ComplianceScore = a.CalculateComplianceScore()
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compliance report");
                throw;
            }
        }

        /// <summary>
        /// Update audit status based on finding severity
        /// </summary>
        private void UpdateAuditStatus(ComplianceAudit audit, ComplianceFinding finding)
        {
            if (finding.Severity == ComplianceSeverity.Critical)
                audit.OverallStatus = ComplianceStatus.CriticalNonCompliance;
            else if (finding.Severity == ComplianceSeverity.High)
                audit.OverallStatus = ComplianceStatus.NonCompliant;
        }

        /// <summary>
        /// Determine overall compliance status based on audit findings
        /// </summary>
        private ComplianceStatus DetermineOverallComplianceStatus(ComplianceAudit audit)
        {
            var complianceScore = audit.CalculateComplianceScore();

            return complianceScore switch
            {
                >= 90 => ComplianceStatus.Compliant,
                >= 70 => ComplianceStatus.PartiallyCompliant,
                _ => ComplianceStatus.NonCompliant
            };
        }
    }

    /// <summary>
    /// Represents a comprehensive compliance report
    /// </summary>
    public class ComplianceReport
    {
        /// <summary>
        /// Timestamp of report generation
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Overall compliance score across all audits
        /// </summary>
        public double OverallComplianceScore { get; set; }

        /// <summary>
        /// Total number of audits
        /// </summary>
        public int TotalAudits { get; set; }

        /// <summary>
        /// Summary of individual audits
        /// </summary>
        public List<AuditSummary> AuditsSummary { get; set; }
    }

    /// <summary>
    /// Summary of an individual audit
    /// </summary>
    public class AuditSummary
    {
        /// <summary>
        /// Unique identifier of the audit
        /// </summary>
        public Guid AuditId { get; set; }

        /// <summary>
        /// Regulatory standard audited
        /// </summary>
        public string RegulatoryStandard { get; set; }

        /// <summary>
        /// Overall compliance status
        /// </summary>
        public ComplianceStatus Status { get; set; }

        /// <summary>
        /// Compliance score for this audit
        /// </summary>
        public double ComplianceScore { get; set; }
    }

    /// <summary>
    /// Interface for compliance audit service
    /// </summary>
    public interface IComplianceAuditService
    {
        /// <summary>
        /// Initiate a new compliance audit
        /// </summary>
        Task<ComplianceAudit> InitiateAuditAsync(
            ComplianceAuditType auditType, 
            string regulatoryStandard);

        /// <summary>
        /// Add a finding to an existing audit
        /// </summary>
        Task<ComplianceAudit> AddAuditFindingAsync(
            Guid auditId, 
            ComplianceFinding finding);

        /// <summary>
        /// Complete an ongoing audit
        /// </summary>
        Task<ComplianceAudit> CompleteAuditAsync(Guid auditId);

        /// <summary>
        /// Generate a comprehensive compliance report
        /// </summary>
        Task<ComplianceReport> GenerateComplianceReportAsync();
    }
}
