using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EMRNext.Core.Domain.Entities.Interoperability
{
    /// <summary>
    /// Represents a generic FHIR resource with advanced mapping capabilities
    /// </summary>
    public class FhirResource
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ResourceType { get; set; }

        [Required]
        [StringLength(50)]
        public string FhirVersion { get; set; }

        /// <summary>
        /// Raw JSON representation of the FHIR resource
        /// </summary>
        public string RawContent { get; set; }

        /// <summary>
        /// Metadata about the resource mapping
        /// </summary>
        public FhirResourceMetadata Metadata { get; set; }

        /// <summary>
        /// Mapping rules for this specific resource type
        /// </summary>
        public List<FhirMappingRule> MappingRules { get; set; }

        /// <summary>
        /// Converts the raw content to a strongly-typed object
        /// </summary>
        public T DeserializeContent<T>() where T : class
        {
            return string.IsNullOrWhiteSpace(RawContent) 
                ? null 
                : JsonSerializer.Deserialize<T>(RawContent);
        }

        /// <summary>
        /// Validates the FHIR resource against its mapping rules
        /// </summary>
        public bool Validate()
        {
            if (MappingRules == null) return true;

            foreach (var rule in MappingRules)
            {
                if (!rule.Validate(RawContent))
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Metadata for a FHIR resource
    /// </summary>
    public class FhirResourceMetadata
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string SourceSystem { get; set; }
        public string Version { get; set; }
        public ResourceIntegrityStatus IntegrityStatus { get; set; }
    }

    /// <summary>
    /// Represents a mapping rule for FHIR resource transformation
    /// </summary>
    public class FhirMappingRule
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RuleName { get; set; }

        [Required]
        public string SourcePath { get; set; }

        [Required]
        public string TargetPath { get; set; }

        public string TransformationExpression { get; set; }

        /// <summary>
        /// Validates the rule against the resource content
        /// </summary>
        public bool Validate(string resourceContent)
        {
            try
            {
                // Implement complex validation logic
                // This is a placeholder for more advanced validation
                var jsonDocument = JsonDocument.Parse(resourceContent);
                var sourceElement = jsonDocument.RootElement.GetProperty(SourcePath);
                
                return !sourceElement.ValueEquals(JsonValueKind.Null) && 
                       !string.IsNullOrWhiteSpace(sourceElement.GetString());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Transforms the source value based on the rule
        /// </summary>
        public string Transform(string sourceValue)
        {
            // Implement transformation logic
            // This is a placeholder for more complex transformations
            return sourceValue?.Trim();
        }
    }

    /// <summary>
    /// Represents the integrity status of a FHIR resource
    /// </summary>
    public enum ResourceIntegrityStatus
    {
        Unknown = 0,
        Valid = 1,
        Partial = 2,
        Invalid = 3,
        Transformed = 4
    }
}
