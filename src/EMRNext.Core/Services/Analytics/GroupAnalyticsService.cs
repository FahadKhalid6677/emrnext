using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Services.Clinical;
using EMRNext.Core.Services.Cache;

namespace EMRNext.Core.Services.Analytics
{
    public class GroupAnalyticsService : IGroupAnalyticsService
    {
        private readonly EMRNextDbContext _context;
        private readonly IClinicalAnalyticsService _clinicalAnalytics;
        private readonly IResourceAnalyticsService _resourceAnalytics;
        private readonly IQualityMetricsService _qualityMetrics;
        private readonly ICacheService _cacheService;
        private readonly IMetricsService _metricsService;

        public GroupAnalyticsService(
            EMRNextDbContext context,
            IClinicalAnalyticsService clinicalAnalytics,
            IResourceAnalyticsService resourceAnalytics,
            IQualityMetricsService qualityMetrics,
            ICacheService cacheService,
            IMetricsService metricsService)
        {
            _context = context;
            _clinicalAnalytics = clinicalAnalytics;
            _resourceAnalytics = resourceAnalytics;
            _qualityMetrics = qualityMetrics;
            _cacheService = cacheService;
            _metricsService = metricsService;
        }

        public async Task<SeriesAnalytics> GetSeriesAnalyticsAsync(
            int seriesId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var cacheKey = $"series_analytics_{seriesId}_{startDate}_{endDate}";
            var cachedResult = await _cacheService.GetAsync<SeriesAnalytics>(cacheKey);
            if (cachedResult != null)
                return cachedResult;

            var series = await _context.GroupSeries
                .Include(s => s.Sessions)
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.Id == seriesId);

            if (series == null)
                throw new NotFoundException("Series not found");

            var analytics = new SeriesAnalytics
            {
                SeriesId = seriesId,
                SeriesName = series.Name,
                DateRange = new DateRange
                {
                    StartDate = startDate ?? series.StartDate,
                    EndDate = endDate ?? series.EndDate
                },
                AttendanceMetrics = await CalculateAttendanceMetricsAsync(series, startDate, endDate),
                ProgressMetrics = await CalculateProgressMetricsAsync(series, startDate, endDate),
                OutcomeMetrics = await CalculateOutcomeMetricsAsync(series, startDate, endDate),
                ResourceMetrics = await CalculateResourceMetricsAsync(series, startDate, endDate),
                QualityMetrics = await CalculateQualityMetricsAsync(series, startDate, endDate)
            };

            await _cacheService.SetAsync(cacheKey, analytics, TimeSpan.FromHours(1));
            return analytics;
        }

        public async Task<ClinicalAnalytics> GetClinicalAnalyticsAsync(
            int seriesId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var cacheKey = $"clinical_analytics_{seriesId}_{startDate}_{endDate}";
            var cachedResult = await _cacheService.GetAsync<ClinicalAnalytics>(cacheKey);
            if (cachedResult != null)
                return cachedResult;

            var analytics = new ClinicalAnalytics
            {
                SeriesId = seriesId,
                DateRange = new DateRange
                {
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue
                },
                TreatmentEffectiveness = await CalculateTreatmentEffectivenessAsync(seriesId, startDate, endDate),
                GoalAchievement = await CalculateGoalAchievementAsync(seriesId, startDate, endDate),
                ProtocolCompliance = await CalculateProtocolComplianceAsync(seriesId, startDate, endDate),
                PopulationHealth = await CalculatePopulationHealthMetricsAsync(seriesId, startDate, endDate)
            };

            await _cacheService.SetAsync(cacheKey, analytics, TimeSpan.FromHours(1));
            return analytics;
        }

