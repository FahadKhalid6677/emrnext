using System;
using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActivityAsync(string activity, string userId, string details);
        Task LogErrorAsync(Exception ex, string userId = null);
        Task LogSecurityEventAsync(string eventType, string userId, string details);
        Task<bool> ValidateAuditTrailAsync(DateTime startDate, DateTime endDate);
    }
}
