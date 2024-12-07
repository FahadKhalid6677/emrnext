using System;

namespace EMRNext.Core.Domain.Entities
{
    public class Vital
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int EncounterId { get; set; }
        public DateTime Date { get; set; }
        
        // Vital Signs
        public decimal? Temperature { get; set; }
        public string TemperatureUnit { get; set; } // F or C
        public decimal? Pulse { get; set; }
        public decimal? RespiratoryRate { get; set; }
        public decimal? BloodPressureSystolic { get; set; }
        public decimal? BloodPressureDiastolic { get; set; }
        public decimal? BloodPressurePosition { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public decimal? InhaledOxygenConcentration { get; set; }
        public decimal? Height { get; set; }
        public string HeightUnit { get; set; } // in or cm
        public decimal? Weight { get; set; }
        public string WeightUnit { get; set; } // lbs or kg
        public decimal? BMI { get; set; }
        public decimal? WaistCircumference { get; set; }
        public string WaistCircumferenceUnit { get; set; }
        public decimal? HeadCircumference { get; set; }
        public string HeadCircumferenceUnit { get; set; }
        public string PulseRhythm { get; set; }
        public string PulseLocation { get; set; }
        public string Notes { get; set; }
        
        // Navigation Properties
        public virtual Patient Patient { get; set; }
        public virtual Encounter Encounter { get; set; }
    }
}
