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
    public class WorkQueueService : IWorkQueueService
    {
        private readonly IRepository<WorkQueueTask> _taskRepository;
        private readonly IUserContext _userContext;
        private readonly ILogger<WorkQueueService> _logger;
        private readonly ITaskValidator _validator;
        private readonly IAuditService _auditService;
        private readonly IAlertService _alertService;
        private readonly INotificationService _notificationService;

        public WorkQueueService(
            IRepository<WorkQueueTask> taskRepository,
            IUserContext userContext,
            ILogger<WorkQueueService> logger,
            ITaskValidator validator,
            IAuditService auditService,
            IAlertService alertService,
            INotificationService notificationService)
        {
            _taskRepository = taskRepository;
            _userContext = userContext;
            _logger = logger;
            _validator = validator;
            _auditService = auditService;
            _alertService = alertService;
            _notificationService = notificationService;
        }

        public async Task<WorkQueueTask> CreateTaskAsync(WorkQueueTaskRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new work queue task");

                if (!await _userContext.HasPermissionAsync(Permission.CreateTask))
                {
                    throw new UnauthorizedAccessException("User does not have permission to create tasks");
                }

                var validationResult = await _validator.ValidateTaskRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var task = new WorkQueueTask
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = request.Type,
                    EntityId = request.EntityId,
                    Priority = request.Priority,
                    Status = TaskStatus.Pending,
                    Description = request.Description,
                    AssignedTo = request.AssignedTo,
                    DueDate = request.DueDate,
                    CreatedBy = _userContext.CurrentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _taskRepository.AddAsync(task);

                // Notify assigned user
                await _notificationService.SendNotificationAsync(new NotificationRequest
                {
                    UserId = task.AssignedTo,
                    Type = NotificationType.TaskAssigned,
                    Message = $"New task assigned: {task.Description}",
                    Priority = MapTaskPriorityToNotificationPriority(task.Priority)
                });

                // Create alert for high priority tasks
                if (task.Priority == TaskPriority.High || task.Priority == TaskPriority.Critical)
                {
                    await _alertService.CreateAlertAsync(new AlertRequest
                    {
                        Type = AlertType.HighPriorityTask,
                        EntityId = task.Id,
                        Message = $"High priority task created: {task.Description}",
                        Priority = AlertPriority.High
                    });
                }

                await _auditService.CreateAuditAsync(
                    EntityType.WorkQueueTask,
                    task.Id,
                    AuditAction.Create,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Work queue task {TaskId} created successfully", task.Id);

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work queue task");
                throw;
            }
        }

        public async Task<WorkQueueTask> GetTaskAsync(string id)
        {
            try
            {
                _logger.LogInformation("Retrieving work queue task {TaskId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.ViewTask))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view tasks");
                }

                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    throw new NotFoundException($"Work queue task {id} not found");
                }

                // Check if user has access to this task
                if (!await CanAccessTaskAsync(task))
                {
                    throw new UnauthorizedAccessException("User does not have access to this task");
                }

                await _auditService.CreateAuditAsync(
                    EntityType.WorkQueueTask,
                    id,
                    AuditAction.View,
                    _userContext.CurrentUserId);

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work queue task {TaskId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<WorkQueueTask>> GetTasksAsync(string assignedTo)
        {
            try
            {
                _logger.LogInformation("Retrieving work queue tasks for user {UserId}", assignedTo);

                if (!await _userContext.HasPermissionAsync(Permission.ViewTask))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view tasks");
                }

                // Users can only view their own tasks unless they have admin privileges
                if (assignedTo != _userContext.CurrentUserId && !await _userContext.HasPermissionAsync(Permission.AdminTask))
                {
                    throw new UnauthorizedAccessException("User can only view their own tasks");
                }

                var tasks = await _taskRepository.FindAsync(t => t.AssignedTo == assignedTo && t.Status != TaskStatus.Completed);

                // Sort tasks by priority and due date
                tasks = tasks.OrderByDescending(t => t.Priority)
                            .ThenBy(t => t.DueDate);

                await _auditService.CreateAuditAsync(
                    EntityType.WorkQueueTask,
                    assignedTo,
                    AuditAction.List,
                    _userContext.CurrentUserId);

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work queue tasks for user {UserId}", assignedTo);
                throw;
            }
        }

        public async Task<WorkQueueTask> UpdateTaskAsync(string id, WorkQueueTaskRequest request)
        {
            try
            {
                _logger.LogInformation("Updating work queue task {TaskId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.UpdateTask))
                {
                    throw new UnauthorizedAccessException("User does not have permission to update tasks");
                }

                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    throw new NotFoundException($"Work queue task {id} not found");
                }

                if (!await CanAccessTaskAsync(task))
                {
                    throw new UnauthorizedAccessException("User does not have access to this task");
                }

                var validationResult = await _validator.ValidateTaskRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                // Check if assignee is being changed
                bool isReassignment = task.AssignedTo != request.AssignedTo;

                // Update task details
                task.Priority = request.Priority;
                task.Description = request.Description;
                task.DueDate = request.DueDate;
                task.UpdatedBy = _userContext.CurrentUserId;
                task.UpdatedAt = DateTime.UtcNow;

                if (isReassignment)
                {
                    task.AssignedTo = request.AssignedTo;
                    task.ReassignedBy = _userContext.CurrentUserId;
                    task.ReassignedAt = DateTime.UtcNow;

                    // Notify new assignee
                    await _notificationService.SendNotificationAsync(new NotificationRequest
                    {
                        UserId = task.AssignedTo,
                        Type = NotificationType.TaskReassigned,
                        Message = $"Task reassigned to you: {task.Description}",
                        Priority = MapTaskPriorityToNotificationPriority(task.Priority)
                    });
                }

                await _taskRepository.UpdateAsync(task);

                await _auditService.CreateAuditAsync(
                    EntityType.WorkQueueTask,
                    id,
                    AuditAction.Update,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Work queue task {TaskId} updated successfully", id);

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work queue task {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> CompleteTaskAsync(string id)
        {
            try
            {
                _logger.LogInformation("Completing work queue task {TaskId}", id);

                if (!await _userContext.HasPermissionAsync(Permission.CompleteTask))
                {
                    throw new UnauthorizedAccessException("User does not have permission to complete tasks");
                }

                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    throw new NotFoundException($"Work queue task {id} not found");
                }

                if (!await CanAccessTaskAsync(task))
                {
                    throw new UnauthorizedAccessException("User does not have access to this task");
                }

                task.Status = TaskStatus.Completed;
                task.CompletedBy = _userContext.CurrentUserId;
                task.CompletedAt = DateTime.UtcNow;
                task.UpdatedBy = _userContext.CurrentUserId;
                task.UpdatedAt = DateTime.UtcNow;

                await _taskRepository.UpdateAsync(task);

                await _auditService.CreateAuditAsync(
                    EntityType.WorkQueueTask,
                    id,
                    AuditAction.Complete,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Work queue task {TaskId} completed successfully", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing work queue task {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> ReassignTaskAsync(string id, string newAssignee)
        {
            try
            {
                _logger.LogInformation("Reassigning work queue task {TaskId} to user {UserId}", id, newAssignee);

                if (!await _userContext.HasPermissionAsync(Permission.ReassignTask))
                {
                    throw new UnauthorizedAccessException("User does not have permission to reassign tasks");
                }

                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    throw new NotFoundException($"Work queue task {id} not found");
                }

                if (!await CanAccessTaskAsync(task))
                {
                    throw new UnauthorizedAccessException("User does not have access to this task");
                }

                task.AssignedTo = newAssignee;
                task.ReassignedBy = _userContext.CurrentUserId;
                task.ReassignedAt = DateTime.UtcNow;
                task.UpdatedBy = _userContext.CurrentUserId;
                task.UpdatedAt = DateTime.UtcNow;

                await _taskRepository.UpdateAsync(task);

                // Notify new assignee
                await _notificationService.SendNotificationAsync(new NotificationRequest
                {
                    UserId = newAssignee,
                    Type = NotificationType.TaskReassigned,
                    Message = $"Task reassigned to you: {task.Description}",
                    Priority = MapTaskPriorityToNotificationPriority(task.Priority)
                });

                await _auditService.CreateAuditAsync(
                    EntityType.WorkQueueTask,
                    id,
                    AuditAction.Reassign,
                    _userContext.CurrentUserId);

                _logger.LogInformation("Work queue task {TaskId} reassigned successfully", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reassigning work queue task {TaskId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<WorkQueueTask>> GetPendingTasksAsync(string departmentId)
        {
            try
            {
                _logger.LogInformation("Retrieving pending tasks for department {DepartmentId}", departmentId);

                if (!await _userContext.HasPermissionAsync(Permission.ViewDepartmentTasks))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view department tasks");
                }

                var tasks = await _taskRepository.FindAsync(t => 
                    t.DepartmentId == departmentId && 
                    t.Status == TaskStatus.Pending);

                // Sort tasks by priority and due date
                tasks = tasks.OrderByDescending(t => t.Priority)
                            .ThenBy(t => t.DueDate);

                await _auditService.CreateAuditAsync(
                    EntityType.WorkQueueTask,
                    departmentId,
                    AuditAction.List,
                    _userContext.CurrentUserId);

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending tasks for department {DepartmentId}", departmentId);
                throw;
            }
        }

        public async Task<IEnumerable<WorkQueueTask>> GetOverdueTasksAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving overdue tasks");

                if (!await _userContext.HasPermissionAsync(Permission.ViewOverdueTasks))
                {
                    throw new UnauthorizedAccessException("User does not have permission to view overdue tasks");
                }

                var tasks = await _taskRepository.FindAsync(t => 
                    t.Status != TaskStatus.Completed && 
                    t.DueDate < DateTime.UtcNow);

                // Sort tasks by due date and priority
                tasks = tasks.OrderBy(t => t.DueDate)
                            .ThenByDescending(t => t.Priority);

                await _auditService.CreateAuditAsync(
                    EntityType.WorkQueueTask,
                    "overdue",
                    AuditAction.List,
                    _userContext.CurrentUserId);

                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue tasks");
                throw;
            }
        }

        private async Task<bool> CanAccessTaskAsync(WorkQueueTask task)
        {
            // Users can access tasks if they:
            // 1. Are the assigned user
            // 2. Have admin privileges
            // 3. Are in the same department (if they have department view permission)
            return task.AssignedTo == _userContext.CurrentUserId ||
                   await _userContext.HasPermissionAsync(Permission.AdminTask) ||
                   (await _userContext.HasPermissionAsync(Permission.ViewDepartmentTasks) &&
                    await _userContext.IsInDepartmentAsync(task.DepartmentId));
        }

        private NotificationPriority MapTaskPriorityToNotificationPriority(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Critical => NotificationPriority.Critical,
                TaskPriority.High => NotificationPriority.High,
                TaskPriority.Normal => NotificationPriority.Normal,
                TaskPriority.Low => NotificationPriority.Low,
                _ => NotificationPriority.Normal
            };
        }
    }
}
