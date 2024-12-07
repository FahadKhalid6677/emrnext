using AutoMapper;
using EMRNext.Core.Domain.Entities;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMRNext.Core.FHIR.Profiles
{
    public class GroupSessionFhirProfile : Profile
    {
        public GroupSessionFhirProfile()
        {
            CreateMap<GroupAppointment, Encounter>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatus(src.Status)))
                .ForMember(dest => dest.Class, opt => opt.MapFrom(src => new Coding
                {
                    System = "http://terminology.hl7.org/CodeSystem/v3-ActCode",
                    Code = "GRP",
                    Display = "Group Encounter"
                }))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://snomed.info/sct",
                                Code = "310893008",
                                Display = "Group psychotherapy"
                            }
                        }
                    }
                }))
                .ForMember(dest => dest.ServiceType, opt => opt.MapFrom(src => new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            System = "http://snomed.info/sct",
                            Code = "310893008",
                            Display = "Group psychotherapy"
                        }
                    }
                }))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => 
                    new ResourceReference($"Group/{src.GroupSeriesId}")))
                .ForMember(dest => dest.Participant, opt => opt.MapFrom(src => 
                    MapParticipants(src)))
                .ForMember(dest => dest.Period, opt => opt.MapFrom(src => new Period
                {
                    Start = src.StartTime,
                    End = src.EndTime
                }))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => new List<Encounter.LocationComponent>
                {
                    new Encounter.LocationComponent
                    {
                        Location = new ResourceReference($"Location/{src.LocationId}"),
                        Status = Encounter.EncounterLocationStatus.Planned
                    }
                }))
                .ForMember(dest => dest.ServiceProvider, opt => opt.MapFrom(src => 
                    new ResourceReference($"Organization/{src.FacilityId}")));

            CreateMap<Encounter, GroupAppointment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => int.Parse(src.Id)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatusBack(src.Status)))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Period.Start))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Period.End))
                .ForMember(dest => dest.GroupSeriesId, opt => opt.MapFrom(src => 
                    int.Parse(src.Subject.Reference.Split('/')[1])))
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => 
                    int.Parse(src.Location.First().Location.Reference.Split('/')[1])))
                .ForMember(dest => dest.FacilityId, opt => opt.MapFrom(src => 
                    int.Parse(src.ServiceProvider.Reference.Split('/')[1])));
        }

        private static Encounter.EncounterStatus MapStatus(string status)
        {
            return status?.ToLower() switch
            {
                "scheduled" => Encounter.EncounterStatus.Planned,
                "in progress" => Encounter.EncounterStatus.InProgress,
                "completed" => Encounter.EncounterStatus.Finished,
                "cancelled" => Encounter.EncounterStatus.Cancelled,
                _ => Encounter.EncounterStatus.Unknown
            };
        }

        private static string MapStatusBack(Encounter.EncounterStatus status)
        {
            return status switch
            {
                Encounter.EncounterStatus.Planned => "Scheduled",
                Encounter.EncounterStatus.InProgress => "In Progress",
                Encounter.EncounterStatus.Finished => "Completed",
                Encounter.EncounterStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        private static List<Encounter.ParticipantComponent> MapParticipants(GroupAppointment session)
        {
            var participants = new List<Encounter.ParticipantComponent>();

            // Add provider
            if (session.ProviderId != null)
            {
                participants.Add(new Encounter.ParticipantComponent
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
                                    Code = "PPRF",
                                    Display = "Primary Performer"
                                }
                            }
                        }
                    },
                    Individual = new ResourceReference($"Practitioner/{session.ProviderId}")
                });
            }

            // Add participants
            foreach (var participant in session.Participants ?? Enumerable.Empty<SessionParticipant>())
            {
                participants.Add(new Encounter.ParticipantComponent
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
                                    Code = "PTNT",
                                    Display = "Patient"
                                }
                            }
                        }
                    },
                    Individual = new ResourceReference($"Patient/{participant.PatientId}")
                });
            }

            return participants;
        }
    }
}
