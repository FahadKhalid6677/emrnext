using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class ClinicalNoteRepository : BaseRepository<ClinicalNote>, IClinicalNoteRepository
    {
        public ClinicalNoteRepository(EMRDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ClinicalNote>> GetPatientNotesAsync(int patientId)
        {
            return await _dbSet
                .Where(n => n.PatientId == patientId && !n.IsDeleted)
                .OrderByDescending(n => n.NoteDate)
                .Include(n => n.Provider)
                .Include(n => n.Encounter)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClinicalNote>> GetEncounterNotesAsync(int encounterId)
        {
            return await _dbSet
                .Where(n => n.EncounterId == encounterId && !n.IsDeleted)
                .OrderByDescending(n => n.NoteDate)
                .Include(n => n.Provider)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClinicalNote>> GetProviderNotesAsync(int providerId)
        {
            return await _dbSet
                .Where(n => n.ProviderId == providerId && !n.IsDeleted)
                .OrderByDescending(n => n.NoteDate)
                .Include(n => n.Patient)
                .Include(n => n.Encounter)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClinicalNote>> GetNotesByTypeAsync(int patientId, string noteType)
        {
            return await _dbSet
                .Where(n => 
                    n.PatientId == patientId && 
                    !n.IsDeleted &&
                    n.Type == noteType)
                .OrderByDescending(n => n.NoteDate)
                .Include(n => n.Provider)
                .Include(n => n.Encounter)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClinicalNote>> GetUnsignedNotesAsync(int providerId)
        {
            return await _dbSet
                .Where(n => 
                    n.ProviderId == providerId && 
                    !n.IsDeleted &&
                    !n.IsSigned)
                .OrderByDescending(n => n.NoteDate)
                .Include(n => n.Patient)
                .Include(n => n.Encounter)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClinicalNote>> GetNotesPendingCosignAsync(int providerId)
        {
            return await _dbSet
                .Where(n => 
                    n.CosignProvider == providerId.ToString() && 
                    !n.IsDeleted &&
                    n.RequiresCosign &&
                    !n.CosignDate.HasValue)
                .OrderByDescending(n => n.NoteDate)
                .Include(n => n.Patient)
                .Include(n => n.Provider)
                .Include(n => n.Encounter)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClinicalNote>> SearchNotesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Where(n => 
                    !n.IsDeleted &&
                    (n.Title.Contains(searchTerm) ||
                     n.Content.Contains(searchTerm) ||
                     n.ChiefComplaint.Contains(searchTerm) ||
                     n.Assessment.Contains(searchTerm) ||
                     n.Plan.Contains(searchTerm)))
                .OrderByDescending(n => n.NoteDate)
                .Include(n => n.Provider)
                .Include(n => n.Patient)
                .Include(n => n.Encounter)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClinicalNote>> GetAmendedNotesAsync(int patientId)
        {
            return await _dbSet
                .Where(n => 
                    n.PatientId == patientId && 
                    !n.IsDeleted &&
                    n.IsAmended)
                .OrderByDescending(n => n.AmendedDate)
                .Include(n => n.Provider)
                .Include(n => n.Encounter)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClinicalNote>> GetConfidentialNotesAsync(int patientId, int providerId)
        {
            return await _dbSet
                .Where(n => 
                    n.PatientId == patientId && 
                    !n.IsDeleted &&
                    n.IsConfidential &&
                    (n.ProviderId == providerId || n.RestrictedTo.Contains(providerId.ToString())))
                .OrderByDescending(n => n.NoteDate)
                .Include(n => n.Provider)
                .Include(n => n.Encounter)
                .ToListAsync();
        }
    }
}
