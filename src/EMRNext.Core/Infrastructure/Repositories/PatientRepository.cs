using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Repositories;
using EMRNext.Core.Infrastructure.Repositories;
using EMRNext.Core.Domain.Specifications;

namespace EMRNext.Core.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of Patient repository
    /// </summary>
    public class PatientRepository : BaseRepository<Patient, int>, IPatientRepository
    {
        public PatientRepository(DbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Patient>> FindByNameAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => 
                    p.FirstName.Contains(searchTerm) || 
                    p.LastName.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Patient>> FindByAgeRangeAsync(int minAge, int maxAge)
        {
            var spec = new PatientSpecifications.PatientByAgeRangeSpec(minAge, maxAge);
            return await FindAsync(spec);
        }

        public async Task<IReadOnlyList<Patient>> FindByPrimaryProviderAsync(string providerId)
        {
            return await _dbSet
                .Where(p => p.PrimaryCareProvider == providerId)
                .ToListAsync();
        }

        public async Task<bool> ExistsBySocialSecurityNumberAsync(string ssn)
        {
            return await _dbSet.AnyAsync(p => p.SocialSecurityNumber == ssn);
        }

        public async Task<Patient> GetPatientWithFullHistoryAsync(int patientId)
        {
            return await _dbSet
                .Include(p => p.Vitals)
                .Include(p => p.Prescriptions)
                .Include(p => p.Allergies)
                .FirstOrDefaultAsync(p => p.Id == patientId);
        }
    }
}
