using System;
using System.Threading.Tasks;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMRNext.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly EMRNextDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AuditService(EMRNextDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task LogActivityAsync(string activity, string details = null)
        {
            var auditLog = new AuditLog
            {
                UserId = _currentUserService.UserId,
                Activity = activity,
                Details = details,
                Timestamp = DateTime.UtcNow,
                Type = "Activity"
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogErrorAsync(string error, string stackTrace = null)
        {
            var auditLog = new AuditLog
            {
                UserId = _currentUserService.UserId,
                Activity = "Error",
                Details = error,
                AdditionalData = stackTrace,
                Timestamp = DateTime.UtcNow,
                Type = "Error"
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogSecurityEventAsync(string eventType, string details = null)
        {
            var auditLog = new AuditLog
            {
                UserId = _currentUserService.UserId,
                Activity = eventType,
                Details = details,
                Timestamp = DateTime.UtcNow,
                Type = "Security"
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<AuditLog[]> GetUserAuditTrailAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogs.Where(log => log.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(log => log.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(log => log.Timestamp <= endDate.Value);

            return await query.OrderByDescending(log => log.Timestamp).ToArrayAsync();
        }

        public async Task<AuditLog[]> GetSecurityAuditTrailAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogs.Where(log => log.Type == "Security");

            if (startDate.HasValue)
                query = query.Where(log => log.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(log => log.Timestamp <= endDate.Value);

            return await query.OrderByDescending(log => log.Timestamp).ToArrayAsync();
        }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Activity { get; set; }
        public string Details { get; set; }
        public string AdditionalData { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
    }
}
