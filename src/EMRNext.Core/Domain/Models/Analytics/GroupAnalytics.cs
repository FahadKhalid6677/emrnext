using System;
using System.Collections.Generic;

namespace EMRNext.Core.Domain.Models.Analytics
{
    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SeriesAnalytics
    {
        public int SeriesId { get; set; }
        public string SeriesName { get; set; }
        public DateRange DateRange { get; set; }
        public AttendanceMetrics AttendanceMetrics { get; set; }
        public ProgressMetrics ProgressMetrics { get; set; }
        public OutcomeMetrics OutcomeMetrics { get; set; }
        public ResourceMetrics ResourceMetrics { get; set; }
        public QualityMetrics QualityMetrics { get; set; }
    }

    public class AttendanceMetrics
    {
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public double AverageAttendance { get; set; }
        public double DropoutRate { get; set; }
        public List<AttendancePattern> AttendancePatterns { get; set; }
    }

    public class AttendancePattern
    {
        public string PatternType { get; set; }
        public double Frequency { get; set; }
        public string Description { get; set; }
        public List<string> AffectedSessions { get; set; }
    }

    public class ProgressMetrics
    {
        public double AverageProgress { get; set; }
        public double GoalAchievementRate { get; set; }
        public List<MilestoneCompletion> MilestoneCompletion { get; set; }
        public List<ProgressTrend> ProgressTrends { get; set; }
    }

    public class MilestoneCompletion
    {
        public string Milestone { get; set; }
        public double CompletionRate { get; set; }
        public double AverageTimeToComplete { get; set; }
    }

    public class ProgressTrend
    {
        public DateTime Date { get; set; }
        public double AverageProgress { get; set; }
        public int ParticipantCount { get; set; }
    }

    public class OutcomeMetrics
    {
        public List<ClinicalOutcome> ClinicalOutcomes { get; set; }
        public double PatientSatisfaction { get; set; }
        public TreatmentEffectiveness TreatmentEffectiveness { get; set; }
        public List<LongTermOutcome> LongTermOutcomes { get; set; }
    }

    public class ClinicalOutcome
    {
        public string Measure { get; set; }
        public double BaselineValue { get; set; }
        public double CurrentValue { get; set; }
        public double ChangePercentage { get; set; }
        public string ClinicalSignificance { get; set; }
    }

    public class TreatmentEffectiveness
    {
        public double OverallEffectiveness { get; set; }
        public List<EffectivenessMetric> Metrics { get; set; }
        public List<SubgroupAnalysis> SubgroupAnalyses { get; set; }
    }

    public class EffectivenessMetric
    {
        public string Metric { get; set; }
        public double Value { get; set; }
        public string Interpretation { get; set; }
    }

    public class SubgroupAnalysis
    {
        public string Subgroup { get; set; }
        public double Effectiveness { get; set; }
        public string Significance { get; set; }
    }

    public class LongTermOutcome
    {
        public string Outcome { get; set; }
        public double AchievementRate { get; set; }
        public double Sustainability { get; set; }
        public List<string> ContributingFactors { get; set; }
    }

    public class ResourceMetrics
    {
        public ResourceUtilization ResourceUtilization { get; set; }
        public ProviderEfficiency ProviderEfficiency { get; set; }
        public SpaceUtilization SpaceUtilization { get; set; }
        public CostEfficiency CostEfficiency { get; set; }
    }

    public class ResourceUtilization
    {
        public double OverallUtilization { get; set; }
        public Dictionary<string, double> ResourceTypeUtilization { get; set; }
        public List<UtilizationTrend> Trends { get; set; }
    }

    public class UtilizationTrend
    {
        public DateTime Date { get; set; }
        public string ResourceType { get; set; }
        public double UtilizationRate { get; set; }
    }

    public class ProviderEfficiency
    {
        public double OverallEfficiency { get; set; }
        public double TimeUtilization { get; set; }
        public double PatientLoadBalance { get; set; }
        public List<ProviderMetric> Metrics { get; set; }
    }

