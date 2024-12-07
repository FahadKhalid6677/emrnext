using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Services
{
    public class OrderEntryService
    {
        private readonly IRepository<ClinicalOrder> _orderRepository;
        private readonly IRepository<Patient> _patientRepository;
        private readonly IMedicationInteractionService _medicationInteractionService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrderEntryService> _logger;

        public OrderEntryService(
            IRepository<ClinicalOrder> orderRepository,
            IRepository<Patient> patientRepository,
            IMedicationInteractionService medicationInteractionService,
            INotificationService notificationService,
            ILogger<OrderEntryService> logger)
        {
            _orderRepository = orderRepository;
            _patientRepository = patientRepository;
            _medicationInteractionService = medicationInteractionService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ClinicalOrder> CreateOrderAsync(ClinicalOrder order)
        {
            try
            {
                // Validate patient
                var patient = await _patientRepository.GetByIdAsync(order.PatientId);
                if (patient == null)
                {
                    throw new ArgumentException("Invalid patient ID");
                }

                // Set initial order details
                order.Id = Guid.NewGuid();
                order.OrderedAt = DateTime.UtcNow;
                order.Status = OrderStatus.Pending;

                // Perform specific validations based on order type
                await ValidateOrderAsync(order);

                // Handle medication-specific checks
                if (order is MedicationOrder medicationOrder)
                {
                    await HandleMedicationOrderAsync(medicationOrder);
                }

                // Save order
                await _orderRepository.AddAsync(order);

                // Notify relevant parties
                await _notificationService.NotifyOrderCreatedAsync(order);

                _logger.LogInformation("Order created: {OrderId}", order.Id);

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                throw;
            }
        }

        private async Task ValidateOrderAsync(ClinicalOrder order)
        {
            // Basic order validation
            if (string.IsNullOrWhiteSpace(order.OrderedBy))
            {
                throw new ArgumentException("Order must have an ordering provider");
            }

            // Type-specific validations
            switch (order)
            {
                case MedicationOrder medOrder:
                    await ValidateMedicationOrderAsync(medOrder);
                    break;
                case DiagnosticTestOrder testOrder:
                    ValidateDiagnosticTestOrder(testOrder);
                    break;
                case ProcedureOrder procedureOrder:
                    ValidateProcedureOrder(procedureOrder);
                    break;
                case ReferralOrder referralOrder:
                    ValidateReferralOrder(referralOrder);
                    break;
            }
        }

        private async Task ValidateMedicationOrderAsync(MedicationOrder order)
        {
            if (string.IsNullOrWhiteSpace(order.MedicationId))
            {
                throw new ArgumentException("Medication ID is required");
            }

            // Check medication interactions
            var interactions = await _medicationInteractionService.CheckMedicationInteractionsAsync(
                order.PatientId, 
                order.MedicationId
            );

            order.PotentialInteractions = interactions
                .Where(i => i.Severity >= InteractionSeverity.Moderate)
                .ToList();

            // Notify about significant interactions
            if (order.PotentialInteractions.Any())
            {
                await _notificationService.NotifyMedicationInteractionsAsync(order);
            }
        }

        private void ValidateDiagnosticTestOrder(DiagnosticTestOrder order)
        {
            if (string.IsNullOrWhiteSpace(order.TestCode))
            {
                throw new ArgumentException("Test code is required");
            }
        }

        private void ValidateProcedureOrder(ProcedureOrder order)
        {
            if (string.IsNullOrWhiteSpace(order.ProcedureCode))
            {
                throw new ArgumentException("Procedure code is required");
            }
        }

        private void ValidateReferralOrder(ReferralOrder order)
        {
            if (string.IsNullOrWhiteSpace(order.SpecialtyId))
            {
                throw new ArgumentException("Referral specialty is required");
            }
        }

        private async Task HandleMedicationOrderAsync(MedicationOrder order)
        {
            // Additional medication-specific processing
            // Could include dosage calculations, pharmacy routing, etc.
        }

        public async Task<ClinicalOrder> UpdateOrderStatusAsync(
            Guid orderId, 
            OrderStatus newStatus, 
            string updatedBy)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new ArgumentException("Order not found");
                }

                // Validate status transition
                ValidateStatusTransition(order.Status, newStatus);

                // Update status
                order.Status = newStatus;

                // Add action to history
                order.ActionHistory ??= new List<OrderAction>();
                order.ActionHistory.Add(new OrderAction
                {
                    Id = Guid.NewGuid(),
                    Type = MapStatusToActionType(newStatus),
                    PerformedBy = updatedBy,
                    PerformedAt = DateTime.UtcNow
                });

                // Calculate compliance metrics
                UpdateComplianceMetrics(order);

                // Save updated order
                await _orderRepository.UpdateAsync(order);

                // Notify about status change
                await _notificationService.NotifyOrderStatusChangeAsync(order);

                _logger.LogInformation(
                    "Order {OrderId} status updated to {NewStatus}", 
                    orderId, 
                    newStatus
                );

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                throw;
            }
        }

        private void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.Pending, new List<OrderStatus> 
                    { OrderStatus.Approved, OrderStatus.Cancelled, OrderStatus.Rejected } },
                { OrderStatus.Approved, new List<OrderStatus> 
                    { OrderStatus.InProgress, OrderStatus.Cancelled } },
                { OrderStatus.InProgress, new List<OrderStatus> 
                    { OrderStatus.Completed, OrderStatus.OnHold, OrderStatus.Cancelled } }
            };

            if (!validTransitions.TryGetValue(currentStatus, out var allowedStatuses) ||
                !allowedStatuses.Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"Invalid status transition from {currentStatus} to {newStatus}"
                );
            }
        }

        private OrderActionType MapStatusToActionType(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Approved => OrderActionType.Approved,
                OrderStatus.Cancelled => OrderActionType.Cancelled,
                OrderStatus.Completed => OrderActionType.Completed,
                OrderStatus.Rejected => OrderActionType.Rejected,
                _ => OrderActionType.Updated
            };
        }

        private void UpdateComplianceMetrics(ClinicalOrder order)
        {
            // Basic compliance calculation
            order.ComplianceMetrics = new OrderComplianceMetrics
            {
                IsTimelySigned = order.ActionHistory?.Any(a => 
                    a.Type == OrderActionType.Approved && 
                    a.PerformedAt <= order.OrderedAt.AddHours(24)) ?? false,
                IsCompletedOnTime = order.Status == OrderStatus.Completed && 
                    order.ActionHistory?.Any(a => 
                        a.Type == OrderActionType.Completed && 
                        a.PerformedAt <= order.OrderedAt.AddDays(7)) ?? false,
                ComplianceScore = CalculateComplianceScore(order)
            };
        }

        private decimal CalculateComplianceScore(ClinicalOrder order)
        {
            // Implement a comprehensive compliance scoring mechanism
            decimal baseScore = 100m;

            // Deduct points for delays, cancellations, etc.
            if (order.Status == OrderStatus.Cancelled)
                baseScore -= 50;

            if (order.ActionHistory?.Count(a => 
                a.Type == OrderActionType.Updated) > 2)
                baseScore -= 20;

            return Math.Max(0, baseScore);
        }

        public async Task<List<ClinicalOrder>> GetPatientOrdersAsync(
            string patientId, 
            OrderType? orderType = null, 
            OrderStatus? status = null)
        {
            try
            {
                var query = await _orderRepository.FindAsync(o => o.PatientId == patientId);

                if (orderType.HasValue)
                    query = query.Where(o => o.Type == orderType.Value);

                if (status.HasValue)
                    query = query.Where(o => o.Status == status.Value);

                return query
                    .OrderByDescending(o => o.OrderedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient orders");
                throw;
            }
        }

        public async Task<OrderAnalytics> GenerateOrderAnalyticsAsync(
            DateTime startDate, 
            DateTime endDate)
        {
            try
            {
                var orders = await _orderRepository.FindAsync(o => 
                    o.OrderedAt >= startDate && o.OrderedAt <= endDate);

                return new OrderAnalytics
                {
                    TotalOrders = orders.Count(),
                    OrderTypeBreakdown = orders
                        .GroupBy(o => o.Type)
                        .Select(g => new OrderTypeAnalytics
                        {
                            Type = g.Key,
                            Count = g.Count(),
                            CompletionRate = CalculateCompletionRate(g)
                        })
                        .ToList(),
                    StatusBreakdown = orders
                        .GroupBy(o => o.Status)
                        .Select(g => new OrderStatusAnalytics
                        {
                            Status = g.Key,
                            Count = g.Count()
                        })
                        .ToList(),
                    AverageComplianceScore = orders
                        .Average(o => o.ComplianceMetrics?.ComplianceScore ?? 0)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating order analytics");
                throw;
            }
        }

        private decimal CalculateCompletionRate(IGrouping<OrderType, ClinicalOrder> orderGroup)
        {
            var totalOrders = orderGroup.Count();
            var completedOrders = orderGroup.Count(o => o.Status == OrderStatus.Completed);

            return totalOrders > 0 
                ? (decimal)completedOrders / totalOrders * 100 
                : 0;
        }
    }

    // Analytics models
    public class OrderAnalytics
    {
        public int TotalOrders { get; set; }
        public List<OrderTypeAnalytics> OrderTypeBreakdown { get; set; }
        public List<OrderStatusAnalytics> StatusBreakdown { get; set; }
        public decimal AverageComplianceScore { get; set; }
    }

    public class OrderTypeAnalytics
    {
        public OrderType Type { get; set; }
        public int Count { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class OrderStatusAnalytics
    {
        public OrderStatus Status { get; set; }
        public int Count { get; set; }
    }
}
