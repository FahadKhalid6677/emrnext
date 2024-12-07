using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class CarePlanRepository : BaseRepository<CarePlan>, ICarePlanRepository
    {
        private readonly DbSet<CarePlanActivity> _activities;

        public CarePlanRepository(EMRDbContext context) : base(context)
        {
            _activities = context.Set<CarePlanActivity>();
        }

        public async Task<IEnumerable<CarePlan>> GetPatientCarePlansAsync(int patientId)
        {
            return await _dbSet
                .Where(cp => cp.PatientId == patientId && !cp.IsDeleted)
                .OrderByDescending(cp => cp.StartDate)
                .Include(cp => cp.Provider)
                .Include(cp => cp.Activities)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarePlan>> GetActiveCarePlansAsync(int patientId)
        {
            var currentDate = DateTime.UtcNow;
            return await _dbSet
                .Where(cp => 
                    cp.PatientId == patientId && 
                    !cp.IsDeleted &&
                    cp.StartDate <= currentDate &&
                    (!cp.EndDate.HasValue || cp.EndDate.Value >= currentDate))
                .OrderBy(cp => cp.Title)
                .Include(cp => cp.Provider)
                .Include(cp => cp.Activities)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarePlanActivity>> GetCarePlanActivitiesAsync(int carePlanId)
        {
            return await _activities
                .Where(cpa => cpa.CarePlanId == carePlanId && !cpa.IsDeleted)
                .OrderBy(cpa => cpa.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarePlanActivity>> GetPendingActivitiesAsync(int carePlanId)
        {
            var currentDate = DateTime.UtcNow;
            return await _activities
                .Where(cpa => 
                    cpa.CarePlanId == carePlanId && 
                    !cpa.IsDeleted &&
                    cpa.StartDate <= currentDate &&
                    (!cpa.EndDate.HasValue || cpa.EndDate.Value >= currentDate) &&
                    cpa.CompletionStatus != "Completed")
                .OrderBy(cpa => cpa.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarePlan>> GetCarePlansByProviderAsync(int providerId)
        {
            return await _dbSet
                .Where(cp => cp.ProviderId == providerId && !cp.IsDeleted)
                .OrderByDescending(cp => cp.StartDate)
                .Include(cp => cp.Patient)
                .Include(cp => cp.Activities)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarePlan>> SearchCarePlansAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Where(cp => 
                    cp.Title.Contains(searchTerm) ||
                    cp.Description.Contains(searchTerm) ||
                    cp.Category.Contains(searchTerm) ||
                    cp.PrimaryDiagnosis.Contains(searchTerm))
                .OrderByDescending(cp => cp.StartDate)
                .Include(cp => cp.Provider)
                .Include(cp => cp.Patient)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarePlan>> GetCarePlansNeedingReviewAsync(int daysThreshold = 7)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            return await _dbSet
                .Where(cp => 
                    !cp.IsDeleted &&
                    cp.NextReviewDate.HasValue &&
                    cp.NextReviewDate.Value <= thresholdDate)
                .OrderBy(cp => cp.NextReviewDate)
                .Include(cp => cp.Provider)
                .Include(cp => cp.Patient)
                .ToListAsync();
        }
    }
}
