using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities.Laboratory;
using EMRNext.Core.Domain.Models.Laboratory;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Services.Interface;
using EMRNext.Core.Services.Document;
using EMRNext.Core.Services.Notification;
using EMRNext.Core.Services.Clinical;

namespace EMRNext.Core.Services.Laboratory
{
    public class LabOrderService : ILabOrderService
    {
        private readonly EMRNextDbContext _context;
        private readonly ILabInterfaceEngine _interfaceEngine;
        private readonly IDocumentService _documentService;
        private readonly INotificationService _notificationService;
        private readonly IClinicalService _clinicalService;
        private readonly IAuditService _auditService;
        private readonly IQualityService _qualityService;

        public LabOrderService(
            EMRNextDbContext context,
            ILabInterfaceEngine interfaceEngine,
            IDocumentService documentService,
            INotificationService notificationService,
            IClinicalService clinicalService,
            IAuditService auditService,
            IQualityService qualityService)
        {
            _context = context;
            _interfaceEngine = interfaceEngine;
            _documentService = documentService;
            _notificationService = notificationService;
            _clinicalService = clinicalService;
            _auditService = auditService;
            _qualityService = qualityService;
        }

        public async Task<LabOrder> CreateOrderAsync(LabOrderRequest request)
        {
            // Validate order request
            await ValidateOrderRequestAsync(request);

            var order = new LabOrder
            {
                OrderNumber = await GenerateOrderNumberAsync(),
                PatientId = request.PatientId,
                ProviderId = request.ProviderId,
                EncounterId = request.EncounterId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Priority = request.Priority,
                ClinicalNotes = request.ClinicalNotes,
                DiagnosisCodes = request.DiagnosisCodes,
                IsFasting = request.IsFasting,
                SpecimenType = request.SpecimenType,
                RequiresApproval = request.RequiresApproval
            };

            // Add order items
            foreach (var item in request.OrderItems)
            {
                var orderItem = new LabOrderItem
                {
                    LabTestId = item.TestId,
                    Status = "Pending",
                    Priority = item.Priority,
                    SpecialInstructions = item.SpecialInstructions
                };
                order.OrderItems.Add(orderItem);
            }

            _context.LabOrders.Add(order);
            await _context.SaveChangesAsync();

            // Create initial documents
            await CreateOrderDocumentsAsync(order, request.Documents);

            // Send notifications
            await NotifyOrderCreationAsync(order);

            // Audit trail
            await _auditService.LogActivityAsync(
                "LabOrderCreation",
                $"Created lab order: {order.OrderNumber}",
                order);

            return order;
        }

        public async Task<bool> SubmitToLabAsync(int orderId)
        {
            var order = await GetOrderWithDetailsAsync(orderId);
            if (order == null)
                throw new NotFoundException("Lab order not found");

            // Validate order readiness
            await ValidateOrderForSubmissionAsync(order);

            // Determine lab routing
            if (order.ExternalLabId.HasValue)
            {
                // Route to external lab
                var success = await _interfaceEngine.SendOrderAsync(order);
                if (!success)
                    throw new InterfaceException("Failed to send order to external lab");
            }
            else
            {
                // Route to internal lab
                await RouteToInternalLabAsync(order);
            }

            order.Status = "Submitted";
            await _context.SaveChangesAsync();

            // Notify relevant parties
            await NotifyOrderSubmissionAsync(order);

            return true;
        }

        public async Task<LabResult> RecordResultAsync(LabResultRequest request)
        {
            var orderItem = await _context.LabOrderItems
                .Include(o => o.LabOrder)
                .Include(o => o.LabTest)
                .FirstOrDefaultAsync(o => o.Id == request.OrderItemId);

            if (orderItem == null)
                throw new NotFoundException("Lab order item not found");

            var result = new LabResult
            {
                LabOrderId = orderItem.LabOrderId,
                LabOrderItemId = orderItem.Id,
                ResultDate = DateTime.UtcNow,
                Status = "Pending Review",
                PerformingLab = request.PerformingLab,
                PerformingTechnician = request.PerformingTechnician,
                Comments = request.Comments
            };

            // Record result values
            foreach (var value in request.ResultValues)
            {
                var resultValue = new LabResultValue
                {
                    LabTestComponentId = value.ComponentId,
                    Value = value.Value,
                    Units = value.Units
                };

                // Analyze result value
                await AnalyzeResultValueAsync(resultValue, orderItem.LabTest);
                result.ResultValues.Add(resultValue);

                // Check for critical values
                if (resultValue.IsCritical)
                {
                    result.IsCritical = true;
                    await CreateCriticalResultAlertAsync(result, resultValue);
                }
            }

            _context.LabResults.Add(result);
            await _context.SaveChangesAsync();

            // Process result documents
            await ProcessResultDocumentsAsync(result, request.Documents);

            // Update order status
            await UpdateOrderStatusForResultAsync(orderItem.LabOrder);

            // Notify providers
            await NotifyResultAvailabilityAsync(result);

            return result;
        }

