using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Services;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Monitoring
{
    public class BusinessMetricsCollector
    {
        private readonly IMonitoringService _monitoring;
        private readonly IClinicalService _clinicalService;
        private readonly ISchedulingService _schedulingService;
        private readonly IBillingService _billingService;
        private readonly ILogger<BusinessMetricsCollector> _logger;

        public BusinessMetricsCollector(
            IMonitoringService monitoring,
            IClinicalService clinicalService,
            ISchedulingService schedulingService,
            IBillingService billingService,
            ILogger<BusinessMetricsCollector> logger)
        {
            _monitoring = monitoring;
            _clinicalService = clinicalService;
            _schedulingService = schedulingService;
            _billingService = billingService;
            _logger = logger;
        }

        public async Task CollectMetricsAsync()
        {
            try
            {
                await CollectClinicalMetricsAsync();
                await CollectSchedulingMetricsAsync();
                await CollectBillingMetricsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting business metrics");
                await _monitoring.AlertAsync(AlertLevel.Error, "Failed to collect business metrics", ex);
            }
        }

        private async Task CollectClinicalMetricsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var metrics = await _clinicalService.GetDailyMetricsAsync(today);

                _monitoring.RecordMetric(
                    "clinical.encounters.daily",
                    metrics.EncounterCount,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                _monitoring.RecordMetric(
                    "clinical.orders.daily",
                    metrics.OrderCount,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                _monitoring.RecordMetric(
                    "clinical.notes.daily",
                    metrics.NoteCount,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                // Monitor clinical alerts
                var activeAlerts = await _clinicalService.GetActiveAlertsCountAsync();
                _monitoring.RecordMetric("clinical.alerts.active", activeAlerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting clinical metrics");
                throw;
            }
        }

        private async Task CollectSchedulingMetricsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var metrics = await _schedulingService.GetDailyMetricsAsync(today);

                _monitoring.RecordMetric(
                    "scheduling.appointments.daily",
                    metrics.AppointmentCount,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                _monitoring.RecordMetric(
                    "scheduling.cancellations.daily",
                    metrics.CancellationCount,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                _monitoring.RecordMetric(
                    "scheduling.noshow.daily",
                    metrics.NoShowCount,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                // Monitor scheduling efficiency
                var utilization = await _schedulingService.GetProviderUtilizationAsync(today);
                _monitoring.RecordMetric("scheduling.utilization", utilization * 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting scheduling metrics");
                throw;
            }
        }

        private async Task CollectBillingMetricsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var metrics = await _billingService.GetDailyMetricsAsync(today);

                _monitoring.RecordMetric(
                    "billing.claims.submitted",
                    metrics.ClaimsSubmitted,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                _monitoring.RecordMetric(
                    "billing.claims.pending",
                    metrics.ClaimsPending,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                _monitoring.RecordMetric(
                    "billing.payments.received",
                    metrics.PaymentsReceived,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                _monitoring.RecordMetric(
                    "billing.payments.amount",
                    metrics.PaymentAmount,
                    new KeyValuePair<string, object>("date", today.ToString("yyyy-MM-dd"))
                );

                // Monitor aging accounts receivable
                var arMetrics = await _billingService.GetAccountsReceivableMetricsAsync();
                _monitoring.RecordMetric("billing.ar.0_30", arMetrics.Days0To30);
                _monitoring.RecordMetric("billing.ar.31_60", arMetrics.Days31To60);
                _monitoring.RecordMetric("billing.ar.61_90", arMetrics.Days61To90);
                _monitoring.RecordMetric("billing.ar.90_plus", arMetrics.Days90Plus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting billing metrics");
                throw;
            }
        }
    }
}
