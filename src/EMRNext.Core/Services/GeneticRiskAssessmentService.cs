using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;

namespace EMRNext.Core.Services
{
    /// <summary>
    /// Service for assessing genetic risks and predispositions
    /// </summary>
    public class GeneticRiskAssessmentService
    {
        private readonly ILogger<GeneticRiskAssessmentService> _logger;
        private readonly IGenericRepository<Patient> _patientRepository;
        private readonly IGenericRepository<FamilyMedicalHistory> _familyHistoryRepository;
        private readonly IGenericRepository<GeneticRiskProgressTracking> _progressTrackingRepository;

        // Predefined risk factors and their weights
        private static readonly Dictionary<string, double> DiseaseRiskFactors = new Dictionary<string, double>
        {
            { "Diabetes", 0.3 },
            { "HeartDisease", 0.4 },
            { "Alzheimers", 0.2 },
            { "Breast Cancer", 0.5 },
            { "Colorectal Cancer", 0.35 }
        };

        public GeneticRiskAssessmentService(
            ILogger<GeneticRiskAssessmentService> logger,
            IGenericRepository<Patient> patientRepository,
            IGenericRepository<FamilyMedicalHistory> familyHistoryRepository,
            IGenericRepository<GeneticRiskProgressTracking> progressTrackingRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _familyHistoryRepository = familyHistoryRepository ?? throw new ArgumentNullException(nameof(familyHistoryRepository));
            _progressTrackingRepository = progressTrackingRepository ?? throw new ArgumentNullException(nameof(progressTrackingRepository));
        }

        /// <summary>
        /// Assess genetic risk for a patient
        /// </summary>
        public async Task<Patient.MedicalRiskProfile> AssessGeneticRiskAsync(Guid patientId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                throw new ArgumentException("Patient not found", nameof(patientId));

            var familyHistory = await GetFamilyMedicalHistoryAsync(patientId);

            return CalculateRiskProfile(patient, familyHistory);
        }

        /// <summary>
        /// Get family medical history for a patient
        /// </summary>
        private async Task<IEnumerable<FamilyMedicalHistory>> GetFamilyMedicalHistoryAsync(Guid patientId)
        {
            return await _familyHistoryRepository.FindAsync(
                history => history.PatientId == patientId
            );
        }

        /// <summary>
        /// Calculate comprehensive risk profile
        /// </summary>
        private Patient.MedicalRiskProfile CalculateRiskProfile(
            Patient patient, 
            IEnumerable<FamilyMedicalHistory> familyHistory)
        {
            var riskProfile = new Patient.MedicalRiskProfile
            {
                GeneticPredispositions = new List<string>()
            };

            // Calculate base risks from patient demographics
            riskProfile.DiabetesRisk = CalculateDemographicRisk(patient, "Diabetes");
            riskProfile.CardiovascularRisk = CalculateDemographicRisk(patient, "HeartDisease");
            riskProfile.CancerRisk = CalculateDemographicRisk(patient, "Cancer");

            // Adjust risks based on family history
            foreach (var condition in DiseaseRiskFactors.Keys)
            {
                var familyRisk = CalculateFamilyHistoryRisk(familyHistory, condition);
                riskProfile = AdjustRiskBasedOnFamilyHistory(riskProfile, condition, familyRisk);
            }

            return riskProfile;
        }

        /// <summary>
        /// Calculate base risk from patient demographics
        /// </summary>
        private double CalculateDemographicRisk(Patient patient, string condition)
        {
            double baseRisk = 0.1; // Default base risk

            // Age-based risk adjustment
            int age = patient.CalculateAge();
            if (age > 50)
                baseRisk *= 1.5;

            // Gender-based risk adjustment
            if (condition == "Breast Cancer" && patient.Gender == "Female")
                baseRisk *= 1.7;

            // Lifestyle factors
            if (patient.Medications?.Any() == true)
                baseRisk *= 1.2;

            return Math.Min(baseRisk, 1.0);
        }

        /// <summary>
        /// Calculate risk based on family medical history
        /// </summary>
        private double CalculateFamilyHistoryRisk(
            IEnumerable<FamilyMedicalHistory> familyHistory, 
            string condition)
        {
            var relevantHistory = familyHistory
                .Where(h => h.Condition.Contains(condition))
                .ToList();

            if (!relevantHistory.Any())
                return 0.0;

            // Count close relatives with the condition
            int closeRelativesCount = relevantHistory
                .Count(h => h.Relationship == "Parent" || h.Relationship == "Sibling");

            // Calculate risk based on number of close relatives
            double familyRisk = closeRelativesCount * 0.25;
            return Math.Min(familyRisk, 1.0);
        }

        /// <summary>
        /// Adjust risk profile based on family history
        /// </summary>
        private Patient.MedicalRiskProfile AdjustRiskBasedOnFamilyHistory(
            Patient.MedicalRiskProfile riskProfile, 
            string condition, 
            double familyRisk)
        {
            switch (condition)
            {
                case "Diabetes":
                    riskProfile.DiabetesRisk = Math.Min(riskProfile.DiabetesRisk + familyRisk, 1.0);
                    break;
                case "HeartDisease":
                    riskProfile.CardiovascularRisk = Math.Min(riskProfile.CardiovascularRisk + familyRisk, 1.0);
                    break;
                case "Breast Cancer":
                case "Colorectal Cancer":
                    riskProfile.CancerRisk = Math.Min(riskProfile.CancerRisk + familyRisk, 1.0);
                    break;
            }

            // Add genetic predispositions
            if (familyRisk > 0.5)
                riskProfile.GeneticPredispositions.Add(condition);

            return riskProfile;
        }

