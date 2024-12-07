using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;

        public PatientRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Patient> GetByIdAsync(int id)
        {
            return await _context.Patients
                .Include(p => p.Encounters)
                .Include(p => p.Documents)
                .Include(p => p.Insurances)
                .Include(p => p.Allergies)
                .Include(p => p.Problems)
                .Include(p => p.Medications)
                .Include(p => p.Immunizations)
                .Include(p => p.LabResults)
                .Include(p => p.Vitals)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Patient> GetByPublicIdAsync(Guid publicId)
        {
            return await _context.Patients
                .Include(p => p.Encounters)
                .Include(p => p.Documents)
                .Include(p => p.Insurances)
                .Include(p => p.Allergies)
                .Include(p => p.Problems)
                .Include(p => p.Medications)
                .Include(p => p.Immunizations)
                .Include(p => p.LabResults)
                .Include(p => p.Vitals)
                .FirstOrDefaultAsync(p => p.PublicId == publicId);
        }

        public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm, int skip = 0, int take = 20)
        {
            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(searchTerm) ||
                    p.LastName.ToLower().Contains(searchTerm) ||
                    p.SocialSecurityNumber.Contains(searchTerm) ||
                    p.PhoneHome.Contains(searchTerm) ||
                    p.PhoneCell.Contains(searchTerm) ||
                    p.Email.ToLower().Contains(searchTerm));
            }

            return await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<Patient> UpdateAsync(Patient patient)
        {
            _context.Entry(patient).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<IEnumerable<Encounter>> GetEncountersAsync(int patientId)
        {
            return await _context.Encounters
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.EncounterDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetDocumentsAsync(int patientId)
        {
            return await _context.Documents
                .Where(d => d.PatientId == patientId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Insurance>> GetInsurancesAsync(int patientId)
        {
            return await _context.Insurances
                .Where(i => i.PatientId == patientId)
                .OrderByDescending(i => i.EffectiveDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Allergy>> GetAllergiesAsync(int patientId)
        {
            return await _context.Allergies
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.Severity)
                .ToListAsync();
        }

        public async Task<IEnumerable<Problem>> GetProblemsAsync(int patientId)
        {
            return await _context.Problems
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.OnsetDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Medication>> GetMedicationsAsync(int patientId)
        {
            return await _context.Medications
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Immunization>> GetImmunizationsAsync(int patientId)
        {
            return await _context.Immunizations
                .Where(i => i.PatientId == patientId)
                .OrderByDescending(i => i.AdministrationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LabResult>> GetLabResultsAsync(int patientId)
        {
            return await _context.LabResults
                .Where(l => l.PatientId == patientId)
                .OrderByDescending(l => l.ResultDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vital>> GetVitalsAsync(int patientId)
        {
            return await _context.Vitals
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.MeasurementDate)
                .ToListAsync();
        }
    }
}
