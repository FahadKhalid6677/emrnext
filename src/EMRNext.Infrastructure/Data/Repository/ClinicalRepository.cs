using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;

namespace EMRNext.Infrastructure.Data.Repository
{
    public class ClinicalRepository : BaseRepository<Encounter>, IClinicalRepository
    {
        public ClinicalRepository(EMRDbContext context) : base(context)
        {
        }

        public async Task<Encounter> GetEncounterWithDetailsAsync(int encounterId)
        {
            return await _dbSet
                .Include(e => e.Patient)
                .Include(e => e.Provider)
                .Include(e => e.Supervisor)
                .Include(e => e.Facility)
                .Include(e => e.Diagnoses)
                .Include(e => e.Procedures)
                .Include(e => e.Prescriptions)
                .Include(e => e.Orders)
                    .ThenInclude(o => o.OrderDetails)
                .Include(e => e.Orders)
                    .ThenInclude(o => o.OrderResults)
                .Include(e => e.Vitals)
                .FirstOrDefaultAsync(e => e.Id == encounterId);
        }

        public async Task<IEnumerable<Encounter>> GetPatientEncountersAsync(int patientId)
        {
            return await _dbSet
                .Include(e => e.Provider)
                .Include(e => e.Facility)
                .Where(e => e.PatientId == patientId && !e.IsDeleted)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId)
        {
            return await _context.Vitals
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetActiveOrdersAsync(int patientId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Provider)
                .Where(o => o.PatientId == patientId && 
                           o.Status != "Completed" && 
                           o.Status != "Cancelled")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderResult>> GetOrderResultsAsync(int orderId)
        {
            return await _context.OrderResults
                .Where(r => r.OrderId == orderId)
                .OrderByDescending(r => r.ResultDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateClinicalNotesAsync(int encounterId, string subjective, string objective, string assessment, string plan)
        {
            var encounter = await GetByIdAsync(encounterId);
            if (encounter == null)
                return false;

            encounter.SubjectiveNotes = subjective;
            encounter.ObjectiveNotes = objective;
            encounter.AssessmentNotes = assessment;
            encounter.PlanNotes = plan;
            encounter.ModifiedAt = DateTime.UtcNow;

            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddDiagnosisAsync(Diagnosis diagnosis)
        {
            try
            {
                await _context.Diagnoses.AddAsync(diagnosis);
                await SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddProcedureAsync(Procedure procedure)
        {
            try
            {
                await _context.Procedures.AddAsync(procedure);
                await SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddOrderAsync(Order order)
        {
            try
            {
                await _context.Orders.AddAsync(order);
                await SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, string notes = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return false;

            order.Status = status;
            if (!string.IsNullOrEmpty(notes))
                order.ResultNotes = notes;
            order.ModifiedAt = DateTime.UtcNow;

            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddVitalsAsync(Vital vitals)
        {
            try
            {
                await _context.Vitals.AddAsync(vitals);
                await SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Diagnosis>> GetActiveDiagnosesAsync(int patientId)
        {
            return await _context.Diagnoses
                .Where(d => d.PatientId == patientId && 
                           d.Status == "Active" && 
                           !d.IsDeleted)
                .OrderByDescending(d => d.DateDiagnosed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Prescription>> GetActivePrescriptionsAsync(int patientId)
        {
            var currentDate = DateTime.UtcNow.Date;
            return await _context.Prescriptions
                .Where(p => p.PatientId == patientId && 
                           p.StartDate <= currentDate &&
                           (!p.EndDate.HasValue || p.EndDate.Value >= currentDate) &&
                           p.Status == "Active" &&
                           !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
    }
}
