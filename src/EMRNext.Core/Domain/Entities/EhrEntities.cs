using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Domain.Entities
{
    /// <summary>
    /// Represents a comprehensive medical chart for a patient
    /// </summary>
    public class MedicalChart : BaseIntEntity
    {
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }
        
        public string ChartTitle { get; set; }
        public DateTime ChartDate { get; set; }
        public string ChartType { get; set; }
        
        public virtual ICollection<ProgressNote> ProgressNotes { get; set; }
        public virtual ICollection<ClinicalDocument> Documents { get; set; }
    }

    /// <summary>
    /// Represents a progress note in a patient's medical chart
    /// </summary>
    public class ProgressNote : BaseIntEntity
    {
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }
        
        public Guid MedicalChartId { get; set; }
        public MedicalChart MedicalChart { get; set; }
        
        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; }
        
        public DateTime NoteDate { get; set; }
        public string NoteType { get; set; }
        public string SubjectiveObservation { get; set; }
        public string ObjectiveObservation { get; set; }
        public string Assessment { get; set; }
        public string Plan { get; set; }
        
        public virtual ICollection<ClinicalDocument> AttachedDocuments { get; set; }
    }

    /// <summary>
    /// Represents a clinical document attached to medical records
    /// </summary>
    public class ClinicalDocument : BaseIntEntity
    {
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }
        
        public string DocumentTitle { get; set; }
        public string DocumentType { get; set; }
        public string FileLocation { get; set; }
        public DateTime DocumentDate { get; set; }
        public string DocumentStatus { get; set; }
        
        public Guid? ProgressNoteId { get; set; }
        public ProgressNote ProgressNote { get; set; }
    }

    /// <summary>
    /// Represents a clinical protocol for patient care
    /// </summary>
    public class ClinicalProtocol : BaseIntEntity
    {
        public string ProtocolName { get; set; }
        public string ProtocolDescription { get; set; }
        public string MedicalSpecialty { get; set; }
        
        public virtual ICollection<ProtocolStep> Steps { get; set; }
        public virtual ICollection<Patient> AssignedPatients { get; set; }
    }

    /// <summary>
    /// Represents a step in a clinical protocol
    /// </summary>
    public class ProtocolStep : BaseIntEntity
    {
        public Guid ClinicalProtocolId { get; set; }
        public ClinicalProtocol ClinicalProtocol { get; set; }
        
        public int StepOrder { get; set; }
        public string StepDescription { get; set; }
        public string ExpectedOutcome { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
    }

    /// <summary>
    /// Represents a medical provider
    /// </summary>
    public class Provider : BaseIntEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MedicalSpecialty { get; set; }
        public string LicenseNumber { get; set; }
        
        public virtual ICollection<ProgressNote> ProgressNotes { get; set; }
    }
}
