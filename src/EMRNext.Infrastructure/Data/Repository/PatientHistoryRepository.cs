using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class PatientHistoryRepository : IPatientHistoryRepository
    {
        private readonly EMRDbContext _context;
        private readonly DbSet<FamilyHistory> _familyHistories;
        private readonly DbSet<SocialHistory> _socialHistories;

        public PatientHistoryRepository(EMRDbContext context)
        {
            _context = context;
            _familyHistories = context.Set<FamilyHistory>();
            _socialHistories = context.Set<SocialHistory>();
        }

        // Family History Methods
        public async Task<IEnumerable<FamilyHistory>> GetFamilyHistoryAsync(int patientId)
        {
            return await _familyHistories
                .Where(fh => fh.PatientId == patientId && !fh.IsDeleted)
                .OrderBy(fh => fh.Relationship)
                .ToListAsync();
        }

        public async Task<IEnumerable<FamilyHistory>> GetFamilyHistoryByConditionAsync(int patientId, string condition)
        {
            return await _familyHistories
                .Where(fh => 
                    fh.PatientId == patientId && 
                    !fh.IsDeleted &&
                    (fh.Condition.Contains(condition) ||
                     fh.ICD10Code.Contains(condition) ||
                     fh.SNOMEDCode.Contains(condition)))
                .OrderBy(fh => fh.Relationship)
                .ToListAsync();
        }

        public async Task<IEnumerable<FamilyHistory>> GetGeneticRisksAsync(int patientId)
        {
            return await _familyHistories
                .Where(fh => 
                    fh.PatientId == patientId && 
                    !fh.IsDeleted &&
                    fh.IsGeneticRisk)
                .OrderBy(fh => fh.RiskLevel)
                .ToListAsync();
        }

        public async Task<FamilyHistory> AddFamilyHistoryAsync(FamilyHistory familyHistory)
        {
            await _familyHistories.AddAsync(familyHistory);
            await _context.SaveChangesAsync();
            return familyHistory;
        }

        public async Task UpdateFamilyHistoryAsync(FamilyHistory familyHistory)
        {
            _familyHistories.Update(familyHistory);
            await _context.SaveChangesAsync();
        }

        // Social History Methods
        public async Task<SocialHistory> GetSocialHistoryAsync(int patientId)
        {
            return await _socialHistories
                .FirstOrDefaultAsync(sh => sh.PatientId == patientId && !sh.IsDeleted);
        }

        public async Task<IEnumerable<SocialHistory>> GetSocialHistoryByDateRangeAsync(int patientId, DateTime startDate, DateTime endDate)
        {
            return await _socialHistories
                .Where(sh => 
                    sh.PatientId == patientId && 
                    !sh.IsDeleted &&
                    sh.ModifiedAt >= startDate &&
                    sh.ModifiedAt <= endDate)
                .OrderByDescending(sh => sh.ModifiedAt)
                .ToListAsync();
        }

        public async Task<SocialHistory> AddSocialHistoryAsync(SocialHistory socialHistory)
        {
            await _socialHistories.AddAsync(socialHistory);
            await _context.SaveChangesAsync();
            return socialHistory;
        }

        public async Task UpdateSocialHistoryAsync(SocialHistory socialHistory)
        {
            _socialHistories.Update(socialHistory);
            await _context.SaveChangesAsync();
        }

        // Combined History Methods
        public async Task<(IEnumerable<FamilyHistory> FamilyHistory, SocialHistory SocialHistory)> 
            GetCompletePatientHistoryAsync(int patientId)
        {
            var familyHistory = await GetFamilyHistoryAsync(patientId);
            var socialHistory = await GetSocialHistoryAsync(patientId);

            return (familyHistory, socialHistory);
        }

        public async Task<bool> HasSignificantFamilyHistoryAsync(int patientId, string condition)
        {
            return await _familyHistories
                .AnyAsync(fh => 
                    fh.PatientId == patientId && 
                    !fh.IsDeleted &&
                    fh.Condition.Contains(condition) &&
                    (fh.IsGeneticRisk || fh.Severity == "Severe"));
        }

        public async Task<IEnumerable<string>> GetCommonFamilyConditionsAsync(int patientId)
        {
            return await _familyHistories
                .Where(fh => fh.PatientId == patientId && !fh.IsDeleted)
                .GroupBy(fh => fh.Condition)
                .Select(g => g.Key)
                .ToListAsync();
        }
    }
}
