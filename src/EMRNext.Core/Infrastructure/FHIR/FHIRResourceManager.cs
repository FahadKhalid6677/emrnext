using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Validation;

namespace EMRNext.Core.Infrastructure.FHIR
{
    public class FHIRResourceManager
    {
        private readonly FhirJsonParser _parser;
        private readonly FhirJsonSerializer _serializer;
        private readonly Validator _validator;

        public FHIRResourceManager()
        {
            _parser = new FhirJsonParser();
            _serializer = new FhirJsonSerializer(new SerializerSettings 
            { 
                Pretty = true 
            });
            _validator = new Validator();
        }

        // Generic Resource Parsing
        public T ParseResource<T>(string jsonResource) where T : Resource
        {
            try
            {
                return _parser.Parse<T>(jsonResource);
            }
            catch (FormatException ex)
            {
                throw new FHIRValidationException($"Invalid FHIR Resource: {ex.Message}", ex);
            }
        }

        // Resource Validation
        public ValidationResult ValidateResource<T>(T resource) where T : Resource
        {
            var validationContext = new ValidationContext();
            return _validator.Validate(resource, validationContext);
        }

        // Resource Transformation Interface
        public interface IFHIRResourceTransformer<TSource, TTarget>
            where TSource : class
            where TTarget : Resource
        {
            TTarget Transform(TSource sourceObject);
        }

        // Patient Resource Transformer
        public class PatientResourceTransformer : 
            IFHIRResourceTransformer<EMRNext.Core.Models.Patient, Hl7.Fhir.Model.Patient>
        {
            public Hl7.Fhir.Model.Patient Transform(EMRNext.Core.Models.Patient sourcePatient)
            {
                return new Hl7.Fhir.Model.Patient
                {
                    Id = sourcePatient.Id.ToString(),
                    Name = new List<HumanName>
                    {
                        new HumanName()
                        .WithGiven(sourcePatient.FirstName)
                        .WithFamily(sourcePatient.LastName)
                    },
                    Identifier = new List<Identifier>
                    {
                        new Identifier(
                            system: "http://emrnext.com/patient", 
                            value: sourcePatient.UniqueIdentifier)
                    },
                    Gender = sourcePatient.Gender == "Male" ? 
                        AdministrativeGender.Male : 
                        AdministrativeGender.Female,
                    BirthDate = sourcePatient.DateOfBirth.ToString("yyyy-MM-dd")
                };
            }
        }

        // FHIR Resource Type Enumeration
        public enum FHIRResourceType
        {
            Patient,
            Encounter,
            Observation,
            Condition,
            Procedure,
            MedicationRequest
        }

        // Resource Metadata Tracking
        public class FHIRResourceMetadata
        {
            public string ResourceId { get; set; }
            public FHIRResourceType ResourceType { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Version { get; set; }
            public Dictionary<string, string> Tags { get; set; }
        }

        // Custom FHIR Validation Exception
        public class FHIRValidationException : Exception
        {
            public List<string> ValidationErrors { get; }

            public FHIRValidationException(string message, Exception innerException) 
                : base(message, innerException)
            {
                ValidationErrors = new List<string>();
            }

            public void AddValidationError(string error)
            {
                ValidationErrors.Add(error);
            }
        }

        // Resource Conversion Utility
        public string ConvertResourceToJson<T>(T resource) where T : Resource
        {
            return _serializer.SerializeToString(resource);
        }

        // Bulk Resource Processing
        public List<FHIRResourceMetadata> ProcessBulkResources<T>(
            IEnumerable<T> resources, 
            IFHIRResourceTransformer<T, Resource> transformer) 
            where T : class
        {
            var metadata = new List<FHIRResourceMetadata>();

            foreach (var resource in resources)
            {
                try
                {
                    var fhirResource = transformer.Transform(resource);
                    var validationResult = ValidateResource(fhirResource);

                    if (validationResult.Success)
                    {
                        metadata.Add(new FHIRResourceMetadata
                        {
                            ResourceId = fhirResource.Id,
                            ResourceType = (FHIRResourceType)Enum.Parse(
                                typeof(FHIRResourceType), 
                                fhirResource.TypeName),
                            CreatedAt = DateTime.UtcNow,
                            Version = fhirResource.VersionId
                        });
                    }
                }
                catch (FHIRValidationException ex)
                {
                    // Log or handle validation errors
                }
            }

            return metadata;
        }
    }
}
