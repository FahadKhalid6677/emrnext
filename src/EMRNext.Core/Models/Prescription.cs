using System;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Enums;

namespace EMRNext.Core.Models
{
    /// <summary>
    /// Represents a medical prescription in the EMR system
    /// </summary>
    public class Prescription : BaseIntEntity
    {
        // Medication Details
        public string MedicationName { get; set; }
        public string GenericName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public int Quantity { get; set; }
        public string Instructions { get; set; }

        // Prescription Metadata
        public int PatientId { get; set; }
        public int PrescriberId { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Prescription Status
        public PrescriptionStatus Status { get; set; }
        public bool IsControlledSubstance { get; set; }
        public bool IsRepeat { get; set; }
        public int? RefillsRemaining { get; set; }

        // Prescription Tracking
        public List<PrescriptionFill> Fills { get; set; } = new List<PrescriptionFill>();

        // Domain Logic Methods
        public bool CanRefill() => 
            Status == PrescriptionStatus.Active && 
            RefillsRemaining > 0;

        public void Refill(string updatedBy)
        {
            if (!CanRefill())
                throw new InvalidOperationException("Prescription cannot be refilled");

            RefillsRemaining--;
            Update(updatedBy);
        }

        public void Discontinue(string reason, string updatedBy)
        {
            Status = PrescriptionStatus.Discontinued;
            Update(updatedBy);
        }

        public bool IsExpired() => 
            EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a prescription fill event
    /// </summary>
    public class PrescriptionFill : BaseIntEntity
    {
        public int PrescriptionId { get; set; }
        public DateTime FillDate { get; set; }
        public int QuantityDispensed { get; set; }
        public string PharmacyName { get; set; }
        public string PharmacistName { get; set; }
    }

    /// <summary>
    /// Prescription status enumeration
    /// </summary>
    public enum PrescriptionStatus
    {
        Active,
        Discontinued,
        Pending,
        Expired
    }
}
