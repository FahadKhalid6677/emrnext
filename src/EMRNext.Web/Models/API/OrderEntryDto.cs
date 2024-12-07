using System;
using System.Collections.Generic;
using System.Text.Json;
using EMRNext.Core.Models;

namespace EMRNext.Web.Models.API
{
    public class CreateOrderRequestDto
    {
        public string PatientId { get; set; }
        public OrderType Type { get; set; }
        public OrderPriority Priority { get; set; }
        public string Notes { get; set; }
        public JsonElement Details { get; set; }
    }

    public class UpdateOrderStatusRequestDto
    {
        public OrderStatus NewStatus { get; set; }
        public string Reason { get; set; }
    }

    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public string OrderedBy { get; set; }
        public DateTime OrderedAt { get; set; }
        public OrderType Type { get; set; }
        public OrderPriority Priority { get; set; }
        public OrderStatus Status { get; set; }
        public string Notes { get; set; }
        public List<MedicationInteractionDto> PotentialInteractions { get; set; }
    }

    public class MedicationInteractionDto
    {
        public string MedicationId { get; set; }
        public InteractionSeverity Severity { get; set; }
        public string InteractionDescription { get; set; }
    }

    public class OrderAnalyticsDto
    {
        public int TotalOrders { get; set; }
        public List<OrderTypeAnalyticsDto> OrderTypeBreakdown { get; set; }
        public List<OrderStatusAnalyticsDto> StatusBreakdown { get; set; }
        public decimal AverageComplianceScore { get; set; }
    }

    public class OrderTypeAnalyticsDto
    {
        public OrderType Type { get; set; }
        public int Count { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class OrderStatusAnalyticsDto
    {
        public OrderStatus Status { get; set; }
        public int Count { get; set; }
    }
}
