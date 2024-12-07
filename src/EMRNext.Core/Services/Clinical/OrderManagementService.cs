using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;
using EMRNext.Core.Repositories;
using EMRNext.Core.Security;
using EMRNext.Core.Validation;
using EMRNext.Core.Exceptions;

namespace EMRNext.Core.Services.Clinical
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderSet> _orderSetRepository;
        private readonly IRepository<OrderResult> _resultRepository;
        private readonly IUserContext _userContext;
        private readonly ILogger<OrderManagementService> _logger;
        private readonly IOrderValidator _validator;
        private readonly IAuditService _auditService;
        private readonly IAlertService _alertService;
        private readonly IWorkQueueService _workQueueService;

        public OrderManagementService(
            IRepository<Order> orderRepository,
            IRepository<OrderSet> orderSetRepository,
            IRepository<OrderResult> resultRepository,
            IUserContext userContext,
            ILogger<OrderManagementService> logger,
            IOrderValidator validator,
            IAuditService auditService,
            IAlertService alertService,
            IWorkQueueService workQueueService)
        {
            _orderRepository = orderRepository;
            _orderSetRepository = orderSetRepository;
            _resultRepository = resultRepository;
            _userContext = userContext;
            _logger = logger;
            _validator = validator;
            _auditService = auditService;
            _alertService = alertService;
            _workQueueService = workQueueService;
        }

        public async Task<Order> CreateOrderAsync(OrderRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new order for patient {PatientId}", request.PatientId);

                if (!await _userContext.HasPermissionAsync(Permission.CreateOrder))
                {
                    throw new UnauthorizedAccessException("User does not have permission to create orders");
                }

                var validationResult = await _validator.ValidateOrderRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    PatientId = request.PatientId,
                    OrderType = request.OrderType,
                    OrderSetId = request.OrderSetId,
                    Priority = request.Priority,
                    Status = OrderStatus.Pending,
                    OrderedBy = _userContext.CurrentUserId,
                    OrderedAt = DateTime.UtcNow,
                    DueDate = CalculateDueDate(request.Priority),
                    Instructions = request.Instructions,
                    Diagnosis = request.Diagnosis,
                    CreatedBy = _userContext.CurrentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _orderRepository.AddAsync(order);

                // Create work queue task for the order
                await CreateOrderTask(order);

                // Generate alerts based on priority
                if (order.Priority == OrderPriority.Stat || order.Priority == OrderPriority.Urgent)
                {
                    await _alertService.CreateAlertAsync(new AlertRequest
                    {
                        Type = AlertType.HighPriorityOrder,
                        EntityId = order.Id,
                        Message = $"High priority {order.OrderType} order created for patient {order.PatientId}",
                        Priority = AlertPriority.High
                    });
                }

                await _auditService.CreateAuditAsync(
                    EntityType.Order,
                    order.Id,
                    AuditAction.Create,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Order {OrderId} created successfully", order.Id);

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for patient {PatientId}", request.PatientId);
                throw;
            }
        }

        public async Task<Order> GetOrderAsync(string id)
        {
            try
            {
                _logger.LogInformation("Retrieving order {OrderId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.ViewOrder))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view orders");
                }

                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    throw new NotFoundException($"Order {id} not found");
                }

                if (!await _userContext.CanAccessPatientDataAsync(order.PatientId))
                {
                    throw new UnauthorizedAccessException("User does not have access to this patient's orders");
                }

                // Load results
                var results = await _resultRepository.FindAsync(r => r.OrderId == id);
                order.Results = results.ToList();

                await _auditService.CreateAuditAsync(
                    EntityType.Order,
                    id,
                    AuditAction.View,
                    _userContext.CurrentUserId);

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetPatientOrdersAsync(string patientId)
        {
            try
            {
                _logger.LogInformation("Retrieving orders for patient {PatientId}", patientId);

                if (!await _userContext.HasPermissionAsync(Permission.ViewOrder))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view orders");
                }

                if (!await _userContext.CanAccessPatientDataAsync(patientId))
                {
                    throw new UnauthorizedAccessException("User does not have access to this patient's orders");
                }

                var orders = await _orderRepository.FindAsync(o => o.PatientId == patientId);

                // Load results for each order
                foreach (var order in orders)
                {
                    var results = await _resultRepository.FindAsync(r => r.OrderId == order.Id);
                    order.Results = results.ToList();
                }

                await _auditService.CreateAuditAsync(
                    EntityType.Order,
                    patientId,
                    AuditAction.List,
                    _userContext.CurrentUserId);

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<Order> UpdateOrderAsync(string id, OrderRequest request)
        {
            try
            {
                _logger.LogInformation("Updating order {OrderId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.UpdateOrder))
                {
                    throw new UnauthorizedAccessException("User does not have permission to update orders");
                }

                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    throw new NotFoundException($"Order {id} not found");
                }

                if (!await _userContext.CanAccessPatientDataAsync(order.PatientId))
                {
                    throw new UnauthorizedAccessException("User does not have access to this patient's orders");
                }

                var validationResult = await _validator.ValidateOrderRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                // Update order details
                order.Priority = request.Priority;
                order.Instructions = request.Instructions;
                order.Diagnosis = request.Diagnosis;
                order.UpdatedBy = _userContext.CurrentUserId;
                order.UpdatedAt = DateTime.UtcNow;
                order.DueDate = CalculateDueDate(request.Priority);

                await _orderRepository.UpdateAsync(order);

                // Update work queue task
                await UpdateOrderTask(order);

                await _auditService.CreateAuditAsync(
                    EntityType.Order,
                    id,
                    AuditAction.Update,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Order {OrderId} updated successfully", id);

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(string id, string reason)
        {
            try
            {
                _logger.LogInformation("Cancelling order {OrderId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.CancelOrder))
                {
                    throw new UnauthorizedAccessException("User does not have permission to cancel orders");
                }

                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    throw new NotFoundException($"Order {id} not found");
                }

                if (!await _userContext.CanAccessPatientDataAsync(order.PatientId))
                {
                    throw new UnauthorizedAccessException("User does not have access to this patient's orders");
                }

                order.Status = OrderStatus.Cancelled;
                order.CancelReason = reason;
                order.CancelledBy = _userContext.CurrentUserId;
                order.CancelledAt = DateTime.UtcNow;
                order.UpdatedBy = _userContext.CurrentUserId;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);

                // Cancel associated work queue task
                await CancelOrderTask(order);

                await _auditService.CreateAuditAsync(
                    EntityType.Order,
                    id,
                    AuditAction.Cancel,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Order {OrderId} cancelled successfully", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderResult> AddResultAsync(string orderId, OrderResultRequest request)
        {
            try
            {
                _logger.LogInformation("Adding result to order {OrderId}", orderId);

                if (!await _userContext.HasPermissionAsync(Permission.AddOrderResult))
                {
                    throw new UnauthorizedAccessException("User does not have permission to add order results");
                }

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new NotFoundException($"Order {orderId} not found");
                }

                var result = new OrderResult
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = orderId,
                    Value = request.Value,
                    Unit = request.Unit,
                    ReferenceRange = request.ReferenceRange,
                    Status = request.Status,
                    ResultedBy = _userContext.CurrentUserId,
                    ResultedAt = DateTime.UtcNow,
                    CreatedBy = _userContext.CurrentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _resultRepository.AddAsync(result);

                // Update order status
                order.Status = OrderStatus.Completed;
                order.CompletedBy = _userContext.CurrentUserId;
                order.CompletedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);

                // Complete work queue task
                await CompleteOrderTask(order);

                // Generate alerts for abnormal results
                if (request.Status == ResultStatus.Abnormal || request.Status == ResultStatus.Critical)
                {
                    await _alertService.CreateAlertAsync(new AlertRequest
                    {
                        Type = AlertType.AbnormalResult,
                        EntityId = result.Id,
                        Message = $"Abnormal result received for order {orderId}",
                        Priority = request.Status == ResultStatus.Critical ? AlertPriority.High : AlertPriority.Medium
                    });
                }

                await _auditService.CreateAuditAsync(
                    EntityType.OrderResult,
                    result.Id,
                    AuditAction.Create,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Result added to order {OrderId} successfully", orderId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding result to order {OrderId}", orderId);
                throw;
            }
        }

        private DateTime CalculateDueDate(OrderPriority priority)
        {
            return priority switch
            {
                OrderPriority.Stat => DateTime.UtcNow.AddHours(1),
                OrderPriority.Urgent => DateTime.UtcNow.AddHours(4),
                OrderPriority.Routine => DateTime.UtcNow.AddHours(24),
                _ => DateTime.UtcNow.AddHours(24)
            };
        }

        private async Task CreateOrderTask(Order order)
        {
            await _workQueueService.CreateTaskAsync(new WorkQueueTaskRequest
            {
                Type = TaskType.Order,
                EntityId = order.Id,
                Priority = MapOrderPriorityToTaskPriority(order.Priority),
                DueDate = order.DueDate,
                AssignedTo = await GetAppropriateAssignee(order),
                Description = $"{order.OrderType} order for patient {order.PatientId}"
            });
        }

        private async Task UpdateOrderTask(Order order)
        {
            var tasks = await _workQueueService.GetTasksAsync(t => t.EntityId == order.Id && t.Status != TaskStatus.Completed);
            foreach (var task in tasks)
            {
                await _workQueueService.UpdateTaskAsync(task.Id, new WorkQueueTaskRequest
                {
                    Priority = MapOrderPriorityToTaskPriority(order.Priority),
                    DueDate = order.DueDate
                });
            }
        }

        private async Task CancelOrderTask(Order order)
        {
            var tasks = await _workQueueService.GetTasksAsync(t => t.EntityId == order.Id && t.Status != TaskStatus.Completed);
            foreach (var task in tasks)
            {
                await _workQueueService.CompleteTaskAsync(task.Id, TaskCompletionStatus.Cancelled);
            }
        }

        private async Task CompleteOrderTask(Order order)
        {
            var tasks = await _workQueueService.GetTasksAsync(t => t.EntityId == order.Id && t.Status != TaskStatus.Completed);
            foreach (var task in tasks)
            {
                await _workQueueService.CompleteTaskAsync(task.Id, TaskCompletionStatus.Completed);
            }
        }

        private TaskPriority MapOrderPriorityToTaskPriority(OrderPriority priority)
        {
            return priority switch
            {
                OrderPriority.Stat => TaskPriority.Critical,
                OrderPriority.Urgent => TaskPriority.High,
                OrderPriority.Routine => TaskPriority.Normal,
                _ => TaskPriority.Normal
            };
        }

        private async Task<string> GetAppropriateAssignee(Order order)
        {
            // Implementation would depend on business rules for assignment
            // This could involve checking department schedules, workload balancing, etc.
            return "default_assignee"; // Placeholder
        }
    }
}
