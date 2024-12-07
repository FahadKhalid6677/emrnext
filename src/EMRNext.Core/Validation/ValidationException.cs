using System;
using System.Linq;

namespace EMRNext.Core.Validation
{
    public class ValidationException : Exception
    {
        public ValidationResult ValidationResult { get; }

        public ValidationException(ValidationResult validationResult)
            : base(FormatMessage(validationResult))
        {
            ValidationResult = validationResult;
        }

        private static string FormatMessage(ValidationResult validationResult)
        {
            var errors = string.Join(Environment.NewLine, 
                validationResult.Errors.Select(e => $"{e.PropertyName}: {e.Message}"));
            
            var warnings = string.Join(Environment.NewLine, 
                validationResult.Warnings.Select(w => $"{w.PropertyName}: {w.Message}"));

            return $"Validation failed with the following errors:{Environment.NewLine}{errors}" +
                   (string.IsNullOrEmpty(warnings) ? "" : 
                    $"{Environment.NewLine}Warnings:{Environment.NewLine}{warnings}");
        }
    }
}