        public async Task<bool> ReviewResultAsync(int resultId, string reviewerId)
        {
            var result = await _context.LabResults
                .Include(r => r.LabOrder)
                .Include(r => r.ResultValues)
                .FirstOrDefaultAsync(r => r.Id == resultId);

            if (result == null)
                throw new NotFoundException("Lab result not found");

            result.ReviewedBy = reviewerId;
            result.ReviewDate = DateTime.UtcNow;
            result.Status = "Reviewed";

            // Update clinical record
            await _clinicalService.UpdateLabResultsAsync(result);

            // Generate clinical alerts if needed
            await GenerateClinicalAlertsAsync(result);

            await _context.SaveChangesAsync();

            // Notify ordering provider
            await NotifyResultReviewedAsync(result);

            return true;
        }

        public async Task<LabOrderAlert> CreateOrderAlertAsync(int orderId, AlertRequest request)
        {
            var order = await GetOrderAsync(orderId);
            if (order == null)
                throw new NotFoundException("Lab order not found");

            var alert = new LabOrderAlert
            {
                LabOrderId = orderId,
                AlertType = request.AlertType,
                Severity = request.Severity,
                Message = request.Message
            };

            _context.LabOrderAlerts.Add(alert);
            await _context.SaveChangesAsync();

            // Send notifications based on severity
            await NotifyAlertCreationAsync(alert);

            return alert;
        }

        public async Task<bool> ProcessInterfaceResultAsync(InterfaceResult result)
        {
            // Validate interface result
            await ValidateInterfaceResultAsync(result);

            // Find corresponding order
            var order = await _context.LabOrders
                .FirstOrDefaultAsync(o => o.ExternalOrderId == result.ExternalOrderId);

            if (order == null)
                throw new NotFoundException("Matching lab order not found");

            // Process result
            var resultRequest = MapInterfaceResultToRequest(result);
            await RecordResultAsync(resultRequest);

            // Update interface status
            await _interfaceEngine.AcknowledgeResultAsync(result.MessageId);

            return true;
        }

        private async Task ValidateOrderRequestAsync(LabOrderRequest request)
        {
            // Validate patient
            var patient = await _context.Patients.FindAsync(request.PatientId);
            if (patient == null)
                throw new ValidationException("Invalid patient");

            // Validate provider
            var provider = await _context.Providers.FindAsync(request.ProviderId);
            if (provider == null)
                throw new ValidationException("Invalid provider");

            // Validate tests
            foreach (var item in request.OrderItems)
            {
                var test = await _context.LabTests.FindAsync(item.TestId);
                if (test == null)
                    throw new ValidationException($"Invalid test: {item.TestId}");

                if (!test.IsActive)
                    throw new ValidationException($"Test is inactive: {test.Name}");
            }

            // Validate clinical requirements
            await _clinicalService.ValidateLabOrderAsync(request);
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            var prefix = "LAB";
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var sequence = await _context.LabOrders
                .Where(o => o.OrderNumber.StartsWith($"{prefix}{date}"))
                .CountAsync() + 1;

            return $"{prefix}{date}{sequence:D4}";
        }

        private async Task CreateOrderDocumentsAsync(
            LabOrder order,
            IEnumerable<DocumentRequest> documents)
        {
            foreach (var doc in documents)
            {
                var document = new LabOrderDocument
                {
                    LabOrderId = order.Id,
                    DocumentType = doc.DocumentType,
                    Description = doc.Description
                };

                // Process document content
                document.DocumentPath = await _documentService.StoreDocumentAsync(
                    doc.Content,
                    doc.DocumentType);

                _context.LabOrderDocuments.Add(document);
            }

            await _context.SaveChangesAsync();
        }

