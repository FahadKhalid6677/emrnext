using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class MedicationRepository : BaseRepository<Medication>, IMedicationRepository
    {
        public MedicationRepository(EMRDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Medication>> GetPatientMedicationsAsync(int patientId)
        {
            return await _dbSet
                .Where(m => m.PatientId == patientId && !m.IsDeleted)
                .OrderByDescending(m => m.StartDate)
                .Include(m => m.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetActiveMedicationsAsync(int patientId)
        {
            var currentDate = DateTime.UtcNow;
            return await _dbSet
                .Where(m => 
                    m.PatientId == patientId && 
                    !m.IsDeleted &&
                    m.StartDate <= currentDate &&
                    (!m.EndDate.HasValue || m.EndDate.Value >= currentDate))
                .OrderBy(m => m.Name)
                .Include(m => m.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> SearchMedicationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Where(m => 
                    m.Name.Contains(searchTerm) ||
                    m.GenericName.Contains(searchTerm) ||
                    m.NDCCode.Contains(searchTerm) ||
                    m.RxNorm.Contains(searchTerm))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetMedicationsByProviderAsync(int providerId)
        {
            return await _dbSet
                .Where(m => m.ProviderId == providerId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Include(m => m.Patient)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetMedicationsNeedingRefillAsync(int patientId, int daysThreshold = 7)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            return await _dbSet
                .Where(m => 
                    m.PatientId == patientId && 
                    !m.IsDeleted &&
                    m.NextRefillDate.HasValue &&
                    m.NextRefillDate.Value <= thresholdDate)
                .OrderBy(m => m.NextRefillDate)
                .Include(m => m.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetDiscontinuedMedicationsAsync(int patientId)
        {
            return await _dbSet
                .Where(m => 
                    m.PatientId == patientId && 
                    !m.IsDeleted &&
                    !string.IsNullOrEmpty(m.DiscontinuationReason))
                .OrderByDescending(m => m.DiscontinuedDate)
                .Include(m => m.Provider)
                .ToListAsync();
        }
    }
}
