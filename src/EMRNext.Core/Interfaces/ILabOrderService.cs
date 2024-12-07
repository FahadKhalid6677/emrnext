using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Clinical;
using EMRNext.Core.Models.Laboratory;

namespace EMRNext.Core.Interfaces
{
    public interface ILabOrderService
    {
        Task<LabOrderEntity> CreateOrderAsync(LabOrderRequest request);
        Task<LabOrderEntity> UpdateOrderAsync(int orderId, LabOrderRequest request);
        Task<bool> CancelOrderAsync(int orderId, string reason);
        Task<LabOrderEntity> GetOrderAsync(int orderId);
        Task<IEnumerable<LabOrderEntity>> GetPatientOrdersAsync(int patientId);
        Task<IEnumerable<LabOrderEntity>> GetPendingOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> AssignExternalLabAsync(int orderId, int externalLabId);
        Task<bool> RequestAuthorizationAsync(int orderId);
        Task<bool> UpdateResultAsync(int orderId, LabResultRequest result);
        Task<bool> FlagAbnormalResultAsync(int orderId, string comment);
        Task<IEnumerable<LabResultEntity>> GetPendingResultsAsync();
        Task<bool> AttachOrderDocumentAsync(int orderId, DocumentRequest document);
        Task<bool> AttachResultDocumentAsync(int orderId, DocumentRequest document);
        Task<IEnumerable<DocumentEntity>> GetOrderDocumentsAsync(int orderId);
        Task<bool> CreateResultAlertAsync(int orderId, AlertRequest alert);
        Task<bool> AcknowledgeAlertAsync(int alertId, string comment);
        Task<IEnumerable<AlertEntity>> GetPendingAlertsAsync();
        Task<IEnumerable<LabTestDefinitionEntity>> GetAvailableTestsAsync();
        Task<LabTestDefinitionEntity> GetTestDetailsAsync(int testId);
        Task<IEnumerable<ExternalLabEntity>> GetActiveExternalLabsAsync();
        Task<byte[]> GenerateOrderReportAsync(int orderId);
        Task<byte[]> GenerateResultReportAsync(int orderId);
        Task<LabOrderSummary> GetOrderSummaryAsync(DateTime startDate, DateTime endDate);
        Task<bool> SendToInterfaceAsync(int orderId);
        Task<bool> HandleInterfaceErrorAsync(InterfaceError error);
        Task<byte[]> GenerateQualityReportAsync(DateTime startDate, DateTime endDate);
        Task<bool> FlagQualityIssueAsync(int orderId, QualityIssue issue);
        Task<QualityMetrics> GetQualityMetricsAsync();
    }
}
