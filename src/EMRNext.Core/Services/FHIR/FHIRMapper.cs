using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.FHIR
{
    public class FHIRMapper : IFHIRMapper
    {
        public Patient MapToFHIRPatient(Domain.Entities.Patient patient)
        {
            var fhirPatient = new Patient
            {
                Id = patient.Id.ToString(),
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        System = "urn:oid:2.16.840.1.113883.4.6",
                        Value = patient.MedicalRecordNumber
                    }
                },
                Active = !patient.IsDeleted,
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Family = patient.LastName,
                        Given = new[] { patient.FirstName, patient.MiddleName }.Where(n => !string.IsNullOrEmpty(n)).ToList()
                    }
                },
                BirthDate = patient.DateOfBirth.ToString("yyyy-MM-dd"),
                Gender = MapGender(patient.Gender),
                Address = new List<Address>
                {
                    new Address
                    {
                        Line = new[] { patient.Address1, patient.Address2 }.Where(a => !string.IsNullOrEmpty(a)).ToList(),
                        City = patient.City,
                        State = patient.State,
                        PostalCode = patient.PostalCode,
                        Country = patient.Country
                    }
                },
                Telecom = new List<ContactPoint>
                {
                    new ContactPoint { System = ContactPoint.ContactPointSystem.Phone, Value = patient.Phone },
                    new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = patient.Email }
                }
            };

            return fhirPatient;
        }

        public Domain.Entities.Patient MapFromFHIRPatient(Patient fhirPatient)
        {
            var name = fhirPatient.Name.FirstOrDefault();
            var address = fhirPatient.Address.FirstOrDefault();
            var phone = fhirPatient.Telecom?.FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Phone)?.Value;
            var email = fhirPatient.Telecom?.FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Email)?.Value;

            return new Domain.Entities.Patient
            {
                Id = int.Parse(fhirPatient.Id),
                MedicalRecordNumber = fhirPatient.Identifier.FirstOrDefault()?.Value,
                FirstName = name?.Given.FirstOrDefault(),
                MiddleName = name?.Given.Skip(1).FirstOrDefault(),
                LastName = name?.Family,
                DateOfBirth = DateTime.Parse(fhirPatient.BirthDate),
                Gender = MapGender(fhirPatient.Gender),
                Address1 = address?.Line.FirstOrDefault(),
                Address2 = address?.Line.Skip(1).FirstOrDefault(),
                City = address?.City,
                State = address?.State,
                PostalCode = address?.PostalCode,
                Country = address?.Country,
                Phone = phone,
                Email = email,
                IsDeleted = !fhirPatient.Active.GetValueOrDefault(true)
            };
        }

        public Encounter MapToFHIREncounter(Domain.Entities.Encounter encounter)
        {
            var fhirEncounter = new Encounter
            {
                Id = encounter.Id.ToString(),
                Status = MapEncounterStatus(encounter.Status),
                Class = new Coding
                {
                    System = "http://terminology.hl7.org/CodeSystem/v3-ActCode",
                    Code = encounter.ClassCode,
                    Display = encounter.Type
                },
                Subject = new ResourceReference($"Patient/{encounter.PatientId}"),
                Participant = new List<Encounter.ParticipantComponent>
                {
                    new Encounter.ParticipantComponent
                    {
                        Individual = new ResourceReference($"Practitioner/{encounter.ProviderId}")
                    }
                },
                Period = new Period
                {
                    Start = encounter.Date,
                    End = encounter.DischargeDate
                },
                ReasonCode = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Text = encounter.Reason
                    }
                },
                ServiceProvider = new ResourceReference($"Organization/{encounter.FacilityId}")
            };

            if (encounter.SupervisorId.HasValue)
            {
                fhirEncounter.Participant.Add(new Encounter.ParticipantComponent
                {
                    Type = new List<CodeableConcept>
                    {
                        new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    System = "http://terminology.hl7.org/CodeSystem/v3-ParticipationType",
                                    Code = "ATND",
                                    Display = "Supervisor"
                                }
                            }
                        }
                    },
                    Individual = new ResourceReference($"Practitioner/{encounter.SupervisorId}")
                });
            }

            return fhirEncounter;
        }

        public Domain.Entities.Encounter MapFromFHIREncounter(Encounter fhirEncounter)
        {
            return new Domain.Entities.Encounter
            {
                Id = int.Parse(fhirEncounter.Id),
                Status = MapEncounterStatus(fhirEncounter.Status),
                ClassCode = fhirEncounter.Class.Code,
                Type = fhirEncounter.Class.Display,
                PatientId = int.Parse(fhirEncounter.Subject.Reference.Split('/').Last()),
                ProviderId = int.Parse(fhirEncounter.Participant.First().Individual.Reference.Split('/').Last()),
                SupervisorId = fhirEncounter.Participant
                    .FirstOrDefault(p => p.Type?.Any(t => t.Coding.Any(c => c.Code == "ATND")) ?? false)
                    ?.Individual.Reference.Split('/').Last()
                    .Let(id => int.Parse(id)),
                Date = fhirEncounter.Period.Start ?? DateTime.UtcNow,
                DischargeDate = fhirEncounter.Period.End,
                Reason = fhirEncounter.ReasonCode.FirstOrDefault()?.Text,
                FacilityId = int.Parse(fhirEncounter.ServiceProvider.Reference.Split('/').Last())
            };
        }

        public Observation MapToFHIRObservation(Vital vital)
        {
            var observation = new Observation
            {
                Id = vital.Id.ToString(),
                Status = ObservationStatus.Final,
                Category = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://terminology.hl7.org/CodeSystem/observation-category",
                                Code = "vital-signs",
                                Display = "Vital Signs"
                            }
                        }
                    }
                },
                Subject = new ResourceReference($"Patient/{vital.PatientId}"),
                Encounter = new ResourceReference($"Encounter/{vital.EncounterId}"),
                EffectiveDateTime = vital.Date
            };

            // Add vital sign components
            var components = new List<Observation.ComponentComponent>();

            if (vital.Temperature.HasValue)
            {
                components.Add(CreateVitalComponent(
                    "8310-5",
                    "Body temperature",
                    vital.Temperature.Value,
                    vital.TemperatureUnit));
            }

            if (vital.Pulse.HasValue)
            {
                components.Add(CreateVitalComponent(
                    "8867-4",
                    "Heart rate",
                    vital.Pulse.Value,
                    "beats/min"));
            }

            if (vital.RespiratoryRate.HasValue)
            {
                components.Add(CreateVitalComponent(
                    "9279-1",
                    "Respiratory rate",
                    vital.RespiratoryRate.Value,
                    "breaths/min"));
            }

            if (vital.BloodPressureSystolic.HasValue && vital.BloodPressureDiastolic.HasValue)
            {
                components.Add(CreateVitalComponent(
                    "8480-6",
                    "Systolic blood pressure",
                    vital.BloodPressureSystolic.Value,
                    "mmHg"));

                components.Add(CreateVitalComponent(
                    "8462-4",
                    "Diastolic blood pressure",
                    vital.BloodPressureDiastolic.Value,
                    "mmHg"));
            }

            if (vital.OxygenSaturation.HasValue)
            {
                components.Add(CreateVitalComponent(
                    "2708-6",
                    "Oxygen saturation",
                    vital.OxygenSaturation.Value,
                    "%"));
            }

            observation.Component = components;
            return observation;
        }

        public Vital MapFromFHIRObservation(Observation fhirObservation)
        {
            var vital = new Vital
            {
                Id = int.Parse(fhirObservation.Id),
                PatientId = int.Parse(fhirObservation.Subject.Reference.Split('/').Last()),
                EncounterId = int.Parse(fhirObservation.Encounter.Reference.Split('/').Last()),
                Date = fhirObservation.EffectiveDateTime.Value
            };

            foreach (var component in fhirObservation.Component)
            {
                var code = component.Code.Coding.First().Code;
                var value = component.Value as Quantity;

                switch (code)
                {
                    case "8310-5": // Temperature
                        vital.Temperature = (decimal)value.Value;
                        vital.TemperatureUnit = value.Unit;
                        break;
                    case "8867-4": // Heart rate
                        vital.Pulse = (decimal)value.Value;
                        break;
                    case "9279-1": // Respiratory rate
                        vital.RespiratoryRate = (decimal)value.Value;
                        break;
                    case "8480-6": // Systolic BP
                        vital.BloodPressureSystolic = (decimal)value.Value;
                        break;
                    case "8462-4": // Diastolic BP
                        vital.BloodPressureDiastolic = (decimal)value.Value;
                        break;
                    case "2708-6": // Oxygen saturation
                        vital.OxygenSaturation = (decimal)value.Value;
                        break;
                }
            }

            return vital;
        }

        // Helper methods
        private AdministrativeGender MapGender(string gender)
        {
            return gender?.ToLower() switch
            {
                "male" => AdministrativeGender.Male,
                "female" => AdministrativeGender.Female,
                "other" => AdministrativeGender.Other,
                _ => AdministrativeGender.Unknown
            };
        }

        private string MapGender(AdministrativeGender gender)
        {
            return gender switch
            {
                AdministrativeGender.Male => "male",
                AdministrativeGender.Female => "female",
                AdministrativeGender.Other => "other",
                _ => "unknown"
            };
        }

        private Encounter.EncounterStatus MapEncounterStatus(string status)
        {
            return status?.ToLower() switch
            {
                "planned" => Encounter.EncounterStatus.Planned,
                "arrived" => Encounter.EncounterStatus.Arrived,
                "triaged" => Encounter.EncounterStatus.Triaged,
                "in-progress" => Encounter.EncounterStatus.InProgress,
                "onleave" => Encounter.EncounterStatus.OnLeave,
                "finished" => Encounter.EncounterStatus.Finished,
                "cancelled" => Encounter.EncounterStatus.Cancelled,
                _ => Encounter.EncounterStatus.Unknown
            };
        }

        private string MapEncounterStatus(Encounter.EncounterStatus status)
        {
            return status switch
            {
                Encounter.EncounterStatus.Planned => "planned",
                Encounter.EncounterStatus.Arrived => "arrived",
                Encounter.EncounterStatus.Triaged => "triaged",
                Encounter.EncounterStatus.InProgress => "in-progress",
                Encounter.EncounterStatus.OnLeave => "onleave",
                Encounter.EncounterStatus.Finished => "finished",
                Encounter.EncounterStatus.Cancelled => "cancelled",
                _ => "unknown"
            };
        }

        private Observation.ComponentComponent CreateVitalComponent(string code, string display, decimal value, string unit)
        {
            return new Observation.ComponentComponent
            {
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://loinc.org",
                            Code = code,
                            Display = display
                        }
                    }
                },
                Value = new Quantity
                {
                    Value = (decimal?)value,
                    Unit = unit,
                    System = "http://unitsofmeasure.org"
                }
            };
        }

        public Bundle CreateFHIRBundle(string type, IEnumerable<Resource> resources)
        {
            var bundle = new Bundle
            {
                Type = type switch
                {
                    "search" => Bundle.BundleType.SearchSet,
                    "transaction" => Bundle.BundleType.Transaction,
                    "history" => Bundle.BundleType.History,
                    "document" => Bundle.BundleType.Document,
                    _ => Bundle.BundleType.Collection
                },
                Entry = resources.Select(resource => new Bundle.EntryComponent { Resource = resource }).ToList()
            };

            return bundle;
        }

        // TODO: Implement remaining interface methods for DocumentReference, MedicationRequest, 
        // ServiceRequest, and DiagnosticReport mappings
        public DocumentReference MapToFHIRDocumentReference(ClinicalNote note)
        {
            throw new NotImplementedException();
        }

        public ClinicalNote MapFromFHIRDocumentReference(DocumentReference fhirDocument)
        {
            throw new NotImplementedException();
        }

        public MedicationRequest MapToFHIRMedicationRequest(Prescription prescription)
        {
            throw new NotImplementedException();
        }

        public Prescription MapFromFHIRMedicationRequest(MedicationRequest fhirMedRequest)
        {
            throw new NotImplementedException();
        }

        public ServiceRequest MapToFHIRServiceRequest(Order order)
        {
            throw new NotImplementedException();
        }

        public Order MapFromFHIRServiceRequest(ServiceRequest fhirServiceRequest)
        {
            throw new NotImplementedException();
        }

        public DiagnosticReport MapToFHIRDiagnosticReport(Result result)
        {
            throw new NotImplementedException();
        }

        public Result MapFromFHIRDiagnosticReport(DiagnosticReport fhirDiagReport)
        {
            throw new NotImplementedException();
        }
    }
}