        private async Task NotifyOrderCreationAsync(LabOrder order)
        {
            // Notify ordering provider
            await _notificationService.SendProviderNotificationAsync(
                order.ProviderId,
                "Lab Order Created",
                $"Lab order {order.OrderNumber} has been created");

            // Notify lab staff if internal
            if (!order.ExternalLabId.HasValue)
            {
                await _notificationService.SendLabStaffNotificationAsync(
                    "New Lab Order",
                    $"New lab order received: {order.OrderNumber}");
            }
        }

        private async Task ValidateOrderForSubmissionAsync(LabOrder order)
        {
            // Check approval if required
            if (order.RequiresApproval && order.ApprovalStatus != "Approved")
                throw new ValidationException("Order requires approval");

            // Validate insurance if needed
            if (string.IsNullOrEmpty(order.InsuranceAuthorizationNumber))
            {
                var requiresAuth = await _clinicalService.RequiresAuthorizationAsync(order);
                if (requiresAuth)
                    throw new ValidationException("Insurance authorization required");
            }

            // Validate specimen requirements
            foreach (var item in order.OrderItems)
            {
                var test = await _context.LabTests.FindAsync(item.LabTestId);
                if (test.RequiresFasting && !order.IsFasting)
                    throw new ValidationException($"Test {test.Name} requires fasting");
            }
        }

        private async Task RouteToInternalLabAsync(LabOrder order)
        {
            // Determine lab department
            var department = await DetermineDepartmentAsync(order);

            // Create lab work queue entry
            await CreateLabWorkQueueEntryAsync(order, department);

            // Generate lab worksheets
            await GenerateLabWorksheetsAsync(order);
        }

        private async Task AnalyzeResultValueAsync(
            LabResultValue resultValue,
            LabTest test)
        {
            var component = await _context.LabTestComponents
                .FindAsync(resultValue.LabTestComponentId);

            // Check reference ranges
            if (!string.IsNullOrEmpty(component.ReferenceRange))
            {
                resultValue.ReferenceRange = component.ReferenceRange;
                resultValue.IsAbnormal = !IsWithinReferenceRange(
                    resultValue.Value,
                    component.ReferenceRange);
            }

            // Check critical ranges
            if (!string.IsNullOrEmpty(component.CriticalRange))
            {
                resultValue.IsCritical = IsWithinCriticalRange(
                    resultValue.Value,
                    component.CriticalRange);
            }

            // Get previous value for trending
            var previousValue = await GetPreviousResultValueAsync(
                test.Id,
                component.Id);

            if (previousValue != null)
            {
                resultValue.PreviousValue = previousValue.Value;
                resultValue.PreviousValueDate = previousValue.CreatedDate;
                resultValue.TrendIndicator = CalculateTrendIndicator(
                    resultValue.Value,
                    previousValue.Value);
            }
        }

        private async Task CreateCriticalResultAlertAsync(
            LabResult result,
            LabResultValue criticalValue)
        {
            var alert = new LabResultAlert
            {
                LabResultId = result.Id,
                AlertType = "CriticalValue",
                Severity = "High",
                Message = $"Critical value detected: {criticalValue.Value} " +
                         $"{criticalValue.Units} for {criticalValue.LabTestComponent.Name}"
            };

            _context.LabResultAlerts.Add(alert);

            // Send immediate notifications
            await NotifyCriticalResultAsync(result, criticalValue);
        }

        private async Task NotifyCriticalResultAsync(
            LabResult result,
            LabResultValue criticalValue)
        {
            // Notify ordering provider immediately
            await _notificationService.SendUrgentProviderNotificationAsync(
                result.LabOrder.ProviderId,
                "Critical Lab Result",
                $"Critical value detected for order {result.LabOrder.OrderNumber}");

            // Notify lab supervisor
            await _notificationService.SendLabSupervisorNotificationAsync(
                "Critical Result",
                $"Critical value reported for order {result.LabOrder.OrderNumber}");

            // Log critical result notification
            await _auditService.LogActivityAsync(
                "CriticalResultNotification",
                $"Critical result notification sent for order {result.LabOrder.OrderNumber}",
                result);
        }
    }
}
