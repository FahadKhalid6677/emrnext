using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Models
{
    public class Alert
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PatientId { get; set; }
        public bool IsResolved { get; set; }
    }

    public class Recommendation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PatientId { get; set; }
    }

    public class DrugInteraction
    {
        public int Id { get; set; }
        public string Drug1 { get; set; }
        public string Drug2 { get; set; }
        public string InteractionType { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
    }

    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public class TimeSlot
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public int? ResourceId { get; set; }
    }

    public class WaitlistEntry
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime EntryTime { get; set; }
        public string Reason { get; set; }
        public int Priority { get; set; }
    }

    public class ScheduleMetrics
    {
        public int TotalAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public TimeSpan AverageWaitTime { get; set; }
    }

    /// <summary>
    /// Represents a comprehensive clinical encounter
    /// </summary>
    public class Encounter
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public EncounterType Type { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Represents a medical diagnosis
    /// </summary>
    public class Diagnosis
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string DiagnosisCode { get; set; }
        public string Description { get; set; }
        public DateTime DiagnosisDate { get; set; }
        public DiagnosisSeverity Severity { get; set; }
    }

    /// <summary>
    /// Enumerations for clinical models
    /// </summary>
    public enum EncounterType
    {
        Initial,
        Followup,
        Emergency,
        Routine,
        Consultation
    }

    public enum DiagnosisSeverity
    {
        Mild,
        Moderate,
        Severe,
        Critical
    }

    /// <summary>
    /// Enumerations for clinical models
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        Completed,
        Cancelled
    }

    public enum RecommendationType
    {
        Treatment,
        Lifestyle,
        Medication
    }

    public enum InteractionSeverity
    {
        Low,
        Moderate,
        High,
        Contraindicated
    }
}