    public class ProviderMetric
    {
        public string Metric { get; set; }
        public double Value { get; set; }
        public string Benchmark { get; set; }
    }

    public class SpaceUtilization
    {
        public double RoomUtilization { get; set; }
        public double CapacityUtilization { get; set; }
        public List<SpaceEfficiencyMetric> Metrics { get; set; }
    }

    public class SpaceEfficiencyMetric
    {
        public string Space { get; set; }
        public double Utilization { get; set; }
        public List<string> OptimizationOpportunities { get; set; }
    }

    public class CostEfficiency
    {
        public double CostPerSession { get; set; }
        public double CostPerParticipant { get; set; }
        public double ResourceCostEfficiency { get; set; }
        public List<CostMetric> DetailedMetrics { get; set; }
    }

    public class CostMetric
    {
        public string Category { get; set; }
        public double Cost { get; set; }
        public double Efficiency { get; set; }
        public List<string> OptimizationStrategies { get; set; }
    }

    public class QualityMetrics
    {
        public ProtocolAdherence ProtocolAdherence { get; set; }
        public DocumentationCompliance DocumentationCompliance { get; set; }
        public OutcomeAchievement OutcomeAchievement { get; set; }
        public DetailedPatientSatisfaction PatientSatisfaction { get; set; }
    }

    public class ProtocolAdherence
    {
        public double OverallAdherence { get; set; }
        public List<ProtocolMetric> Metrics { get; set; }
        public List<ComplianceIssue> Issues { get; set; }
    }

    public class ProtocolMetric
    {
        public string Protocol { get; set; }
        public double AdherenceRate { get; set; }
        public string Impact { get; set; }
    }

    public class ComplianceIssue
    {
        public string Issue { get; set; }
        public string Severity { get; set; }
        public List<string> RecommendedActions { get; set; }
    }

    public class DocumentationCompliance
    {
        public double OverallCompliance { get; set; }
        public Dictionary<string, double> DocumentTypeCompliance { get; set; }
        public List<DocumentationIssue> Issues { get; set; }
    }

    public class DocumentationIssue
    {
        public string Issue { get; set; }
        public string DocumentType { get; set; }
        public string Resolution { get; set; }
    }

    public class OutcomeAchievement
    {
        public double OverallAchievement { get; set; }
        public List<OutcomeMetric> Metrics { get; set; }
        public List<ImprovementOpportunity> Opportunities { get; set; }
    }

    public class OutcomeMetric
    {
        public string Outcome { get; set; }
        public double AchievementRate { get; set; }
        public string Significance { get; set; }
    }

    public class ImprovementOpportunity
    {
        public string Area { get; set; }
        public string Impact { get; set; }
        public List<string> Recommendations { get; set; }
    }

    public class DetailedPatientSatisfaction
    {
        public double OverallSatisfaction { get; set; }
        public Dictionary<string, double> CategorySatisfaction { get; set; }
        public List<FeedbackTrend> Trends { get; set; }
        public List<ImprovementArea> ImprovementAreas { get; set; }
    }

    public class FeedbackTrend
    {
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public double SatisfactionScore { get; set; }
    }

    public class ImprovementArea
    {
        public string Area { get; set; }
        public double CurrentScore { get; set; }
        public List<string> ImprovementStrategies { get; set; }
    }

    public class DashboardData
    {
        public DateRange DateRange { get; set; }
        public List<CapacityTrend> CapacityTrends { get; set; }
        public ResourceUsage ResourceUsage { get; set; }
        public FinancialMetrics FinancialMetrics { get; set; }
        public SchedulingEfficiency SchedulingEfficiency { get; set; }
        public WaitlistStatus WaitlistStatus { get; set; }
        public PatientProgress PatientProgress { get; set; }
        public OutcomeTracking OutcomeTracking { get; set; }
        public ProtocolCompliance ProtocolCompliance { get; set; }
        public QualityMeasures QualityMeasures { get; set; }
        public TreatmentEffectiveness TreatmentEffectiveness { get; set; }
    }
}
