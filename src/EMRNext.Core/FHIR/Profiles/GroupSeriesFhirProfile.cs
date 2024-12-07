using AutoMapper;
using EMRNext.Core.Domain.Entities;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMRNext.Core.FHIR.Profiles
{
    public class GroupSeriesFhirProfile : Profile
    {
        public GroupSeriesFhirProfile()
        {
            CreateMap<GroupSeries, Group>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => new CodeableConcept("http://terminology.hl7.org/CodeSystem/group-type", "therapy")))
                .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Status == "Active"))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Actual, opt => opt.UseValue(true))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.NumberOfSessions))
                .ForMember(dest => dest.Characteristic, opt => opt.MapFrom(src => new List<Group.CharacteristicComponent>
                {
                    new Group.CharacteristicComponent
                    {
                        Code = new CodeableConcept("http://snomed.info/sct", "310893008", "Group psychotherapy"),
                        Value = new CodeableConcept("http://snomed.info/sct", "310893008", "Group psychotherapy"),
                        Exclude = false
                    }
                }))
                .ForMember(dest => dest.Member, opt => opt.MapFrom(src => src.Participants.Select(p => new Group.MemberComponent
                {
                    Entity = new ResourceReference($"Patient/{p.PatientId}"),
                    Period = new Period
                    {
                        Start = p.EnrollmentDate,
                        End = p.CompletionDate
                    },
                    Inactive = p.EnrollmentStatus != "Active"
                })));

            CreateMap<Group, GroupSeries>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => int.Parse(src.Id)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Active ? "Active" : "Inactive"))
                .ForMember(dest => dest.NumberOfSessions, opt => opt.MapFrom(src => src.Quantity));
        }
    }
}
