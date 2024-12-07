using System.Collections.Generic;
using System.Linq;

namespace EMRNext.Core.Validation
{
    public class ValidationResult
    {
        private readonly List<ValidationError> _errors;

        public ValidationResult()
        {
            _errors = new List<ValidationError>();
        }

        public bool IsValid => !_errors.Any();
        public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

        public void AddError(string propertyName, string message, ValidationSeverity severity = ValidationSeverity.Error)
        {
            _errors.Add(new ValidationError(propertyName, message, severity));
        }

        public void AddErrors(IEnumerable<ValidationError> errors)
        {
            _errors.AddRange(errors);
        }

        public void MergeWith(ValidationResult other)
        {
            if (other != null)
            {
                _errors.AddRange(other.Errors);
            }
        }

        public static ValidationResult Success()
        {
            return new ValidationResult();
        }

        public static ValidationResult Failure(string propertyName, string message, ValidationSeverity severity = ValidationSeverity.Error)
        {
            var result = new ValidationResult();
            result.AddError(propertyName, message, severity);
            return result;
        }
    }

    public class ValidationError
    {
        public string PropertyName { get; }
        public string Message { get; }
        public ValidationSeverity Severity { get; }

        public ValidationError(string propertyName, string message, ValidationSeverity severity)
        {
            PropertyName = propertyName;
            Message = message;
            Severity = severity;
        }
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
}
