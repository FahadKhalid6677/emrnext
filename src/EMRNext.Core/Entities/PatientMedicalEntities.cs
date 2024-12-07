using System;

namespace EMRNext.Core.Entities
{
    public class Vital
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; }
        public DateTime MeasurementDate { get; set; }
        public string MeasuredBy { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public virtual Patient Patient { get; set; }
    }

    public class Allergy
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Allergen { get; set; }
        public string AllergenType { get; set; }
        public string Severity { get; set; }
        public string Reaction { get; set; }
        public DateTime OnsetDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public virtual Patient Patient { get; set; }
    }

    public class Problem
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string ProblemName { get; set; }
        public string IcdCode { get; set; }
        public DateTime OnsetDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public virtual Patient Patient { get; set; }
    }
}
