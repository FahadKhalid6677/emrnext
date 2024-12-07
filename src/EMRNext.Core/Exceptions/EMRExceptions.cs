using System;
using System.Collections.Generic;

namespace EMRNext.Core.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }

    public class ValidationException : Exception
    {
        public IEnumerable<string> Errors { get; }

        public ValidationException(IEnumerable<string> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }

    public class DuplicatePatientException : Exception
    {
        public DuplicatePatientException(string message) : base(message)
        {
        }
    }

    public class AuthorizationException : Exception
    {
        public AuthorizationException(string message) : base(message)
        {
        }
    }

    public class BusinessRuleException : Exception
    {
        public string RuleId { get; }

        public BusinessRuleException(string message, string ruleId = null) 
            : base(message)
        {
            RuleId = ruleId;
        }
    }

    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message)
        {
        }
    }

    public class IntegrationException : Exception
    {
        public string ServiceName { get; }

        public IntegrationException(string message, string serviceName) 
            : base(message)
        {
            ServiceName = serviceName;
        }
    }

    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message)
        {
        }
    }

    public class WorkflowException : Exception
    {
        public string WorkflowStep { get; }
        public string CurrentStatus { get; }

        public WorkflowException(string message, string workflowStep, string currentStatus) 
            : base(message)
        {
            WorkflowStep = workflowStep;
            CurrentStatus = currentStatus;
        }
    }

    public class AuditException : Exception
    {
        public string EntityType { get; }
        public string EntityId { get; }

        public AuditException(string message, string entityType, string entityId) 
            : base(message)
        {
            EntityType = entityType;
            EntityId = entityId;
        }
    }
}
