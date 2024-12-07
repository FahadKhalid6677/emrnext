using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using EMRNext.Core.Models;
using EMRNext.Core.Infrastructure.FHIR;

namespace EMRNext.Core.Services.FHIR
{
    public class FHIRConversionService
    {
        private readonly FHIRResourceManager _resourceManager;

        public FHIRConversionService(FHIRResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        // Convert EMRNext Patient to FHIR Patient
        public Hl7.Fhir.Model.Patient ConvertToFHIRPatient(Patient patient)
        {
            var fhirPatient = new Hl7.Fhir.Model.Patient
            {
                Id = patient.Id.ToString(),
                Name = new List<HumanName>
                {
                    new HumanName()
                        .WithGiven(patient.FirstName)
                        .WithFamily(patient.LastName)
                },
                Identifier = new List<Identifier>
                {
                    new Identifier(
                        system: "http://emrnext.com/patient", 
                        value: patient.UniqueIdentifier)
                },
                Gender = patient.Gender.ToLower() == "male" ? 
                    AdministrativeGender.Male : 
                    AdministrativeGender.Female,
                BirthDate = patient.DateOfBirth.ToString("yyyy-MM-dd")
            };

            // Add contact information
            if (!string.IsNullOrEmpty(patient.ContactNumber))
            {
                fhirPatient.Telecom = new List<ContactPoint>
                {
                    new ContactPoint
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = patient.ContactNumber
                    }
                };
            }

            return fhirPatient;
        }

        // Convert FHIR Patient to EMRNext Patient
        public Patient ConvertFromFHIRPatient(Hl7.Fhir.Model.Patient fhirPatient)
        {
            return new Patient
            {
                Id = !string.IsNullOrEmpty(fhirPatient.Id) 
                    ? Guid.Parse(fhirPatient.Id) 
                    : Guid.NewGuid(),
                FirstName = fhirPatient.Name.FirstOrDefault()?.Given.FirstOrDefault(),
                LastName = fhirPatient.Name.FirstOrDefault()?.Family,
                UniqueIdentifier = fhirPatient.Identifier
                    .FirstOrDefault(i => i.System == "http://emrnext.com/patient")?.Value,
                Gender = fhirPatient.Gender.HasValue 
                    ? fhirPatient.Gender.Value.ToString() 
                    : null,
                DateOfBirth = fhirPatient.BirthDate != null 
                    ? DateTime.Parse(fhirPatient.BirthDate) 
                    : DateTime.MinValue,
                ContactNumber = fhirPatient.Telecom
                    .FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Phone)?.Value
            };
        }

        // Bulk Conversion Methods
        public List<Hl7.Fhir.Model.Patient> ConvertPatientsToFHIR(IEnumerable<Patient> patients)
        {
            return patients.Select(ConvertToFHIRPatient).ToList();
        }

        public List<Patient> ConvertPatientsFromFHIR(IEnumerable<Hl7.Fhir.Model.Patient> fhirPatients)
        {
            return fhirPatients.Select(ConvertFromFHIRPatient).ToList();
        }

        // Observation Conversion
        public Observation ConvertToFHIRObservation(VitalSign vitalSign)
        {
            return new Observation
            {
                Status = ObservationStatus.Final,
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://loinc.org",
                            Code = GetLoincCode(vitalSign.Type),
                            Display = vitalSign.Type
                        }
                    }
                },
                Subject = new ResourceReference
                {
                    Reference = $"Patient/{vitalSign.PatientId}"
                },
                Effective = new FhirDateTime(vitalSign.RecordedAt),
                Value = new Quantity
                {
                    Value = decimal.Parse(vitalSign.Value),
                    Unit = vitalSign.Unit
                }
            };
        }

        // Helper method to get LOINC codes
        private string GetLoincCode(string vitalSignType)
        {
            return vitalSignType.ToLower() switch
            {
                "temperature" => "8310-5",
                "blood pressure" => "55284-4",
                "heart rate" => "8867-4",
                "respiratory rate" => "9279-1",
                "oxygen saturation" => "59408-5",
                _ => "unknown"
            };
        }

        // Validation Wrapper
        public ValidationResult ValidateFHIRResource<T>(T resource) where T : Resource
        {
            return _resourceManager.ValidateResource(resource);
        }
    }
}
