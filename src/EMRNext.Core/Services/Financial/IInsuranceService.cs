using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Financial;
using EMRNext.Core.Domain.Models.Financial;

namespace EMRNext.Core.Services.Financial
{
    public interface IInsuranceService
    {
        // Insurance Management
        Task<Insurance> CreateInsuranceAsync(InsuranceRequest request);
        Task<Insurance> UpdateInsuranceAsync(int insuranceId, InsuranceRequest request);
        Task<bool> DeleteInsuranceAsync(int insuranceId);
        Task<Insurance> GetInsuranceAsync(int insuranceId);
        Task<IEnumerable<Insurance>> GetPatientInsurancesAsync(int patientId);
        Task<Insurance> GetPrimaryInsuranceAsync(int patientId);

        // Insurance Verification
        Task<InsuranceVerification> VerifyInsuranceAsync(int insuranceId);
        Task<InsuranceVerification> VerifyEligibilityAsync(int insuranceId, string serviceCode);
        Task<InsuranceVerification> CheckBenefitsAsync(int insuranceId);
        Task<bool> UpdateVerificationAsync(int verificationId, VerificationUpdate update);
        Task<IEnumerable<InsuranceVerification>> GetVerificationHistoryAsync(int insuranceId);

        // Authorization Management
        Task<InsuranceAuthorization> RequestAuthorizationAsync(AuthorizationRequest request);
        Task<InsuranceAuthorization> UpdateAuthorizationAsync(int authorizationId, AuthorizationUpdate update);
        Task<bool> CancelAuthorizationAsync(int authorizationId, string reason);
        Task<InsuranceAuthorization> GetAuthorizationAsync(int authorizationId);
        Task<IEnumerable<InsuranceAuthorization>> GetAuthorizationsAsync(int insuranceId);
        Task<bool> TrackAuthorizationUsageAsync(int authorizationId, int unitsUsed);

        // Document Management
        Task<InsuranceDocument> AttachDocumentAsync(int insuranceId, DocumentRequest request);
        Task<bool> DeleteDocumentAsync(int documentId);
        Task<IEnumerable<InsuranceDocument>> GetInsuranceDocumentsAsync(int insuranceId);

        // EDI Operations
        Task<string> GenerateEDI270Async(int insuranceId);
        Task<bool> ProcessEDI271Async(string ediContent);
        Task<string> GenerateEDI278Async(AuthorizationRequest request);
        Task<bool> ProcessEDI278ResponseAsync(string ediContent);

        // Reporting
        Task<InsuranceReport> GenerateInsuranceReportAsync(int insuranceId);
        Task<IEnumerable<InsuranceSummary>> GetInsuranceSummaryAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<VerificationMetric>> GetVerificationMetricsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<AuthorizationMetric>> GetAuthorizationMetricsAsync(DateTime startDate, DateTime endDate);

        // Batch Operations
        Task<BatchResult> ProcessVerificationBatchAsync(IEnumerable<int> insuranceIds);
        Task<BatchResult> ProcessAuthorizationBatchAsync(IEnumerable<AuthorizationRequest> requests);
        Task<BatchResult> ProcessEDIBatchAsync(string batchContent);

        // Notifications
        Task<bool> SendVerificationNotificationAsync(int verificationId);
        Task<bool> SendAuthorizationNotificationAsync(int authorizationId);
        Task<bool> SendExpirationNotificationAsync(int insuranceId);

        // Analytics
        Task<AnalyticsReport> GenerateAnalyticsReportAsync(AnalyticsRequest request);
        Task<IEnumerable<TrendReport>> GetTrendReportsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PerformanceMetric>> GetPerformanceMetricsAsync();

        // Validation
        Task<bool> ValidateInsuranceAsync(int insuranceId);
        Task<bool> ValidateCoverageAsync(int insuranceId, string serviceCode);
        Task<bool> ValidateAuthorizationRequirementAsync(int insuranceId, string serviceCode);
    }
}
