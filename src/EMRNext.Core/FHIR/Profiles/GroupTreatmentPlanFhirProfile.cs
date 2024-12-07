using AutoMapper;
using EMRNext.Core.Domain.Entities;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMRNext.Core.FHIR.Profiles
{
    public class GroupTreatmentPlanFhirProfile : Profile
    {
        public GroupTreatmentPlanFhirProfile()
        {
            CreateMap<GroupSessionTemplate, CarePlan>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Status, opt => opt.UseValue(CarePlan.CarePlanStatus.Active))
                .ForMember(dest => dest.Intent, opt => opt.UseValue(CarePlan.CarePlanIntent.Plan))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => new List<CodeableConcept>
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
                .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => MapActivities(src)))
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => 
                    new ResourceReference($"Practitioner/{src.DefaultProviderId}")))
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src => new List<Annotation>
                {
                    new Annotation
                    {
                        Text = src.ClinicalProtocol
                    }
                }));
        }

        private static List<CarePlan.ActivityComponent> MapActivities(GroupSessionTemplate template)
        {
            var activities = new List<CarePlan.ActivityComponent>();

            // Add session activity
            activities.Add(new CarePlan.ActivityComponent
            {
                Detail = new CarePlan.DetailComponent
                {
                    Kind = CarePlan.CarePlanActivityKind.ServiceRequest,
                    Code = new CodeableConcept
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
                    },
                    Status = CarePlan.CarePlanActivityStatus.Scheduled,
                    Description = "Group therapy session",
                    Extension = new List<Extension>
                    {
                        new Extension
                        {
                            Url = "http://emrnext.com/fhir/StructureDefinition/group-session-duration",
                            Value = new Duration
                            {
                                Value = template.DefaultDurationMinutes,
                                Unit = "min",
                                System = "http://unitsofmeasure.org",
                                Code = "min"
                            }
                        }
                    }
                }
            });

            // Add material activities
            foreach (var material in template.MaterialTemplates ?? Enumerable.Empty<SessionMaterialTemplate>())
            {
                activities.Add(new CarePlan.ActivityComponent
                {
                    Detail = new CarePlan.DetailComponent
                    {
                        Kind = CarePlan.CarePlanActivityKind.Other,
                        Code = new CodeableConcept
                        {
                            Text = material.Name
                        },
                        Status = CarePlan.CarePlanActivityStatus.Scheduled,
                        Description = material.Description,
                        Extension = new List<Extension>
                        {
                            new Extension
                            {
                                Url = "http://emrnext.com/fhir/StructureDefinition/material-content-type",
                                Value = new FhirString(material.ContentType)
                            }
                        }
                    }
                });
            }

            // Add outcome measure activities
            foreach (var measure in template.OutcomeMeasures ?? Enumerable.Empty<OutcomeMeasureTemplate>())
            {
                activities.Add(new CarePlan.ActivityComponent
                {
                    Detail = new CarePlan.DetailComponent
                    {
                        Kind = CarePlan.CarePlanActivityKind.Observation,
                        Code = new CodeableConcept
                        {
                            Text = measure.Name
                        },
                        Status = CarePlan.CarePlanActivityStatus.Scheduled,
                        Description = measure.Description,
                        Extension = new List<Extension>
                        {
                            new Extension
                            {
                                Url = "http://emrnext.com/fhir/StructureDefinition/outcome-measure-frequency",
                                Value = new FhirString(measure.CollectionFrequency)
                            }
                        }
                    }
                });
            }

            // Add follow-up activities
            foreach (var followUp in template.FollowUpProtocols ?? Enumerable.Empty<FollowUpProtocol>())
            {
                activities.Add(new CarePlan.ActivityComponent
                {
                    Detail = new CarePlan.DetailComponent
                    {
                        Kind = CarePlan.CarePlanActivityKind.Task,
                        Code = new CodeableConcept
                        {
                            Text = followUp.Name
                        },
                        Status = CarePlan.CarePlanActivityStatus.NotStarted,
                        Description = followUp.Description,
                        ScheduledTiming = new Timing
                        {
                            Repeat = new Timing.RepeatComponent
                            {
                                Duration = followUp.TimeframeInDays,
                                DurationUnit = Timing.UnitsOfTime.D
                            }
                        }
                    }
                });
            }

            return activities;
        }
    }
}
