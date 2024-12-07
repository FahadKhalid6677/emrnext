using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Financial;
using EMRNext.Core.Domain.Models.Financial;

namespace EMRNext.Core.Services.Financial
{
    public interface IClaimService
    {
        // Claim Management
        Task<Claim> CreateClaimAsync(ClaimRequest request);
        Task<Claim> UpdateClaimAsync(int claimId, ClaimRequest request);
        Task<bool> DeleteClaimAsync(int claimId);
        Task<Claim> GetClaimAsync(int claimId);
        Task<IEnumerable<Claim>> GetPatientClaimsAsync(int patientId);
        Task<IEnumerable<Claim>> GetProviderClaimsAsync(int providerId);

        // Claim Processing
        Task<bool> SubmitClaimAsync(int claimId);
        Task<bool> ResubmitClaimAsync(int claimId);
        Task<bool> VoidClaimAsync(int claimId, string reason);
        Task<bool> ProcessClaimResponseAsync(ClaimResponse response);
        Task<bool> ProcessClaimPaymentAsync(ClaimPayment payment);
        Task<bool> ProcessClaimDenialAsync(ClaimDenial denial);

        // Claim Items
        Task<ClaimItem> AddClaimItemAsync(int claimId, ClaimItemRequest request);
        Task<ClaimItem> UpdateClaimItemAsync(int itemId, ClaimItemRequest request);
        Task<bool> DeleteClaimItemAsync(int itemId);
        Task<IEnumerable<ClaimItem>> GetClaimItemsAsync(int claimId);

        // Claim Adjustments
        Task<ClaimAdjustment> CreateAdjustmentAsync(int claimId, AdjustmentRequest request);
        Task<bool> DeleteAdjustmentAsync(int adjustmentId);
        Task<IEnumerable<ClaimAdjustment>> GetClaimAdjustmentsAsync(int claimId);

        // Document Management
        Task<ClaimDocument> AttachDocumentAsync(int claimId, DocumentRequest request);
        Task<bool> DeleteDocumentAsync(int documentId);
        Task<IEnumerable<ClaimDocument>> GetClaimDocumentsAsync(int claimId);

        // Validation and Verification
        Task<bool> ValidateClaimAsync(int claimId);
        Task<bool> VerifyInsuranceAsync(int claimId);
        Task<bool> CheckEligibilityAsync(int claimId);
        Task<bool> ValidateAuthorizationAsync(int claimId);

        // Reporting
        Task<ClaimReport> GenerateClaimReportAsync(int claimId);
        Task<IEnumerable<ClaimSummary>> GetClaimSummaryAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<ClaimMetric>> GetClaimMetricsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<DenialReport>> GetDenialReportsAsync(DateTime startDate, DateTime endDate);

        // Electronic Data Interchange (EDI)
        Task<string> GenerateEDI837Async(int claimId);
        Task<bool> ProcessEDI835Async(string ediContent);
        Task<bool> ProcessEDI277Async(string ediContent);
        Task<bool> ProcessEDI999Async(string ediContent);

        // Batch Processing
        Task<BatchResult> ProcessClaimBatchAsync(IEnumerable<ClaimRequest> claims);
        Task<BatchResult> ProcessPaymentBatchAsync(IEnumerable<ClaimPayment> payments);
        Task<BatchResult> ProcessEDIBatchAsync(string batchContent);

        // Notifications
        Task<bool> SendClaimNotificationAsync(int claimId, string notificationType);
        Task<bool> SendPaymentNotificationAsync(int claimId);
        Task<bool> SendDenialNotificationAsync(int claimId);

        // Analytics
        Task<AnalyticsReport> GenerateAnalyticsReportAsync(AnalyticsRequest request);
        Task<IEnumerable<TrendReport>> GetTrendReportsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PerformanceMetric>> GetPerformanceMetricsAsync();
    }
}
