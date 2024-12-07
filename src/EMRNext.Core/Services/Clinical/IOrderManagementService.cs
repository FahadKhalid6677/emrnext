using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Clinical
{
    public interface IOrderManagementService
    {
        Task<Order> CreateOrderAsync(OrderRequest request);
        Task<Order> GetOrderAsync(string id);
        Task<IEnumerable<Order>> GetPatientOrdersAsync(string patientId);
        Task<Order> UpdateOrderAsync(string id, OrderRequest request);
        Task<bool> CancelOrderAsync(string id, string reason);
        Task<IEnumerable<OrderSet>> GetOrderSetsAsync(string specialtyId);
        Task<OrderResult> AddResultAsync(string orderId, OrderResultRequest result);
        Task<IEnumerable<OrderResult>> GetOrderResultsAsync(string orderId);
    }
}
