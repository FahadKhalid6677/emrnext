using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Interoperability;

namespace EMRNext.Core.Services.Interoperability
{
    /// <summary>
    /// Service for transforming and mapping FHIR resources
    /// </summary>
    public interface IFhirTransformationService
    {
        /// <summary>
        /// Transform a FHIR resource from one version to another
        /// </summary>
        Task<FhirResource> TransformResourceAsync(
            FhirResource sourceResource, 
            string targetVersion);

        /// <summary>
        /// Map a FHIR resource to internal domain model
        /// </summary>
        Task<object> MapToInternalModelAsync(
            FhirResource fhirResource, 
            string targetModelType);

        /// <summary>
        /// Validate a FHIR resource against its mapping rules
        /// </summary>
        Task<IEnumerable<string>> ValidateResourceAsync(FhirResource resource);

        /// <summary>
        /// Create mapping rules for a specific resource type
        /// </summary>
        Task<List<FhirMappingRule>> CreateMappingRulesAsync(
            string resourceType, 
            string sourceSystem);

        /// <summary>
        /// Detect and resolve potential mapping conflicts
        /// </summary>
        Task<IEnumerable<string>> DetectMappingConflictsAsync(
            FhirResource resource);
    }
}
