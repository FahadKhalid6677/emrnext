using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Core.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly IAuditRepository _auditRepository;
        private readonly ICurrentUserService _currentUserService;

        public LoggingService(
            ILogger<LoggingService> logger,
            IAuditRepository auditRepository,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _auditRepository = auditRepository;
            _currentUserService = currentUserService;
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
        }

        public async Task LogAuditAsync(string action, string entityType, string entityId, string details)
        {
            var audit = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = _currentUserService.IpAddress
            };

            await _auditRepository.AddAsync(audit);
        }

        public async Task LogSecurityEventAsync(string eventType, string details, bool isSuccess)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = eventType,
                Details = details,
                IsSuccess = isSuccess,
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = _currentUserService.IpAddress
            };

            await _auditRepository.AddSecurityEventAsync(securityEvent);
        }

        public void LogHipaaEvent(string action, int patientId, string details)
        {
            var hipaaLog = new
            {
                Action = action,
                PatientId = patientId,
                Details = details,
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Timestamp = DateTime.UtcNow,
                IpAddress = _currentUserService.IpAddress,
                AccessType = "EMR",
                IsAuthorized = true
            };

            _logger.LogInformation("HIPAA Event: {@HipaaLog}", hipaaLog);
        }

        public void LogPerformanceMetric(string operation, long elapsedMilliseconds, string details = null)
        {
            var metric = new
            {
                Operation = operation,
                ElapsedMilliseconds = elapsedMilliseconds,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Performance Metric: {@Metric}", metric);
        }
    }

    public interface ILoggingService
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception ex, string message, params object[] args);
        Task LogAuditAsync(string action, string entityType, string entityId, string details);
        Task LogSecurityEventAsync(string eventType, string details, bool isSuccess);
        void LogHipaaEvent(string action, int patientId, string details);
        void LogPerformanceMetric(string operation, long elapsedMilliseconds, string details = null);
    }
}
