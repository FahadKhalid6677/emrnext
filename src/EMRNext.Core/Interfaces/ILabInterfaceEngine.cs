using System;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Clinical;

namespace EMRNext.Core.Interfaces
{
    public interface ILabInterfaceEngine
    {
        Task<bool> SendOrderAsync(LabOrderEntity order);
        Task<bool> ReceiveResultAsync(string externalOrderId, string resultData);
        Task<string> GetOrderStatusAsync(string externalOrderId);
        Task<bool> CancelOrderAsync(string externalOrderId);
        Task<bool> UpdateOrderAsync(string externalOrderId, string updateData);
        Task<bool> ValidateConnectionAsync();
        Task<bool> HandleErrorAsync(string errorCode, string errorMessage);
        Task<string> TranslateOrderToHL7Async(LabOrderEntity order);
        Task<LabResultEntity> TranslateHL7ToResultAsync(string hl7Message);
        Task<bool> SendAcknowledgementAsync(string messageId, bool isAccepted, string comments);
    }
}
