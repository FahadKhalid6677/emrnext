using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMRNext.Core.Services
{
    public interface IFhirValidationService
    {
        Task<(bool IsValid, IEnumerable<string> Errors)> ValidateResourceAsync(Resource resource);
        Task<(bool IsValid, IEnumerable<string> Errors)> ValidateGroupAsync(Group group);
    }

    public class FhirValidationService : IFhirValidationService
    {
        private readonly IResourceResolver _resolver;
        private readonly Validator _validator;

        public FhirValidationService(string profilePath = null)
        {
            // Initialize the validation infrastructure
            var source = new CachedResolver(
                new MultiResolver(
                    new DirectorySource(profilePath ?? "Profiles"),
                    ZipSource.CreateValidationSource()
                )
            );

            _resolver = source;
            _validator = new Validator(new ValidationSettings
            {
                ResourceResolver = _resolver,
                GenerateSnapshot = true,
                EnableXsdValidation = true,
                Trace = false
            });
        }

        public async Task<(bool IsValid, IEnumerable<string> Errors)> ValidateResourceAsync(Resource resource)
        {
            try
            {
                var result = await Task.Run(() => _validator.Validate(resource));
                var errors = result.Where(issue => issue.Severity == OperationOutcome.IssueSeverity.Error)
                                 .Select(issue => issue.Details?.Text ?? issue.ToString());

                return (!errors.Any(), errors);
            }
            catch (Exception ex)
            {
                return (false, new[] { $"Validation error: {ex.Message}" });
            }
        }

        public async Task<(bool IsValid, IEnumerable<string> Errors)> ValidateGroupAsync(Group group)
        {
            var errors = new List<string>();

            // Basic validation
            if (string.IsNullOrEmpty(group.Id))
                errors.Add("Group ID is required");

            if (string.IsNullOrEmpty(group.Name))
                errors.Add("Group name is required");

            if (group.Type == null)
                errors.Add("Group type is required");

            // Validate characteristics
            if (group.Characteristic != null)
            {
                foreach (var characteristic in group.Characteristic)
                {
                    if (characteristic.Code == null)
                        errors.Add("Characteristic code is required");
                    if (characteristic.Value == null)
                        errors.Add("Characteristic value is required");
                }
            }

            // Validate members
            if (group.Member != null)
            {
                foreach (var member in group.Member)
                {
                    if (member.Entity == null)
                        errors.Add("Member entity reference is required");
                }
            }

            // Perform FHIR specification validation
            var (isValid, specErrors) = await ValidateResourceAsync(group);
            errors.AddRange(specErrors);

            return (!errors.Any(), errors);
        }
    }
}
