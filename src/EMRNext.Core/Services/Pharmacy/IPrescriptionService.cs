using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Pharmacy;
using EMRNext.Core.Domain.Models.Pharmacy;

namespace EMRNext.Core.Services.Pharmacy
{
    public interface IPrescriptionService
    {
        // Prescription Management
        Task<Prescription> CreatePrescriptionAsync(PrescriptionRequest request);
        Task<Prescription> UpdatePrescriptionAsync(int prescriptionId, PrescriptionRequest request);
        Task<bool> CancelPrescriptionAsync(int prescriptionId, string reason);
        Task<Prescription> GetPrescriptionAsync(int prescriptionId);
        Task<IEnumerable<Prescription>> GetPatientPrescriptionsAsync(int patientId);
        Task<IEnumerable<Prescription>> GetActivePrescriptionsAsync(int patientId);

        // E-Prescribing
        Task<bool> SendToPrescriptionAsync(int prescriptionId);
        Task<bool> UpdatePrescriptionStatusAsync(int prescriptionId, string status);
        Task<bool> AssignPharmacyAsync(int prescriptionId, int pharmacyId);
        Task<bool> RequestAuthorizationAsync(int prescriptionId);

        // Medication Management
        Task<bool> ValidateMedicationAsync(int medicationId, int patientId);
        Task<IEnumerable<DrugInteraction>> CheckInteractionsAsync(int medicationId, int patientId);
        Task<bool> ValidateDosageAsync(PrescriptionItem item);
        Task<IEnumerable<Medication>> GetAlternativeMedicationsAsync(int medicationId);

        // Refill Management
        Task<PrescriptionFill> ProcessRefillRequestAsync(RefillRequest request);
        Task<bool> AuthorizeRefillAsync(int fillId, string providerId);
        Task<bool> DenyRefillAsync(int fillId, string reason);
        Task<IEnumerable<RefillRequest>> GetPendingRefillsAsync();

        // Document Management
        Task<PrescriptionDocument> AttachPrescriptionDocumentAsync(int prescriptionId, DocumentRequest request);
        Task<PrescriptionFillDocument> AttachFillDocumentAsync(int fillId, DocumentRequest request);
        Task<IEnumerable<PrescriptionDocument>> GetPrescriptionDocumentsAsync(int prescriptionId);

        // Alert Management
        Task<PrescriptionAlert> CreatePrescriptionAlertAsync(int prescriptionId, AlertRequest request);
        Task<MedicationAlert> CreateMedicationAlertAsync(int medicationId, AlertRequest request);
        Task<bool> AcknowledgeAlertAsync(int alertId, string userId);
        Task<IEnumerable<PrescriptionAlert>> GetPendingAlertsAsync();

        // Pharmacy Communication
        Task<bool> SendToPharmacyAsync(int prescriptionId);
        Task<bool> ProcessPharmacyResponseAsync(PharmacyResponse response);
        Task<bool> HandlePharmacyErrorAsync(PharmacyError error);

        // Clinical Decision Support
        Task<IEnumerable<ClinicalAlert>> CheckClinicalAlertsAsync(int prescriptionId);
        Task<bool> ValidateContraindicationsAsync(int medicationId, int patientId);
        Task<IEnumerable<DrugAllergy>> CheckAllergyInteractionsAsync(int medicationId, int patientId);

        // Reporting
        Task<PrescriptionReport> GeneratePrescriptionReportAsync(int prescriptionId);
        Task<MedicationReport> GenerateMedicationReportAsync(int medicationId);
        Task<IEnumerable<PrescriptionSummary>> GetPrescriptionSummaryAsync(DateTime startDate, DateTime endDate);

        // Regulatory Compliance
        Task<bool> ValidateControlledSubstanceAsync(int prescriptionId);
        Task<bool> RecordControlledSubstanceAsync(int prescriptionId);
        Task<IEnumerable<ControlledSubstanceReport>> GetControlledSubstanceReportsAsync(DateTime startDate, DateTime endDate);

        // Quality Management
        Task<QualityReport> GenerateQualityReportAsync(DateTime startDate, DateTime endDate);
        Task<bool> FlagQualityIssueAsync(int prescriptionId, QualityIssue issue);
        Task<IEnumerable<QualityMetric>> GetQualityMetricsAsync();
    }
}
