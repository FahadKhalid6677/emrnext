using EMRNext.Core.Validation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMRNext.Core.Services
{
    public interface IValidationService
    {
        Task<ValidationResult> ValidateAsync<T>(T entity, ValidationContext context = null);
        Task<ValidationResult> ValidateAsync<T>(IEnumerable<T> entities, ValidationContext context = null);
    }

    public class ValidationService : IValidationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ValidationResult> ValidateAsync<T>(T entity, ValidationContext context = null)
        {
            if (entity == null)
                return ValidationResult.Failure("Entity", "Entity cannot be null");

            context ??= ValidationContext.Create();
            var result = new ValidationResult();

            try
            {
                // Get all validation rules for the entity type
                var rules = _serviceProvider.GetServices<IValidationRule<T>>();
                
                foreach (var rule in rules)
                {
                    var ruleResult = await rule.ValidateAsync(entity, context);
                    result.MergeWith(ruleResult);

                    // If validation mode is not Full and we have errors, stop validation
                    if (context.Mode != ValidationMode.Full && !result.IsValid)
                        break;
                }
            }
            catch (Exception ex)
            {
                result.AddError("Validation", $"Validation failed: {ex.Message}");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateAsync<T>(IEnumerable<T> entities, ValidationContext context = null)
        {
            if (entities == null)
                return ValidationResult.Failure("Entities", "Entities collection cannot be null");

            context ??= ValidationContext.Create();
            var result = new ValidationResult();

            foreach (var entity in entities)
            {
                var entityResult = await ValidateAsync(entity, context);
                result.MergeWith(entityResult);

                // If validation mode is not Full and we have errors, stop validation
                if (context.Mode != ValidationMode.Full && !result.IsValid)
                    break;
            }

            return result;
        }
    }
}
