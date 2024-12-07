using System;
using System.Collections.Generic;

namespace EMRNext.Web.Models.API
{
    public class PrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid ProviderId { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public int Duration { get; set; }
        public string DurationUnit { get; set; }
        public DateTime? PrescriptionDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public List<string> Instructions { get; set; }
        public List<string> Warnings { get; set; }
    }

    public class DrugInteractionDto
    {
        public string Drug1 { get; set; }
        public string Drug2 { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
    }
}
