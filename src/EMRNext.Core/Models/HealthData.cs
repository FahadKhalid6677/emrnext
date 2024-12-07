namespace EMRNext.Core.Models
{
    public class HealthData
    {
        public int Age { get; set; }
        public double BMI { get; set; }
        public int BloodPressure { get; set; }
        public int Cholesterol { get; set; }
        public string? Gender { get; set; }
        public bool SmokingHistory { get; set; }
        public bool DiabetesHistory { get; set; }
        public bool CardiovascularHistory { get; set; }
    }

    public class HealthRiskAssessment
    {
        public string RiskLevel { get; set; } = "Unknown";
        public double Score { get; set; }
        public List<string> RiskFactors { get; set; } = new();
        public string Recommendation { get; set; } = string.Empty;
    }
}
