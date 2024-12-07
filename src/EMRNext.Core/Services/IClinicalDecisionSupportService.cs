using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services
{
    public interface IClinicalDecisionSupportService
    {
        // Rule Management
        Task<ClinicalRule> CreateRuleAsync(ClinicalRule rule);
        Task<ClinicalRule> UpdateRuleAsync(ClinicalRule rule);
        Task<ClinicalRule> GetRuleByIdAsync(int ruleId);
        Task<IEnumerable<ClinicalRule>> GetActiveRulesAsync();
        Task<IEnumerable<ClinicalRule>> GetRulesBySpecialtyAsync(string specialtyType);
        Task DeactivateRuleAsync(int ruleId);
        
        // Rule Evaluation
        Task<IEnumerable<RuleEvaluation>> EvaluatePatientRulesAsync(int patientId, string context);
        Task<IEnumerable<RuleEvaluation>> EvaluateEncounterRulesAsync(int encounterId);
        Task<RuleEvaluation> ProcessRuleEvaluationAsync(RuleEvaluation evaluation);
        Task<IEnumerable<RuleEvaluation>> GetPendingAlertsAsync(int providerId);
        
        // Override Management
        Task<RuleOverride> CreateOverrideAsync(RuleOverride ruleOverride);
        Task<RuleOverride> UpdateOverrideAsync(RuleOverride ruleOverride);
        Task<IEnumerable<RuleOverride>> GetActiveOverridesAsync(int patientId);
        Task<IEnumerable<RuleOverride>> GetPendingReviewsAsync();
        Task<RuleOverride> ReviewOverrideAsync(int overrideId, string reviewedBy, string outcome, string notes);
        
        // Notification Management
        Task SendAlertNotificationAsync(RuleEvaluation evaluation);
        Task ProcessAlertResponseAsync(int evaluationId, string action, string actionBy, string notes);
        
        // Reporting
        Task<IEnumerable<RuleEvaluation>> GetRuleEvaluationHistoryAsync(int patientId);
        Task<IEnumerable<RuleOverride>> GetOverrideHistoryAsync(int patientId);
        Task<IDictionary<string, int>> GetRuleStatisticsAsync(DateTime startDate, DateTime endDate);
    }
}
