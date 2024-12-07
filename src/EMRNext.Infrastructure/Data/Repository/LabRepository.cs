using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class LabRepository : BaseRepository<LabOrder>, ILabRepository
    {
        private readonly DbSet<LabResult> _labResults;

        public LabRepository(EMRDbContext context) : base(context)
        {
            _labResults = context.Set<LabResult>();
        }

        public async Task<IEnumerable<LabOrder>> GetPatientLabOrdersAsync(int patientId)
        {
            return await _dbSet
                .Where(lo => lo.PatientId == patientId && !lo.IsDeleted)
                .OrderByDescending(lo => lo.OrderDate)
                .Include(lo => lo.Provider)
                .Include(lo => lo.Results)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabOrder>> GetPendingLabOrdersAsync(int patientId)
        {
            return await _dbSet
                .Where(lo => 
                    lo.PatientId == patientId && 
                    !lo.IsDeleted &&
                    (lo.Status == "Pending" || lo.Status == "In Progress"))
                .OrderBy(lo => lo.OrderDate)
                .Include(lo => lo.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabResult>> GetLabResultsAsync(int labOrderId)
        {
            return await _labResults
                .Where(lr => lr.LabOrderId == labOrderId && !lr.IsDeleted)
                .OrderByDescending(lr => lr.ResultDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabResult>> GetPatientLabResultsAsync(int patientId)
        {
            return await _labResults
                .Where(lr => lr.PatientId == patientId && !lr.IsDeleted)
                .OrderByDescending(lr => lr.ResultDate)
                .Include(lr => lr.LabOrder)
                    .ThenInclude(lo => lo.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabResult>> GetAbnormalResultsAsync(int patientId)
        {
            return await _labResults
                .Where(lr => 
                    lr.PatientId == patientId && 
                    !lr.IsDeleted &&
                    lr.IsAbnormal)
                .OrderByDescending(lr => lr.ResultDate)
                .Include(lr => lr.LabOrder)
                    .ThenInclude(lo => lo.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabResult>> GetCriticalResultsAsync(int patientId)
        {
            return await _labResults
                .Where(lr => 
                    lr.PatientId == patientId && 
                    !lr.IsDeleted &&
                    lr.IsCritical)
                .OrderByDescending(lr => lr.ResultDate)
                .Include(lr => lr.LabOrder)
                    .ThenInclude(lo => lo.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabOrder>> SearchLabOrdersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Where(lo => 
                    lo.OrderNumber.Contains(searchTerm) ||
                    lo.TestName.Contains(searchTerm) ||
                    lo.TestCode.Contains(searchTerm) ||
                    lo.LOINC.Contains(searchTerm))
                .OrderByDescending(lo => lo.OrderDate)
                .Include(lo => lo.Provider)
                .Include(lo => lo.Patient)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabOrder>> GetLabOrdersByProviderAsync(int providerId)
        {
            return await _dbSet
                .Where(lo => lo.ProviderId == providerId && !lo.IsDeleted)
                .OrderByDescending(lo => lo.OrderDate)
                .Include(lo => lo.Patient)
                .Include(lo => lo.Results)
                .ToListAsync();
        }
    }
}
