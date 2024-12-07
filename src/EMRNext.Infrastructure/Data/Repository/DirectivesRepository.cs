using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class DirectivesRepository : IDirectivesRepository
    {
        private readonly EMRDbContext _context;
        private readonly DbSet<Consent> _consents;
        private readonly DbSet<AdvanceDirective> _advanceDirectives;

        public DirectivesRepository(EMRDbContext context)
        {
            _context = context;
            _consents = context.Set<Consent>();
            _advanceDirectives = context.Set<AdvanceDirective>();
        }

        // Consent Methods
        public async Task<IEnumerable<Consent>> GetPatientConsentsAsync(int patientId)
        {
            return await _consents
                .Where(c => c.PatientId == patientId && !c.IsDeleted)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consent>> GetActiveConsentsAsync(int patientId)
        {
            var currentDate = DateTime.UtcNow;
            return await _consents
                .Where(c => 
                    c.PatientId == patientId && 
                    !c.IsDeleted &&
                    c.StartDate <= currentDate &&
                    (!c.EndDate.HasValue || c.EndDate.Value >= currentDate) &&
                    c.Status == "Active")
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consent>> GetExpiredConsentsAsync(int patientId)
        {
            var currentDate = DateTime.UtcNow;
            return await _consents
                .Where(c => 
                    c.PatientId == patientId && 
                    !c.IsDeleted &&
                    c.EndDate.HasValue &&
                    c.EndDate.Value < currentDate)
                .OrderByDescending(c => c.EndDate)
                .ToListAsync();
        }

        public async Task<Consent> AddConsentAsync(Consent consent)
        {
            await _consents.AddAsync(consent);
            await _context.SaveChangesAsync();
            return consent;
        }

        public async Task UpdateConsentAsync(Consent consent)
        {
            _consents.Update(consent);
            await _context.SaveChangesAsync();
        }

        // Advance Directive Methods
        public async Task<IEnumerable<AdvanceDirective>> GetAdvanceDirectivesAsync(int patientId)
        {
            return await _advanceDirectives
                .Where(ad => ad.PatientId == patientId && !ad.IsDeleted)
                .OrderByDescending(ad => ad.EffectiveDate)
                .ToListAsync();
        }

        public async Task<AdvanceDirective> GetActiveAdvanceDirectiveAsync(int patientId)
        {
            return await _advanceDirectives
                .Where(ad => 
                    ad.PatientId == patientId && 
                    !ad.IsDeleted &&
                    ad.IsActive)
                .OrderByDescending(ad => ad.EffectiveDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AdvanceDirective>> GetDirectivesNeedingReviewAsync(int daysThreshold = 30)
        {
            var reviewDate = DateTime.UtcNow.AddDays(-daysThreshold);
            return await _advanceDirectives
                .Where(ad => 
                    !ad.IsDeleted &&
                    ad.IsActive &&
                    (!ad.ReviewDate.HasValue || ad.ReviewDate.Value <= reviewDate))
                .OrderBy(ad => ad.ReviewDate)
                .Include(ad => ad.Patient)
                .ToListAsync();
        }

        public async Task<AdvanceDirective> AddAdvanceDirectiveAsync(AdvanceDirective directive)
        {
            // Deactivate any existing active directives for the patient
            var existingDirectives = await _advanceDirectives
                .Where(ad => 
                    ad.PatientId == directive.PatientId && 
                    !ad.IsDeleted &&
                    ad.IsActive)
                .ToListAsync();

            foreach (var existing in existingDirectives)
            {
                existing.IsActive = false;
                existing.ModifiedAt = DateTime.UtcNow;
                _advanceDirectives.Update(existing);
            }

            await _advanceDirectives.AddAsync(directive);
            await _context.SaveChangesAsync();
            return directive;
        }

        public async Task UpdateAdvanceDirectiveAsync(AdvanceDirective directive)
        {
            _advanceDirectives.Update(directive);
            await _context.SaveChangesAsync();
        }

        // Combined Directives Methods
        public async Task<(IEnumerable<Consent> ActiveConsents, AdvanceDirective ActiveDirective)> 
            GetActiveDirectivesAsync(int patientId)
        {
            var consents = await GetActiveConsentsAsync(patientId);
            var directive = await GetActiveAdvanceDirectiveAsync(patientId);

            return (consents, directive);
        }

        public async Task<bool> HasValidConsentAsync(int patientId, string consentType)
        {
            var currentDate = DateTime.UtcNow;
            return await _consents
                .AnyAsync(c => 
                    c.PatientId == patientId && 
                    !c.IsDeleted &&
                    c.Type == consentType &&
                    c.StartDate <= currentDate &&
                    (!c.EndDate.HasValue || c.EndDate.Value >= currentDate) &&
                    c.Status == "Active");
        }

        public async Task<IEnumerable<Consent>> GetConsentsRequiringNotificationAsync()
        {
            return await _consents
                .Where(c => 
                    !c.IsDeleted &&
                    c.RequiresNotification &&
                    (!c.LastNotificationDate.HasValue || 
                     c.LastNotificationDate.Value.AddDays(30) <= DateTime.UtcNow))
                .OrderBy(c => c.LastNotificationDate)
                .Include(c => c.Patient)
                .ToListAsync();
        }
    }
}
