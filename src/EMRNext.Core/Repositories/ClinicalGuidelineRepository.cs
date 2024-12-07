using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure.Persistence;

namespace EMRNext.Core.Repositories
{
    /// <summary>
    /// Advanced repository for managing clinical guidelines with intelligent querying
    /// </summary>
    public class ClinicalGuidelineRepository : GenericRepository<ClinicalGuideline>, IClinicalGuidelineRepository
    {
        private readonly ILogger<ClinicalGuidelineRepository> _logger;

        public ClinicalGuidelineRepository(
            EhrDbContext context, 
            ILogger<ClinicalGuidelineRepository> logger) 
            : base(context)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClinicalGuideline>> FindApplicableGuidelinesAsync(PatientHealthProfile profile)
        {
            try 
            {
                return await _context.Set<ClinicalGuideline>()
                    .Where(g => g.IsActive &&
                                g.AgeMin <= profile.Age &&
                                g.AgeMax >= profile.Age &&
                                g.RiskScoreThreshold <= profile.PredictedHealthRisk &&
                                g.ApplicableMedicalConditions.Contains(profile.PrimaryMedicalCondition))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding applicable guidelines for patient profile");
                return Enumerable.Empty<ClinicalGuideline>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClinicalGuideline>> GetGuidelinesBySpecialtyAsync(string specialty)
        {
            try 
            {
                return await _context.Set<ClinicalGuideline>()
                    .Where(g => g.IsActive && 
                                g.MedicalSpecialty.ToLower() == specialty.ToLower())
                    .OrderByDescending(g => g.LastUpdated)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving guidelines for specialty: {specialty}");
                return Enumerable.Empty<ClinicalGuideline>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClinicalGuideline>> GetLatestHighEvidenceGuidelinesAsync()
        {
            try 
            {
                return await _context.Set<ClinicalGuideline>()
                    .Where(g => g.IsActive && 
                                (g.EvidenceLevel == "A" || g.EvidenceLevel == "B"))
                    .OrderByDescending(g => g.LastUpdated)
                    .Take(50)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest high-evidence guidelines");
                return Enumerable.Empty<ClinicalGuideline>();
            }
        }

        /// <summary>
        /// Seed initial clinical guidelines for the system
        /// </summary>
        public async Task SeedInitialGuidelinesAsync()
        {
            if (!await _context.Set<ClinicalGuideline>().AnyAsync())
            {
                var initialGuidelines = new List<ClinicalGuideline>
                {
                    new ClinicalGuideline
                    {
                        GuidelineId = Guid.NewGuid(),
                        Name = "Diabetes Management",
                        Description = "Comprehensive guideline for diabetes care",
                        MedicalSpecialty = "Endocrinology",
                        AgeMin = 18,
                        AgeMax = 80,
                        RiskScoreThreshold = 0.5,
                        Recommendation = "Annual comprehensive metabolic panel and HbA1c screening",
                        EvidenceLevel = "A",
                        ApplicableMedicalConditions = new List<string> { "Diabetes", "Prediabetes" },
                        RecommendedTests = new List<string> { "HbA1c", "Fasting Glucose", "Lipid Panel" },
                        LastUpdated = DateTime.UtcNow,
                        IsActive = true,
                        MlModelVersion = "1.2.0"
                    },
                    // Add more initial guidelines...
                };

                await _context.Set<ClinicalGuideline>().AddRangeAsync(initialGuidelines);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Initial clinical guidelines seeded successfully");
            }
        }
    }
}
