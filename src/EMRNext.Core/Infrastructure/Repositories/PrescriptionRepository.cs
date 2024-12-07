using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Models;
using EMRNext.Core.Domain.Repositories;
using EMRNext.Core.Infrastructure.Repositories;
using EMRNext.Core.Domain.Specifications;

namespace EMRNext.Core.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of Prescription repository
    /// </summary>
    public class PrescriptionRepository : BaseRepository<Prescription, int>, IPrescriptionRepository
    {
        public PrescriptionRepository(DbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Prescription>> GetActivePatientPrescriptionsAsync(int patientId)
        {
            var spec = new PrescriptionSpecifications.ActivePatientPrescriptionsSpec(patientId);
            return await FindAsync(spec);
        }

        public async Task<IReadOnlyList<Prescription>> FindByMedicationNameAsync(string medicationName)
        {
            return await _dbSet
                .Where(p => p.MedicationName.Contains(medicationName))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Prescription>> GetPrescriptionsDueForRefillAsync()
        {
            var spec = new PrescriptionSpecifications.PrescriptionsDueForRefillSpec();
            return await FindAsync(spec);
        }

        public async Task<IReadOnlyList<DrugInteraction>> CheckDrugInteractionsAsync(List<string> medications)
        {
            // Placeholder implementation - in a real-world scenario, 
            // this would likely involve a more complex drug interaction checking service
            var interactions = new List<DrugInteraction>();

            for (int i = 0; i < medications.Count; i++)
            {
                for (int j = i + 1; j < medications.Count; j++)
                {
                    // Simulate basic interaction checking
                    interactions.Add(new DrugInteraction
                    {
                        Drug1 = medications[i],
                        Drug2 = medications[j],
                        Severity = InteractionSeverity.Low,
                        Description = $"Potential interaction between {medications[i]} and {medications[j]}"
                    });
                }
            }

            return interactions;
        }

        public async Task<Prescription> GetPrescriptionWithFillHistoryAsync(int prescriptionId)
        {
            return await _dbSet
                .Include(p => p.Fills)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);
        }
    }
}
