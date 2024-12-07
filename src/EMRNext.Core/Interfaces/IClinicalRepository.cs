using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Interfaces
{
    public interface IClinicalRepository : IRepository<Encounter>
    {
        Task<Encounter> GetEncounterWithDetailsAsync(int encounterId);
        Task<IEnumerable<Encounter>> GetPatientEncountersAsync(int patientId);
        Task<IEnumerable<Vital>> GetPatientVitalsAsync(int patientId);
        Task<IEnumerable<Order>> GetActiveOrdersAsync(int patientId);
        Task<IEnumerable<OrderResult>> GetOrderResultsAsync(int orderId);
        Task<bool> UpdateClinicalNotesAsync(int encounterId, string subjective, string objective, string assessment, string plan);
        Task<bool> AddDiagnosisAsync(Diagnosis diagnosis);
        Task<bool> AddProcedureAsync(Procedure procedure);
        Task<bool> AddOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, string notes = null);
        Task<bool> AddVitalsAsync(Vital vitals);
        Task<IEnumerable<Diagnosis>> GetActiveDiagnosesAsync(int patientId);
        Task<IEnumerable<Prescription>> GetActivePrescriptionsAsync(int patientId);
    }
}
