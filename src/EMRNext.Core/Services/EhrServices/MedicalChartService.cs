using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;
using EMRNext.Core.Services.EhrServices;

namespace EMRNext.Core.Services.EhrServices
{
    /// <summary>
    /// Service for managing patient medical charts and records
    /// </summary>
    public class MedicalChartService : IMedicalChartService
    {
        private readonly ILogger<MedicalChartService> _logger;
        private readonly IGenericRepository<Patient> _patientRepository;
        private readonly IGenericRepository<MedicalChart> _medicalChartRepository;
        private readonly IGenericRepository<ProgressNote> _progressNoteRepository;
        private readonly IGenericRepository<ClinicalDocument> _documentRepository;

        public MedicalChartService(
            ILogger<MedicalChartService> logger,
            IGenericRepository<Patient> patientRepository,
            IGenericRepository<MedicalChart> medicalChartRepository,
            IGenericRepository<ProgressNote> progressNoteRepository,
            IGenericRepository<ClinicalDocument> documentRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _medicalChartRepository = medicalChartRepository ?? throw new ArgumentNullException(nameof(medicalChartRepository));
            _progressNoteRepository = progressNoteRepository ?? throw new ArgumentNullException(nameof(progressNoteRepository));
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        }

        /// <inheritdoc/>
        public async Task<MedicalChart> CreateMedicalChartAsync(Guid patientId, string chartTitle)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                throw new ArgumentException("Patient not found", nameof(patientId));

            var medicalChart = new MedicalChart
            {
                PatientId = patientId,
                ChartTitle = chartTitle,
                ChartDate = DateTime.UtcNow,
                ChartType = DetermineChartType(chartTitle)
            };

            await _medicalChartRepository.AddAsync(medicalChart);
            await _medicalChartRepository.SaveChangesAsync();

            _logger.LogInformation($"Created medical chart for patient {patientId}");
            return medicalChart;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MedicalChart>> GetPatientMedicalChartsAsync(Guid patientId)
        {
            return await _medicalChartRepository.FindAsync(
                chart => chart.PatientId == patientId
            );
        }

        /// <inheritdoc/>
        public async Task<ProgressNote> AddProgressNoteAsync(
            Guid medicalChartId, 
            Guid providerId, 
            ProgressNote progressNote)
        {
            var medicalChart = await _medicalChartRepository.GetByIdAsync(medicalChartId);
            if (medicalChart == null)
                throw new ArgumentException("Medical chart not found", nameof(medicalChartId));

            progressNote.MedicalChartId = medicalChartId;
            progressNote.ProviderId = providerId;
            progressNote.NoteDate = DateTime.UtcNow;

            await _progressNoteRepository.AddAsync(progressNote);
            await _progressNoteRepository.SaveChangesAsync();

            _logger.LogInformation($"Added progress note to medical chart {medicalChartId}");
            return progressNote;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProgressNote>> GetProgressNotesAsync(Guid medicalChartId)
        {
            return await _progressNoteRepository.FindAsync(
                note => note.MedicalChartId == medicalChartId
            );
        }

        /// <inheritdoc/>
        public async Task<ClinicalDocument> AttachDocumentToProgressNoteAsync(
            Guid progressNoteId, 
            ClinicalDocument document)
        {
            var progressNote = await _progressNoteRepository.GetByIdAsync(progressNoteId);
            if (progressNote == null)
                throw new ArgumentException("Progress note not found", nameof(progressNoteId));

            document.ProgressNoteId = progressNoteId;
            document.DocumentDate = DateTime.UtcNow;

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation($"Attached document to progress note {progressNoteId}");
            return document;
        }

        /// <inheritdoc/>
        public async Task<PatientMedicalHistory> GetPatientMedicalHistoryAsync(Guid patientId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                throw new ArgumentException("Patient not found", nameof(patientId));

            return new PatientMedicalHistory
            {
                PatientId = patientId,
                MedicalCharts = await GetPatientMedicalChartsAsync(patientId),
                Encounters = patient.Encounters,
                ChronicConditions = patient.Problems,
                CurrentMedications = patient.Medications,
                Allergies = patient.Allergies,
                Immunizations = patient.Immunizations,
                RiskProfile = patient.MedicalRiskProfile
            };
        }

        /// <summary>
        /// Determine chart type based on title
        /// </summary>
        private string DetermineChartType(string chartTitle)
        {
            // Implement logic to categorize chart types
            return chartTitle.Contains("Consultation") ? "Consultation" : 
                   chartTitle.Contains("Follow-up") ? "Follow-up" : 
                   "General";
        }
    }
}
