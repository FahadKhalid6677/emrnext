using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Immunization
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string VaccineName { get; set; }
        public string CVXCode { get; set; }
        public string MVXCode { get; set; }
        public DateTime AdministeredDate { get; set; }
        public string AdministeredBy { get; set; }
        public string Route { get; set; }
        public string Site { get; set; }
        public decimal Dose { get; set; }
        public string DoseUnit { get; set; }
        public string LotNumber { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Manufacturer { get; set; }
        public string VisPublished { get; set; }
        public DateTime? VisPresented { get; set; }

        // Vaccine Details
        public int DoseNumber { get; set; }
        public int? SeriesNumber { get; set; }
        public string SeriesComplete { get; set; }
        public DateTime? NextDoseDate { get; set; }
        public string Funding { get; set; }
        public string FundingSource { get; set; }

        // Clinical
        public string ReasonCode { get; set; }
        public string ReasonDescription { get; set; }
        public bool IsHistorical { get; set; }
        public string InformationSource { get; set; }
        public bool RefusedFlag { get; set; }
        public string RefusalReason { get; set; }

        // Reactions
        public bool HasReaction { get; set; }
        public string ReactionType { get; set; }
        public string ReactionSeverity { get; set; }
        public DateTime? ReactionDate { get; set; }
        public string ReactionNotes { get; set; }

        // Registry Reporting
        public bool ReportedToRegistry { get; set; }
        public DateTime? RegistryReportDate { get; set; }
        public string RegistryStatus { get; set; }
        public string RegistryStatusReason { get; set; }

        // System
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual Patient Patient { get; set; }
    }
}
