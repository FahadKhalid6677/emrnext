using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Clinical
{
    public interface IWorkQueueService
    {
        Task<WorkQueueTask> CreateTaskAsync(WorkQueueTaskRequest request);
        Task<WorkQueueTask> GetTaskAsync(string id);
        Task<IEnumerable<WorkQueueTask>> GetTasksAsync(string assignedTo);
        Task<WorkQueueTask> UpdateTaskAsync(string id, WorkQueueTaskRequest request);
        Task<bool> CompleteTaskAsync(string id);
        Task<bool> ReassignTaskAsync(string id, string newAssignee);
        Task<IEnumerable<WorkQueueTask>> GetPendingTasksAsync(string departmentId);
        Task<IEnumerable<WorkQueueTask>> GetOverdueTasksAsync();
    }
}
