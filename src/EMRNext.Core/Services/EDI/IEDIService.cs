using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Models.EDI;

namespace EMRNext.Core.Services.EDI
{
    public interface IEDIService
    {
        // EDI 837 (Claims)
        Task<string> GenerateEDI837Async(EDI837Request request);
        Task<bool> SendEDI837Async(string ediContent);
        Task<EDI837Response> ProcessEDI837ResponseAsync(string ediContent);
        Task<bool> ValidateEDI837Async(string ediContent);

        // EDI 835 (Payment/Remittance)
        Task<EDI835Response> ProcessEDI835Async(string ediContent);
        Task<bool> ValidateEDI835Async(string ediContent);
        Task<PaymentReport> GenerateEDI835ReportAsync(string ediContent);

        // EDI 270/271 (Eligibility)
        Task<string> GenerateEDI270Async(EDI270Request request);
        Task<string> SendEDI270Async(string ediContent);
        Task<EDI271Response> ProcessEDI271Async(string ediContent);
        Task<bool> ValidateEDI270Async(string ediContent);

        // EDI 278 (Authorization)
        Task<string> GenerateEDI278Async(EDI278Request request);
        Task<string> SendEDI278Async(string ediContent);
        Task<EDI278Response> ProcessEDI278ResponseAsync(string ediContent);
        Task<bool> ValidateEDI278Async(string ediContent);

        // EDI 277 (Claim Status)
        Task<EDI277Response> ProcessEDI277Async(string ediContent);
        Task<bool> ValidateEDI277Async(string ediContent);
        Task<StatusReport> GenerateEDI277ReportAsync(string ediContent);

        // EDI 999 (Functional Acknowledgment)
        Task<EDI999Response> ProcessEDI999Async(string ediContent);
        Task<bool> ValidateEDI999Async(string ediContent);
        Task<bool> GenerateEDI999Async(string originalEdiContent);

        // Batch Processing
        Task<BatchResult> ProcessEDIBatchAsync(string batchContent, string transactionType);
        Task<BatchResult> SendEDIBatchAsync(IEnumerable<string> ediContents, string transactionType);
        Task<BatchReport> GenerateBatchReportAsync(string batchId);

        // Trading Partner Management
        Task<bool> ValidateTradingPartnerAsync(string partnerId);
        Task<bool> TestTradingPartnerConnectionAsync(string partnerId);
        Task<TradingPartnerConfig> GetTradingPartnerConfigAsync(string partnerId);

        // Monitoring and Reporting
        Task<IEnumerable<EDITransaction>> GetTransactionHistoryAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<EDIError>> GetErrorReportAsync(DateTime startDate, DateTime endDate);
        Task<EDIMetrics> GetPerformanceMetricsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<EDIAudit>> GetAuditTrailAsync(string transactionId);

        // Configuration
        Task<bool> UpdateEDIConfigAsync(EDIConfig config);
        Task<EDIConfig> GetEDIConfigAsync();
        Task<bool> ValidateEDIConfigAsync(EDIConfig config);
        Task<bool> TestEDIConfigAsync(EDIConfig config);

        // Error Handling
        Task<bool> HandleEDIErrorAsync(EDIError error);
        Task<bool> ReprocessFailedTransactionAsync(string transactionId);
        Task<bool> NotifyEDIErrorAsync(EDIError error);
        Task<IEnumerable<EDIError>> GetPendingErrorsAsync();
    }
}