        public async Task<OperationalAnalytics> GetOperationalAnalyticsAsync(
            int? seriesId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var cacheKey = $"operational_analytics_{seriesId}_{startDate}_{endDate}";
            var cachedResult = await _cacheService.GetAsync<OperationalAnalytics>(cacheKey);
            if (cachedResult != null)
                return cachedResult;

            var analytics = new OperationalAnalytics
            {
                SeriesId = seriesId,
                DateRange = new DateRange
                {
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue
                },
                ResourceUtilization = await CalculateResourceUtilizationAsync(seriesId, startDate, endDate),
                ProviderEfficiency = await CalculateProviderEfficiencyAsync(seriesId, startDate, endDate),
                CapacityMetrics = await CalculateCapacityMetricsAsync(seriesId, startDate, endDate),
                FinancialMetrics = await CalculateFinancialMetricsAsync(seriesId, startDate, endDate)
            };

            await _cacheService.SetAsync(cacheKey, analytics, TimeSpan.FromHours(1));
            return analytics;
        }

        public async Task<DashboardData> GetAdministrativeDashboardAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var cacheKey = $"admin_dashboard_{startDate}_{endDate}";
            var cachedResult = await _cacheService.GetAsync<DashboardData>(cacheKey);
            if (cachedResult != null)
                return cachedResult;

            var dashboard = new DashboardData
            {
                DateRange = new DateRange
                {
                    StartDate = startDate ?? DateTime.Now.AddMonths(-1),
                    EndDate = endDate ?? DateTime.Now
                },
                CapacityTrends = await GetCapacityTrendsAsync(startDate, endDate),
                ResourceUsage = await GetResourceUsageAsync(startDate, endDate),
                FinancialMetrics = await GetFinancialMetricsAsync(startDate, endDate),
                SchedulingEfficiency = await GetSchedulingEfficiencyAsync(startDate, endDate),
                WaitlistStatus = await GetWaitlistStatusAsync(startDate, endDate)
            };

