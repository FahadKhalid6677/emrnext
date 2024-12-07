using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EMRNext.Core.Interoperability.Fhir.Models
{
    /// <summary>
    /// Base class for FHIR resource mapping and transformation
    /// </summary>
    public abstract class FhirResourceBase
    {
        /// <summary>
        /// Unique identifier for the FHIR resource
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Resource type for FHIR serialization
        /// </summary>
        [JsonPropertyName("resourceType")]
        public abstract string ResourceType { get; }

        /// <summary>
        /// Convert to native FHIR resource
        /// </summary>
        public abstract Resource ToFhirResource();

        /// <summary>
        /// Create from native FHIR resource
        /// </summary>
        public abstract void FromFhirResource(Resource resource);

        /// <summary>
        /// Serialize resource to JSON
        /// </summary>
        public virtual string SerializeToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Deserialize from JSON
        /// </summary>
        public virtual T DeserializeFromJson<T>(string json) where T : FhirResourceBase
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Validate FHIR resource
        /// </summary>
        public virtual bool Validate()
        {
            // Base validation logic
            return !string.IsNullOrEmpty(Id);
        }
    }

    /// <summary>
    /// FHIR resource mapping extensions
    /// </summary>
    public static class FhirResourceExtensions
    {
        /// <summary>
        /// Convert to FHIR JSON
        /// </summary>
        public static string ToFhirJson(this Resource resource)
        {
            var parser = new FhirJsonSerializer();
            return parser.SerializeToString(resource);
        }

        /// <summary>
        /// Parse from FHIR JSON
        /// </summary>
        public static Resource FromFhirJson(this Resource resource, string json)
        {
            var parser = new FhirJsonParser();
            return parser.Parse<Resource>(json);
        }
    }
}
