using AutoMapper;
using EMRNext.Core.Domain.Entities;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;

namespace EMRNext.Core.FHIR.Profiles
{
    public class SessionOutcomeFhirProfile : Profile
    {
        public SessionOutcomeFhirProfile()
        {
            CreateMap<SeriesOutcome, Observation>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Status, opt => opt.UseValue(ObservationStatus.Final))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://terminology.hl7.org/CodeSystem/observation-category",
                                Code = "therapy",
                                Display = "Therapy"
                            }
                        }
                    }
                }))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://snomed.info/sct",
                            Code = MapOutcomeTypeToCode(src.OutcomeType),
                            Display = src.OutcomeType
                        }
                    }
                }))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => 
                    new ResourceReference($"Group/{src.GroupSeriesId}")))
                .ForMember(dest => dest.Effective, opt => opt.MapFrom(src => 
                    new FhirDateTime(src.MeasurementDate)))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => 
                    new FhirString(src.Value)))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => new List<Annotation>
                {
                    new Annotation
                    {
                        Text = src.Notes
                    }
                }));

            CreateMap<ParticipantOutcome, Observation>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Status, opt => opt.UseValue(ObservationStatus.Final))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://terminology.hl7.org/CodeSystem/observation-category",
                                Code = "therapy",
                                Display = "Therapy"
                            }
                        }
                    }
                }))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://snomed.info/sct",
                            Code = MapOutcomeTypeToCode(src.OutcomeType),
                            Display = src.OutcomeType
                        }
                    }
                }))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => 
                    new ResourceReference($"Patient/{src.Participant.PatientId}")))
                .ForMember(dest => dest.Effective, opt => opt.MapFrom(src => 
                    new FhirDateTime(src.MeasurementDate)))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => 
                    new FhirString(src.Value)))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => new List<Annotation>
                {
                    new Annotation
                    {
                        Text = src.Notes
                    }
                }));
        }

        private static string MapOutcomeTypeToCode(string outcomeType)
        {
            // Map outcome types to SNOMED CT codes
            return outcomeType?.ToLower() switch
            {
                "progress" => "278383001", // Progress report finding
                "attendance" => "308273005", // Attendance finding
                "participation" => "364645004", // Participation finding
                "goal achievement" => "386452003", // Goal achievement finding
                _ => "404684003" // Clinical finding
            };
        }
    }
}