            await _cacheService.SetAsync(cacheKey, dashboard, TimeSpan.FromHours(1));
            return dashboard;
        }

        public async Task<DashboardData> GetClinicalDashboardAsync(
            int? providerId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var cacheKey = $"clinical_dashboard_{providerId}_{startDate}_{endDate}";
            var cachedResult = await _cacheService.GetAsync<DashboardData>(cacheKey);
            if (cachedResult != null)
                return cachedResult;

            var dashboard = new DashboardData
            {
                DateRange = new DateRange
                {
                    StartDate = startDate ?? DateTime.Now.AddMonths(-1),
                    EndDate = endDate ?? DateTime.Now
                },
                PatientProgress = await GetPatientProgressAsync(providerId, startDate, endDate),
                OutcomeTracking = await GetOutcomeTrackingAsync(providerId, startDate, endDate),
                ProtocolCompliance = await GetProtocolComplianceAsync(providerId, startDate, endDate),
                QualityMeasures = await GetQualityMeasuresAsync(providerId, startDate, endDate),
                TreatmentEffectiveness = await GetTreatmentEffectivenessAsync(providerId, startDate, endDate)
            };

            await _cacheService.SetAsync(cacheKey, dashboard, TimeSpan.FromHours(1));
            return dashboard;
        }

        private async Task<AttendanceMetrics> CalculateAttendanceMetricsAsync(
            GroupSeries series,
            DateTime? startDate,
            DateTime? endDate)
        {
            var sessions = series.Sessions
                .Where(s => (!startDate.HasValue || s.StartTime >= startDate) &&
                           (!endDate.HasValue || s.StartTime <= endDate));

            var metrics = new AttendanceMetrics
            {
                TotalSessions = sessions.Count(),
                CompletedSessions = sessions.Count(s => s.Status == "Completed"),
                AverageAttendance = await CalculateAverageAttendanceAsync(sessions),
                DropoutRate = await CalculateDropoutRateAsync(series),
                AttendancePatterns = await AnalyzeAttendancePatternsAsync(sessions)
            };

            return metrics;
        }

        private async Task<ProgressMetrics> CalculateProgressMetricsAsync(
            GroupSeries series,
            DateTime? startDate,
            DateTime? endDate)
        {
            var participants = series.Participants
                .Where(p => p.EnrollmentDate >= (startDate ?? DateTime.MinValue) &&
                           p.EnrollmentDate <= (endDate ?? DateTime.MaxValue));

            return new ProgressMetrics
            {
                AverageProgress = await CalculateAverageProgressAsync(participants),
                GoalAchievementRate = await CalculateGoalAchievementRateAsync(participants),
                MilestoneCompletion = await CalculateMilestoneCompletionAsync(participants),
                ProgressTrends = await AnalyzeProgressTrendsAsync(participants)
            };
        }

        private async Task<OutcomeMetrics> CalculateOutcomeMetricsAsync(
            GroupSeries series,
            DateTime? startDate,
            DateTime? endDate)
        {
            var outcomes = await _context.SeriesOutcomes
                .Where(o => o.GroupSeriesId == series.Id &&
                           o.MeasurementDate >= (startDate ?? DateTime.MinValue) &&
                           o.MeasurementDate <= (endDate ?? DateTime.MaxValue))
                .ToListAsync();

            return new OutcomeMetrics
            {
                ClinicalOutcomes = await AnalyzeClinicalOutcomesAsync(outcomes),
                PatientSatisfaction = await CalculatePatientSatisfactionAsync(series.Id),
                TreatmentEffectiveness = await AnalyzeTreatmentEffectivenessAsync(outcomes),
                LongTermOutcomes = await AnalyzeLongTermOutcomesAsync(series.Id)
            };
        }

        private async Task<ResourceMetrics> CalculateResourceMetricsAsync(
            GroupSeries series,
            DateTime? startDate,
            DateTime? endDate)
        {
            return new ResourceMetrics
            {
                ResourceUtilization = await CalculateResourceUtilizationAsync(series.Id),
                ProviderEfficiency = await CalculateProviderEfficiencyAsync(series.Id),
                SpaceUtilization = await CalculateSpaceUtilizationAsync(series.Id),
                CostEfficiency = await CalculateCostEfficiencyAsync(series.Id)
            };
        }

        private async Task<QualityMetrics> CalculateQualityMetricsAsync(
            GroupSeries series,
            DateTime? startDate,
            DateTime? endDate)
        {
            return new QualityMetrics
            {
                ProtocolAdherence = await CalculateProtocolAdherenceAsync(series.Id),
                DocumentationCompliance = await CalculateDocumentationComplianceAsync(series.Id),
                OutcomeAchievement = await CalculateOutcomeAchievementAsync(series.Id),
                PatientSatisfaction = await CalculateDetailedPatientSatisfactionAsync(series.Id)
            };
        }

        private async Task<TreatmentEffectiveness> CalculateTreatmentEffectivenessAsync(
            int seriesId,
            DateTime? startDate,
            DateTime? endDate)
        {
            return await _clinicalAnalytics.CalculateTreatmentEffectivenessAsync(
                seriesId,
                startDate,
                endDate);
        }

        private async Task<GoalAchievement> CalculateGoalAchievementAsync(
            int seriesId,
            DateTime? startDate,
            DateTime? endDate)
        {
            return await _clinicalAnalytics.CalculateGoalAchievementAsync(
                seriesId,
                startDate,
                endDate);
        }

        private async Task<ProtocolCompliance> CalculateProtocolComplianceAsync(
            int seriesId,
            DateTime? startDate,
            DateTime? endDate)
        {
            return await _clinicalAnalytics.CalculateProtocolComplianceAsync(
                seriesId,
                startDate,
                endDate);
        }

        private async Task<PopulationHealth> CalculatePopulationHealthMetricsAsync(
            int seriesId,
            DateTime? startDate,
            DateTime? endDate)
        {
            return await _clinicalAnalytics.CalculatePopulationHealthMetricsAsync(
                seriesId,
                startDate,
                endDate);
        }
    }
}
