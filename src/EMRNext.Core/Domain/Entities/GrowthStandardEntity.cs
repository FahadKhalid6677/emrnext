using System;
using System.Collections.Generic;
using EMRNext.Core.Models.Growth;

namespace EMRNext.Core.Domain.Entities
{
    public class GrowthStandardEntity : BaseEntity
    {
        public GrowthStandardType Type { get; set; }
        public string Gender { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime EffectiveDate { get; set; }
        public virtual ICollection<PercentileDataEntity> PercentileData { get; set; }
        public string MetadataJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PercentileDataEntity : BaseEntity
    {
        public int GrowthStandardId { get; set; }
        public virtual GrowthStandardEntity GrowthStandard { get; set; }
        public MeasurementType MeasurementType { get; set; }
        public double Age { get; set; }
        public double L { get; set; }
        public double M { get; set; }
        public double S { get; set; }
        public string PercentileValuesJson { get; set; }
    }

    public class PatientMeasurementEntity : BaseEntity
    {
        public int PatientId { get; set; }
        public MeasurementType Type { get; set; }
        public double Value { get; set; }
        public DateTime MeasurementDate { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public int? ProviderId { get; set; }
        public string MetadataJson { get; set; }
    }

    public class GrowthAlertEntity : BaseEntity
    {
        public int PatientId { get; set; }
        public MeasurementType Type { get; set; }
        public string AlertType { get; set; }
        public string Description { get; set; }
        public DateTime DetectedDate { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string Resolution { get; set; }
        public int? ProviderId { get; set; }
    }
}
