using System.Threading.Tasks;

namespace EMRNext.Core.Validation
{
    public interface IValidationRule<T>
    {
        Task<ValidationResult> ValidateAsync(T entity, ValidationContext context);
    }

    public class ValidationContext
    {
        public bool IsNew { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }
        public ValidationMode Mode { get; set; }

        public static ValidationContext Create(bool isNew = true, ValidationMode mode = ValidationMode.Full)
        {
            return new ValidationContext
            {
                IsNew = isNew,
                Mode = mode
            };
        }
    }

    public enum ValidationMode
    {
        Basic,      // Basic property validation
        Standard,   // Basic + business rules
        Full        // Standard + complex scenarios
    }
}
