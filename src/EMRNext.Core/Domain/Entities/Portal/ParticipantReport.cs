using System;
using System.ComponentModel.DataAnnotations;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Domain.Entities.Portal
{
    public class ParticipantReport : AuditableEntity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid SeriesReportId { get; set; }

        [Required]
        public Guid ParticipantId { get; set; }

        [Required]
        public int SessionsAttended { get; set; }

        [Required]
        public double AttendanceRate { get; set; }

        [Required]
        [StringLength(50)]
        public string ParticipationLevel { get; set; }

        [StringLength(2000)]
        public string Progress { get; set; }

        [StringLength(2000)]
        public string Achievements { get; set; }

        [StringLength(2000)]
        public string Challenges { get; set; }

        [StringLength(2000)]
        public string InterventionsApplied { get; set; }

        [StringLength(2000)]
        public string ResponseToInterventions { get; set; }

        [StringLength(2000)]
        public string Goals { get; set; }

        [StringLength(2000)]
        public string GoalProgress { get; set; }

        [StringLength(2000)]
        public string RecommendedNextSteps { get; set; }

        [StringLength(2000)]
        public string AdditionalNotes { get; set; }

        public virtual SeriesReport SeriesReport { get; set; }
        public virtual GroupParticipant Participant { get; set; }
    }
}