        /// <summary>
        /// Generate personalized risk mitigation recommendations
        /// </summary>
        public async Task<IEnumerable<string>> GenerateRiskMitigationRecommendationsAsync(Guid patientId)
        {
            var riskProfile = await AssessGeneticRiskAsync(patientId);
            var recommendations = new List<string>();

            if (riskProfile.DiabetesRisk > 0.5)
                recommendations.Add("Consider diabetes screening and lifestyle modifications");

            if (riskProfile.CardiovascularRisk > 0.5)
                recommendations.Add("Prioritize cardiovascular health checkups and heart-healthy diet");

            if (riskProfile.CancerRisk > 0.5)
                recommendations.Add("Schedule regular cancer screenings based on genetic predispositions");

            return recommendations;
        }

        /// <summary>
        /// Track genetic risk assessment progress over time
        /// </summary>
        public async Task<GeneticRiskProgressTracking> TrackGeneticRiskProgressAsync(Guid patientId)
        {
            // Get previous tracking records
            var previousTrackings = await _progressTrackingRepository
                .FindAsync(t => t.PatientId == patientId)
                .OrderByDescending(t => t.AssessmentDate)
                .ToListAsync();

            // Perform current risk assessment
            var currentRiskProfile = await AssessGeneticRiskAsync(patientId);
            var recommendations = await GenerateRiskMitigationRecommendationsAsync(patientId);

            // Create progress tracking record
            var progressTracking = new GeneticRiskProgressTracking
            {
                PatientId = patientId,
                AssessmentDate = DateTime.UtcNow,
                RiskProfile = currentRiskProfile,
                RiskSeverity = currentRiskProfile.GetOverallRiskSeverity(),
                Recommendations = recommendations.ToList()
            };

            // Calculate risk factor changes if previous records exist
            if (previousTrackings.Any())
            {
                var latestPreviousTracking = previousTrackings.First();
                progressTracking.RiskFactorTrends = new RiskFactorChanges
                {
                    DiabetesRiskChange = currentRiskProfile.DiabetesRisk - 
                        (latestPreviousTracking.RiskProfile?.DiabetesRisk ?? 0),
                    CardiovascularRiskChange = currentRiskProfile.CardiovascularRisk - 
                        (latestPreviousTracking.RiskProfile?.CardiovascularRisk ?? 0),
                    CancerRiskChange = currentRiskProfile.CancerRisk - 
                        (latestPreviousTracking.RiskProfile?.CancerRisk ?? 0)
                };
            }

            // Track interventions from previous recommendations
            progressTracking.Interventions = await TrackInterventionsAsync(patientId, recommendations);

            // Save the progress tracking record
            await _progressTrackingRepository.AddAsync(progressTracking);

            return progressTracking;
        }

        /// <summary>
        /// Track status of previous risk mitigation recommendations
        /// </summary>
        private async Task<List<RiskInterventionProgress>> TrackInterventionsAsync(
            Guid patientId, 
            IEnumerable<string> currentRecommendations)
        {
            // Get previous tracking records
            var previousTrackings = await _progressTrackingRepository
                .FindAsync(t => t.PatientId == patientId)
                .OrderByDescending(t => t.AssessmentDate)
                .ToListAsync();

            var interventions = new List<RiskInterventionProgress>();

            // If no previous trackings, create initial recommendations
            if (!previousTrackings.Any())
            {
                interventions = currentRecommendations.Select(rec => new RiskInterventionProgress
                {
                    InterventionDescription = rec,
                    RecommendedDate = DateTime.UtcNow,
                    Status = InterventionStatus.Recommended
                }).ToList();

                return interventions;
            }

            // Get interventions from the latest tracking
            var latestTracking = previousTrackings.First();
            var previousInterventions = latestTracking.Interventions ?? new List<RiskInterventionProgress>();

            // Update existing interventions and add new ones
            foreach (var recommendation in currentRecommendations)
            {
                var existingIntervention = previousInterventions
                    .FirstOrDefault(i => i.InterventionDescription == recommendation);

                if (existingIntervention != null)
                {
                    // Update existing intervention status
                    interventions.Add(UpdateInterventionStatus(existingIntervention));
                }
                else
                {
                    // Create new intervention
                    interventions.Add(new RiskInterventionProgress
                    {
                        InterventionDescription = recommendation,
                        RecommendedDate = DateTime.UtcNow,
                        Status = InterventionStatus.Recommended
                    });
                }
            }

            return interventions;
        }

        /// <summary>
        /// Update intervention status based on previous tracking
        /// </summary>
        private RiskInterventionProgress UpdateInterventionStatus(RiskInterventionProgress existingIntervention)
        {
            // Simple logic to progress intervention status
            return existingIntervention.Status switch
            {
                InterventionStatus.Recommended => 
                    new RiskInterventionProgress
                    {
                        InterventionDescription = existingIntervention.InterventionDescription,
                        RecommendedDate = existingIntervention.RecommendedDate,
                        InitiatedDate = DateTime.UtcNow,
                        Status = InterventionStatus.Initiated
                    },
                InterventionStatus.Initiated => 
                    new RiskInterventionProgress
                    {
                        InterventionDescription = existingIntervention.InterventionDescription,
                        RecommendedDate = existingIntervention.RecommendedDate,
                        InitiatedDate = existingIntervention.InitiatedDate,
                        Status = InterventionStatus.InProgress
                    },
                _ => existingIntervention
            };
        }
    }

    /// <summary>
    /// Represents family medical history for genetic risk assessment
    /// </summary>
    public class FamilyMedicalHistory : BaseIntEntity
    {
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }
        
        public string Relationship { get; set; }
        public string Condition { get; set; }
        public int? AgeOfOnset { get; set; }
        public bool StillLiving { get; set; }
    }
}
