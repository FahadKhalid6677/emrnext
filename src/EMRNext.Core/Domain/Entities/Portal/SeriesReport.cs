using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class SeriesReport : AuditableEntity
    {
        public SeriesReport()
        {
            ParticipantReports = new HashSet<ParticipantReport>();
        }

        public Guid Id { get; set; }

        [Required]
        public Guid GroupSeriesId { get; set; }

        [Required]
        public DateTime GeneratedDate { get; set; }

        [Required]
        [StringLength(100)]
        public string GeneratedBy { get; set; }

        [Required]
        [StringLength(50)]
        public string ReportType { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(2000)]
        public string Summary { get; set; }

        [Required]
        public int TotalSessions { get; set; }

        [Required]
        public int CompletedSessions { get; set; }

        [Required]
        public double AverageAttendance { get; set; }

        [StringLength(2000)]
        public string OutcomeMetrics { get; set; }

        [StringLength(2000)]
        public string Recommendations { get; set; }

        public bool IsFinalized { get; set; }

        public DateTime? FinalizedDate { get; set; }

        [StringLength(100)]
        public string FinalizedBy { get; set; }

        public virtual GroupSeries GroupSeries { get; set; }
        public virtual ICollection<ParticipantReport> ParticipantReports { get; set; }
    }
}
