using AutoMapper;
using EMRNext.Core.Entities;
using EMRNext.Web.Models.API;

namespace EMRNext.Web.Mapping
{
    public class MedicalMappingProfile : Profile
    {
        public MedicalMappingProfile()
        {
            // Vitals Mapping
            CreateMap<VitalRequest, Vital>();
            CreateMap<Vital, VitalResponse>();

            // Allergies Mapping
            CreateMap<AllergyRequest, Allergy>();
            CreateMap<Allergy, AllergyResponse>();

            // Problems Mapping
            CreateMap<ProblemRequest, Problem>();
            CreateMap<Problem, ProblemResponse>();
        }
    }
}
