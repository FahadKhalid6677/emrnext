using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.EhrServices
{
    /// <summary>
    /// Service interface for managing patient medical charts
    /// </summary>
    public interface IMedicalChartService
    {
        /// <summary>
        /// Create a new medical chart for a patient
        /// </summary>
        Task<MedicalChart> CreateMedicalChartAsync(Guid patientId, string chartTitle);

        /// <summary>
        /// Get all medical charts for a patient
        /// </summary>
        Task<IEnumerable<MedicalChart>> GetPatientMedicalChartsAsync(Guid patientId);

        /// <summary>
        /// Add a progress note to a medical chart
        /// </summary>
        Task<ProgressNote> AddProgressNoteAsync(
            Guid medicalChartId, 
            Guid providerId, 
            ProgressNote progressNote);

        /// <summary>
        /// Get progress notes for a medical chart
        /// </summary>
        Task<IEnumerable<ProgressNote>> GetProgressNotesAsync(Guid medicalChartId);

        /// <summary>
        /// Attach a clinical document to a progress note
        /// </summary>
        Task<ClinicalDocument> AttachDocumentToProgressNoteAsync(
            Guid progressNoteId, 
            ClinicalDocument document);

        /// <summary>
        /// Get comprehensive patient medical history
        /// </summary>
        Task<PatientMedicalHistory> GetPatientMedicalHistoryAsync(Guid patientId);
    }

    /// <summary>
    /// Represents a comprehensive medical history for a patient
    /// </summary>
    public class PatientMedicalHistory
    {
        public Guid PatientId { get; set; }
        public IEnumerable<MedicalChart> MedicalCharts { get; set; }
        public IEnumerable<Encounter> Encounters { get; set; }
        public IEnumerable<Problem> ChronicConditions { get; set; }
        public IEnumerable<Medication> CurrentMedications { get; set; }
        public IEnumerable<Allergy> Allergies { get; set; }
        public IEnumerable<Immunization> Immunizations { get; set; }
        public Patient.MedicalRiskProfile RiskProfile { get; set; }
    }
}
