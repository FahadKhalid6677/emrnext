using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMRNext.Web.Models.API
{
    public class EncounterCreationRequest
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [Required]
        public string EncounterType { get; set; }

        [Required]
        public DateTime EncounterDate { get; set; }

        public string Department { get; set; }
        public string Facility { get; set; }
        public string ChiefComplaint { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public List<string> ProcedureCodes { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }

    public class EncounterUpdateRequest
    {
        public string EncounterType { get; set; }
        public string Department { get; set; }
        public string Facility { get; set; }
        public string ChiefComplaint { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public List<string> ProcedureCodes { get; set; }
        public string Status { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }

    public class EncounterResponse
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public string EncounterType { get; set; }
        public DateTime EncounterDate { get; set; }
        public string Department { get; set; }
        public string Facility { get; set; }
        public string ChiefComplaint { get; set; }
        public string Status { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public List<string> ProcedureCodes { get; set; }
        public List<VitalsResponse> Vitals { get; set; }
        public List<ClinicalNoteResponse> Notes { get; set; }
        public List<OrderResponse> Orders { get; set; }
        public List<PrescriptionResponse> Prescriptions { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }

    public class ClinicalNoteRequest
    {
        [Required]
        public string NoteType { get; set; }

        [Required]
        public string Content { get; set; }

        public List<string> DiagnosisCodes { get; set; }
        public List<string> ProcedureCodes { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class ClinicalNoteResponse
    {
        public int Id { get; set; }
        public string NoteType { get; set; }
        public string Content { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public List<string> ProcedureCodes { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }

    public class VitalsRequest
    {
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public int? SystolicBP { get; set; }
        public int? DiastolicBP { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BMI { get; set; }
        public int? OxygenSaturation { get; set; }
        public string Pain { get; set; }
        public Dictionary<string, string> CustomMeasurements { get; set; }
    }

    public class VitalsResponse
    {
        public int Id { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public int? SystolicBP { get; set; }
        public int? DiastolicBP { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BMI { get; set; }
        public int? OxygenSaturation { get; set; }
        public string Pain { get; set; }
        public Dictionary<string, string> CustomMeasurements { get; set; }
        public string RecordedBy { get; set; }
        public DateTime RecordedDate { get; set; }
    }

    public class OrderRequest
    {
        [Required]
        public string OrderType { get; set; }

        [Required]
        public string OrderCode { get; set; }

        public string Description { get; set; }
        public string Instructions { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Frequency { get; set; }
        public string Priority { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }

    public class OrderResponse
    {
        public int Id { get; set; }
        public string OrderType { get; set; }
        public string OrderCode { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Frequency { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public List<ResultResponse> Results { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
        public string OrderedBy { get; set; }
        public DateTime OrderedDate { get; set; }
    }

    public class ResultRequest
    {
        [Required]
        public string ResultType { get; set; }

        [Required]
        public string Value { get; set; }

        public string Unit { get; set; }
        public string ReferenceRange { get; set; }
        public string Interpretation { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }

    public class ResultResponse
    {
        public int Id { get; set; }
        public string ResultType { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public string ReferenceRange { get; set; }
        public string Interpretation { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
        public string RecordedBy { get; set; }
        public DateTime RecordedDate { get; set; }
    }

    public class PrescriptionRequest
    {
        [Required]
        public string MedicationCode { get; set; }

        [Required]
        public string Dosage { get; set; }

        [Required]
        public string Route { get; set; }

        [Required]
        public string Frequency { get; set; }

        public string Instructions { get; set; }
        public int Quantity { get; set; }
        public int Refills { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPRN { get; set; }
        public string PRNInstructions { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }

    public class PrescriptionResponse
    {
        public int Id { get; set; }
        public string MedicationCode { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Route { get; set; }
        public string Frequency { get; set; }
        public string Instructions { get; set; }
        public int Quantity { get; set; }
        public int Refills { get; set; }
        public int RefillsRemaining { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPRN { get; set; }
        public string PRNInstructions { get; set; }
        public string Status { get; set; }
        public List<string> DiagnosisCodes { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
        public string PrescribedBy { get; set; }
        public DateTime PrescribedDate { get; set; }
    }
}
