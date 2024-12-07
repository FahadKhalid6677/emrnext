using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Clinical
{
    public class LabOrderEntity : BaseEntity<int>
    {
        public int PatientId { get; set; }
        public int OrderingProviderId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public string Priority { get; set; } // Routine, STAT, Urgent
        public string Status { get; set; } // Ordered, Collected, InProcess, Completed, Cancelled
        public string ClinicalHistory { get; set; }
        public string OrderDiagnosis { get; set; }
        public bool IsFasting { get; set; }
        public string SpecialInstructions { get; set; }
        public DateTime? CollectionDateTime { get; set; }
        public string CollectionSite { get; set; }
        public string SpecimenType { get; set; }
        public string AccessionNumber { get; set; }
        public virtual ICollection<LabTestOrderEntity> Tests { get; set; }
        public virtual ICollection<LabResultEntity> Results { get; set; }
    }

    public class LabTestOrderEntity : BaseEntity<int>
    {
        public int LabOrderId { get; set; }
        public virtual LabOrderEntity LabOrder { get; set; }
        public int TestId { get; set; }
        public virtual LabTestDefinitionEntity Test { get; set; }
        public string Status { get; set; }
        public DateTime? CompletionDateTime { get; set; }
        public string PerformingLab { get; set; }
        public string ExternalLabId { get; set; }
        public virtual ICollection<LabResultEntity> Results { get; set; }
    }

    public class LabTestDefinitionEntity : BaseEntity<int>
    {
        public string Code { get; set; } // LOINC code
        public string Name { get; set; }
        public string Category { get; set; }
        public string Specimen { get; set; }
        public string Container { get; set; }
        public string Method { get; set; }
        public int? TurnaroundTime { get; set; }
        public bool RequiresFasting { get; set; }
        public string PreparationInstructions { get; set; }
        public virtual ICollection<LabReferenceRangeEntity> ReferenceRanges { get; set; }
    }

    public class LabReferenceRangeEntity : BaseEntity<int>
    {
        public int TestId { get; set; }
        public virtual LabTestDefinitionEntity Test { get; set; }
        public string Gender { get; set; }
        public decimal? MinAge { get; set; }
        public decimal? MaxAge { get; set; }
        public string LowValue { get; set; }
        public string HighValue { get; set; }
        public string Units { get; set; }
        public string Interpretation { get; set; }
        public string Source { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    public class LabResultEntity : BaseEntity<int>
    {
        public int LabTestOrderId { get; set; }
        public virtual LabTestOrderEntity TestOrder { get; set; }
        public string Value { get; set; }
        public string Units { get; set; }
        public string Flag { get; set; } // Normal, Low, High, Critical
        public string Status { get; set; } // Preliminary, Final, Corrected, Cancelled
        public DateTime ResultDateTime { get; set; }
        public int? PerformingTechnologistId { get; set; }
        public int? ValidatingProviderId { get; set; }
        public string Method { get; set; }
        public string Equipment { get; set; }
        public string Comments { get; set; }
        public bool IsCritical { get; set; }
        public DateTime? CriticalNotificationDateTime { get; set; }
        public int? NotifiedProviderId { get; set; }
    }

    public class ImagingOrderEntity : BaseEntity<int>
    {
        public int PatientId { get; set; }
        public int OrderingProviderId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public string Priority { get; set; } // Routine, STAT, Urgent
        public string Status { get; set; }
        public string Modality { get; set; } // XR, CT, MRI, US, etc.
        public string StudyType { get; set; }
        public string BodyPart { get; set; }
        public string Laterality { get; set; }
        public string ClinicalHistory { get; set; }
        public string OrderDiagnosis { get; set; }
        public string SpecialInstructions { get; set; }
        public string Protocol { get; set; }
        public DateTime? ScheduledDateTime { get; set; }
        public string AccessionNumber { get; set; }
        public virtual ICollection<ImagingResultEntity> Results { get; set; }
    }

    public class ImagingResultEntity : BaseEntity<int>
    {
        public int ImagingOrderId { get; set; }
        public virtual ImagingOrderEntity Order { get; set; }
        public DateTime StudyDateTime { get; set; }
        public string Status { get; set; } // Preliminary, Final, Addendum
        public string Findings { get; set; }
        public string Impression { get; set; }
        public string Technique { get; set; }
        public string ComparisonStudies { get; set; }
        public bool IsCritical { get; set; }
        public DateTime? CriticalNotificationDateTime { get; set; }
        public int? NotifiedProviderId { get; set; }
        public int RadiologistId { get; set; }
        public DateTime ReportDateTime { get; set; }
        public virtual ICollection<ImagingStudyEntity> Studies { get; set; }
    }

    public class ImagingStudyEntity : BaseEntity<int>
    {
        public int ImagingResultId { get; set; }
        public virtual ImagingResultEntity Result { get; set; }
        public string StudyInstanceUid { get; set; }
        public string AccessionNumber { get; set; }
        public string Modality { get; set; }
        public DateTime StudyDateTime { get; set; }
        public string Description { get; set; }
        public int NumberOfSeries { get; set; }
        public int NumberOfInstances { get; set; }
        public string StoragePath { get; set; }
        public long StorageSize { get; set; }
        public string PacsStatus { get; set; }
        public virtual ICollection<ImagingSeriesEntity> Series { get; set; }
    }

    public class ImagingSeriesEntity : BaseEntity<int>
    {
        public int ImagingStudyId { get; set; }
        public virtual ImagingStudyEntity Study { get; set; }
        public string SeriesInstanceUid { get; set; }
        public string Number { get; set; }
        public string Modality { get; set; }
        public string Description { get; set; }
        public string BodyPart { get; set; }
        public string Laterality { get; set; }
        public DateTime SeriesDateTime { get; set; }
        public int NumberOfInstances { get; set; }
        public string StoragePath { get; set; }
        public virtual ICollection<ImagingInstanceEntity> Instances { get; set; }
    }

    public class ImagingInstanceEntity : BaseEntity<int>
    {
        public int ImagingSeriesId { get; set; }
        public virtual ImagingSeriesEntity Series { get; set; }
        public string SopInstanceUid { get; set; }
        public string Number { get; set; }
        public DateTime InstanceDateTime { get; set; }
        public string StoragePath { get; set; }
        public long StorageSize { get; set; }
        public string DicomMetadata { get; set; }
    }
}
