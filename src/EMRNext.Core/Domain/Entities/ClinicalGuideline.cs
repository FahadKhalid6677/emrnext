using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Base;

namespace EMRNext.Core.Domain.Entities
{
    /// <summary>
    /// Represents a comprehensive clinical guideline with advanced metadata and decision support capabilities
    /// </summary>
    public class ClinicalGuideline : BaseEntity
    {
        /// <summary>
        /// Unique identifier for the clinical guideline
        /// </summary>
        public Guid GuidelineId { get; set; }

        /// <summary>
        /// Name of the clinical guideline
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Detailed description of the guideline
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specific medical specialty or domain this guideline applies to
        /// </summary>
        public string MedicalSpecialty { get; set; }

        /// <summary>
        /// Minimum age for guideline applicability
        /// </summary>
        public int AgeMin { get; set; }

        /// <summary>
        /// Maximum age for guideline applicability
        /// </summary>
        public int AgeMax { get; set; }

        /// <summary>
        /// Risk score threshold for triggering this guideline
        /// </summary>
        public double RiskScoreThreshold { get; set; }

        /// <summary>
        /// Specific recommendation associated with the guideline
        /// </summary>
        public string Recommendation { get; set; }

        /// <summary>
        /// Evidence level for the guideline (e.g., A, B, C)
        /// </summary>
        public string EvidenceLevel { get; set; }

        /// <summary>
        /// List of applicable medical conditions
        /// </summary>
        public List<string> ApplicableMedicalConditions { get; set; } = new List<string>();

        /// <summary>
        /// List of contraindications or exclusion criteria
        /// </summary>
        public List<string> ExclusionCriteria { get; set; } = new List<string>();

        /// <summary>
        /// Recommended diagnostic tests or screenings
        /// </summary>
        public List<string> RecommendedTests { get; set; } = new List<string>();

        /// <summary>
        /// Potential treatment options
        /// </summary>
        public List<string> TreatmentOptions { get; set; } = new List<string>();

        /// <summary>
        /// References and source documentation
        /// </summary>
        public List<string> References { get; set; } = new List<string>();

        /// <summary>
        /// Date when the guideline was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Indicates if the guideline is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Machine learning model version used for guideline generation
        /// </summary>
        public string MlModelVersion { get; set; }

        /// <summary>
        /// Validate if the guideline is applicable to a specific patient profile
        /// </summary>
        public bool IsApplicable(PatientHealthProfile profile)
        {
            return IsActive &&
                   profile.Age >= AgeMin &&
                   profile.Age <= AgeMax &&
                   profile.PredictedHealthRisk >= RiskScoreThreshold &&
                   ApplicableMedicalConditions.Contains(profile.PrimaryMedicalCondition);
        }

        /// <summary>
        /// Generate a detailed explanation of the guideline
        /// </summary>
        public string GenerateDetailedExplanation()
        {
            return $"Guideline: {Name}\n" +
                   $"Specialty: {MedicalSpecialty}\n" +
                   $"Age Range: {AgeMin}-{AgeMax}\n" +
                   $"Risk Threshold: {RiskScoreThreshold}\n" +
                   $"Recommendation: {Recommendation}\n" +
                   $"Evidence Level: {EvidenceLevel}";
        }
    }

    /// <summary>
    /// Represents a clinical guideline repository with advanced querying capabilities
    /// </summary>
    public interface IClinicalGuidelineRepository : IGenericRepository<ClinicalGuideline>
    {
        /// <summary>
        /// Find guidelines applicable to a specific patient health profile
        /// </summary>
        Task<IEnumerable<ClinicalGuideline>> FindApplicableGuidelinesAsync(PatientHealthProfile profile);

        /// <summary>
        /// Get guidelines for a specific medical specialty
        /// </summary>
        Task<IEnumerable<ClinicalGuideline>> GetGuidelinesBySpecialtyAsync(string specialty);

        /// <summary>
        /// Get the most recent guidelines with high evidence levels
        /// </summary>
        Task<IEnumerable<ClinicalGuideline>> GetLatestHighEvidenceGuidelinesAsync();
    }
}
