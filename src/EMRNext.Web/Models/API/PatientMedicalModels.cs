using System;
using System.ComponentModel.DataAnnotations;

namespace EMRNext.Web.Models.API
{
    public class VitalRequest
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        public string Unit { get; set; }

        [Required]
        public DateTime MeasurementDate { get; set; }

        public string Notes { get; set; }
    }

    public class VitalResponse
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public string Unit { get; set; }
        public DateTime MeasurementDate { get; set; }
        public string MeasuredBy { get; set; }
        public string Notes { get; set; }
    }

    public class AllergyRequest
    {
        [Required]
        public string Allergen { get; set; }

        [Required]
        public string AllergenType { get; set; }

        [Required]
        public string Severity { get; set; }

        [Required]
        public string Reaction { get; set; }

        [Required]
        public DateTime OnsetDate { get; set; }

        [Required]
        public string Status { get; set; }

        public string Notes { get; set; }
    }

    public class AllergyResponse
    {
        public int Id { get; set; }
        public string Allergen { get; set; }
        public string AllergenType { get; set; }
        public string Severity { get; set; }
        public string Reaction { get; set; }
        public DateTime OnsetDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class ProblemRequest
    {
        [Required]
        public string ProblemName { get; set; }

        [Required]
        public string IcdCode { get; set; }

        [Required]
        public DateTime OnsetDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string Severity { get; set; }

        public string Notes { get; set; }
    }

    public class ProblemResponse
    {
        public int Id { get; set; }
        public string ProblemName { get; set; }
        public string IcdCode { get; set; }
        public DateTime OnsetDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Severity { get; set; }
        public string Notes { get; set; }
    }
}
