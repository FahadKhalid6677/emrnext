using System;

namespace EMRNext.Core.Domain.Entities
{
    public class SocialHistory
    {
        public int Id { get; set; }
        public int PatientId { get; set; }

        // Smoking Status
        public string SmokingStatus { get; set; }
        public int? PacksPerDay { get; set; }
        public int? YearsSmoked { get; set; }
        public DateTime? QuitDate { get; set; }
        public string SmokingNotes { get; set; }

        // Alcohol Use
        public string AlcoholUse { get; set; }
        public int? DrinksPerWeek { get; set; }
        public string AlcoholPreference { get; set; }
        public DateTime? SobrietyDate { get; set; }
        public string AlcoholNotes { get; set; }

        // Substance Use
        public bool SubstanceUse { get; set; }
        public string SubstanceTypes { get; set; }
        public string SubstanceFrequency { get; set; }
        public DateTime? LastUseDate { get; set; }
        public string SubstanceNotes { get; set; }

        // Exercise
        public string ExerciseFrequency { get; set; }
        public string ExerciseType { get; set; }
        public int? MinutesPerSession { get; set; }
        public string ExerciseNotes { get; set; }

        // Diet
        public string DietType { get; set; }
        public string DietaryRestrictions { get; set; }
        public string CaffeineUse { get; set; }
        public string DietNotes { get; set; }

        // Sexual History
        public string SexuallyActive { get; set; }
        public string SexualOrientation { get; set; }
        public string SexualPartners { get; set; }
        public string SexualPractices { get; set; }
        public string Contraception { get; set; }
        public string SexualRiskFactors { get; set; }

        // Education and Occupation
        public string EducationLevel { get; set; }
        public string SchoolStatus { get; set; }
        public string EmploymentStatus { get; set; }
        public string Occupation { get; set; }
        public string WorkplaceExposures { get; set; }
        public string MilitaryService { get; set; }

        // Living Situation
        public string LivingArrangement { get; set; }
        public string HouseholdMembers { get; set; }
        public string HousingType { get; set; }
        public bool HasCareGiver { get; set; }
        public string CareGiverName { get; set; }
        public string CareGiverRelationship { get; set; }

        // Social Support and Stress
        public string SocialSupport { get; set; }
        public string StressLevel { get; set; }
        public string CopingMechanisms { get; set; }
        public string SpiritualBeliefs { get; set; }
        public string CulturalFactors { get; set; }

        // Safety and Risk Factors
        public bool DomesticViolence { get; set; }
        public bool GunsInHome { get; set; }
        public bool SmokingInHome { get; set; }
        public bool CarbonMonoxideDetector { get; set; }
        public bool SmokeDetector { get; set; }
        public string SafetyNotes { get; set; }

        // Travel History
        public string RecentTravel { get; set; }
        public string TravelDestinations { get; set; }
        public DateTime? TravelReturnDate { get; set; }
        public string TravelNotes { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
    }
}
