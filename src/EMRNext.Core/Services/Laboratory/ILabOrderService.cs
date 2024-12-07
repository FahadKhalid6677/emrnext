using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Laboratory;
using EMRNext.Core.Domain.Models.Laboratory;

namespace EMRNext.Core.Services.Laboratory
{
    public interface ILabOrderService
    {
        // Order Management
        Task<LabOrder> CreateOrderAsync(LabOrderRequest request);
        Task<LabOrder> UpdateOrderAsync(int orderId, LabOrderRequest request);
        Task<bool> CancelOrderAsync(int orderId, string reason);
        Task<LabOrder> GetOrderAsync(int orderId);
        Task<IEnumerable<LabOrder>> GetPatientOrdersAsync(int patientId);
        Task<IEnumerable<LabOrder>> GetPendingOrdersAsync();
        
        // Order Processing
        Task<bool> SubmitToLabAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> AssignExternalLabAsync(int orderId, int externalLabId);
        Task<bool> RequestAuthorizationAsync(int orderId);
        
        // Result Management
        Task<LabResult> RecordResultAsync(LabResultRequest request);
        Task<bool> UpdateResultAsync(int resultId, LabResultRequest request);
        Task<bool> ReviewResultAsync(int resultId, string reviewerId);
        Task<bool> FlagAbnormalResultAsync(int resultId, string flag);
        Task<IEnumerable<LabResult>> GetPendingResultsAsync();
        
        // Document Management
        Task<LabOrderDocument> AttachOrderDocumentAsync(int orderId, DocumentRequest request);
        Task<LabResultDocument> AttachResultDocumentAsync(int resultId, DocumentRequest request);
        Task<IEnumerable<LabOrderDocument>> GetOrderDocumentsAsync(int orderId);
        
        // Alert Management
        Task<LabOrderAlert> CreateOrderAlertAsync(int orderId, AlertRequest request);
        Task<LabResultAlert> CreateResultAlertAsync(int resultId, AlertRequest request);
        Task<bool> AcknowledgeAlertAsync(int alertId, string userId);
        Task<IEnumerable<LabOrderAlert>> GetPendingAlertsAsync();
        
        // Reference Data
        Task<IEnumerable<LabTest>> GetAvailableTestsAsync();
        Task<LabTest> GetTestDetailsAsync(int testId);
        Task<IEnumerable<ExternalLab>> GetActiveExternalLabsAsync();
        
        // Reporting
        Task<LabOrderReport> GenerateOrderReportAsync(int orderId);
        Task<LabResultReport> GenerateResultReportAsync(int resultId);
        Task<IEnumerable<LabOrderSummary>> GetOrderSummaryAsync(DateTime startDate, DateTime endDate);
        
        // Interface Management
        Task<bool> SendToInterfaceAsync(int orderId);
        Task<bool> ProcessInterfaceResultAsync(InterfaceResult result);
        Task<bool> HandleInterfaceErrorAsync(InterfaceError error);
        
        // Quality Management
        Task<QualityReport> GenerateQualityReportAsync(DateTime startDate, DateTime endDate);
        Task<bool> FlagQualityIssueAsync(int orderId, QualityIssue issue);
        Task<IEnumerable<QualityMetric>> GetQualityMetricsAsync();
    }
}
