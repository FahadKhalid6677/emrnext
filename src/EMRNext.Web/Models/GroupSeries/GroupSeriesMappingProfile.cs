using AutoMapper;
using EMRNext.Core.Domain.Entities.Portal;

namespace EMRNext.Web.Models.GroupSeries
{
    public class GroupSeriesMappingProfile : Profile
    {
        public GroupSeriesMappingProfile()
        {
            CreateMap<GroupSeries, GroupSeriesDto>().ReverseMap();
            
            CreateMap<GroupAppointment, GroupSessionDto>()
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.Participants));

            CreateMap<GroupParticipant, ParticipantDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => $"{src.Patient.FirstName} {src.Patient.LastName}"));

            CreateMap<SeriesOutcome, SeriesOutcomeDto>();

            CreateMap<ParticipantReport, ParticipantReportDto>()
                .ForMember(dest => dest.Outcomes, opt => opt.Ignore());
        }
    }
}
