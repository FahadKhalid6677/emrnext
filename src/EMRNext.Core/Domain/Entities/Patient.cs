using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Domain.Entities
{
    /// <summary>
    /// Represents a patient in the EMR system
    /// </summary>
    public class Patient : BaseIntEntity
    {
        // Existing properties
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string SocialSecurityNumber { get; set; }
        public string DriversLicense { get; set; }

        // Contact Information
        public string Email { get; set; }
        public string PhoneHome { get; set; }
        public string PhoneCell { get; set; }
        public string PhoneWork { get; set; }
        public string PhoneEmergency { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyRelationship { get; set; }

        // Address
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        // Demographics
        public string Language { get; set; }
        public string Race { get; set; }
        public string Ethnicity { get; set; }
        public string Religion { get; set; }

        // Employment
        public string Occupation { get; set; }
        public string Employer { get; set; }
        public decimal? MonthlyIncome { get; set; }
        public int? FamilySize { get; set; }

        // Medical
        public string PrimaryCareProvider { get; set; }
        public string ReferredBy { get; set; }
        public string PreferredPharmacy { get; set; }
        public bool AllowHealthInfoEx { get; set; }
        public DateTime? HipaaNoticeReceived { get; set; }

        // HIPAA
        public bool HipaaAllowVoiceMessage { get; set; }
        public bool HipaaAllowMail { get; set; }
        public bool HipaaAllowEmail { get; set; }
        public bool HipaaAllowSMS { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public Guid PublicId { get; set; }

        // Navigation Properties
        public virtual ICollection<Encounter> Encounters { get; set; }
        public virtual ICollection<Insurance> Insurances { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Allergy> Allergies { get; set; }
        public virtual ICollection<Problem> Problems { get; set; }
        public virtual ICollection<Medication> Medications { get; set; }
        public virtual ICollection<Immunization> Immunizations { get; set; }
        public virtual ICollection<LabResult> LabResults { get; set; }
        public virtual ICollection<Vital> Vitals { get; set; }

        // Enhanced medical tracking features
        public virtual ICollection<MedicalChart> MedicalCharts { get; set; }
        public virtual ICollection<ProgressNote> ProgressNotes { get; set; }
        public virtual ICollection<ClinicalProtocol> AssignedProtocols { get; set; }

        // Comprehensive medical risk profile
        public MedicalRiskProfile MedicalRiskProfile { get; set; }

        public class MedicalRiskProfile
        {
            // Existing properties
            public List<string> RiskFactors { get; set; } = new List<string>();

            // Genetic risk assessment properties
            public double DiabetesRisk { get; set; }
            public double CardiovascularRisk { get; set; }
            public double CancerRisk { get; set; }
            public List<string> GeneticPredispositions { get; set; } = new List<string>();

            // Risk assessment timestamp
            public DateTime? LastRiskAssessmentDate { get; set; }

            /// <summary>
            /// Determines overall risk severity based on individual risk factors
            /// </summary>
            public RiskSeverity GetOverallRiskSeverity()
            {
                var riskFactors = new[] { DiabetesRisk, CardiovascularRisk, CancerRisk };
                var averageRisk = riskFactors.Average();

                return averageRisk switch
                {
                    double r when r < 0.2 => RiskSeverity.Low,
                    double r when r < 0.5 => RiskSeverity.Moderate,
                    double r when r < 0.7 => RiskSeverity.High,
                    _ => RiskSeverity.VeryHigh
                };
            }
        }

        public enum RiskSeverity
        {
            Low,
            Moderate, 
            High,
            VeryHigh
        }

        // Domain-specific methods
        public string GetFullName() => 
            $"{FirstName} {(string.IsNullOrWhiteSpace(MiddleName) ? "" : MiddleName + " ")}{LastName}";

        public int CalculateAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        public void UpdateContactInformation(
            string email = null, 
            string phoneHome = null, 
            string phoneCell = null, 
            string updatedBy = null)
        {
            if (!string.IsNullOrWhiteSpace(email)) Email = email;
            if (!string.IsNullOrWhiteSpace(phoneHome)) PhoneHome = phoneHome;
            if (!string.IsNullOrWhiteSpace(phoneCell)) PhoneCell = phoneCell;
            
            Update(updatedBy);
        }
    }
}
