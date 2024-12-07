using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities.Clinical;
using EMRNext.Core.Models.Clinical;

namespace EMRNext.Core.Services.Clinical
{
    public class LabOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LabOrderService> _logger;
        private readonly IClinicalAlertService _alertService;
        private readonly INotificationService _notificationService;

        public LabOrderService(
            ApplicationDbContext context,
            ILogger<LabOrderService> logger,
            IClinicalAlertService alertService,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _alertService = alertService;
            _notificationService = notificationService;
        }

        public async Task<LabOrderEntity> CreateLabOrderAsync(LabOrderRequest request)
        {
            try
            {
                var order = new LabOrderEntity
                {
                    PatientId = request.PatientId,
                    OrderingProviderId = request.ProviderId,
                    OrderDateTime = DateTime.UtcNow,
                    Priority = request.Priority,
                    Status = "Ordered",
                    ClinicalHistory = request.ClinicalHistory,
                    OrderDiagnosis = request.Diagnosis,
                    IsFasting = request.IsFasting,
                    SpecialInstructions = request.SpecialInstructions,
                    Tests = request.TestIds.Select(testId => new LabTestOrderEntity
                    {
                        TestId = testId,
                        Status = "Ordered"
                    }).ToList()
                };

                _context.LabOrders.Add(order);
                await _context.SaveChangesAsync();

                // Generate accession number
                order.AccessionNumber = GenerateAccessionNumber(order.Id);
                await _context.SaveChangesAsync();

                // Create notifications for STAT orders
                if (order.Priority == "STAT")
                {
                    await _notificationService.SendNotificationAsync(new Notification
                    {
                        Type = "STAT Lab Order",
                        Priority = "High",
                        Message = $"New STAT lab order {order.AccessionNumber}",
                        RecipientRoles = new[] { "Lab Technician", "Lab Manager" }
                    });
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lab order");
                throw;
            }
        }

        public async Task<bool> UpdateSpecimenCollectionAsync(SpecimenCollection collection)
        {
            try
            {
                var order = await _context.LabOrders
                    .Include(o => o.Tests)
                    .FirstOrDefaultAsync(o => o.Id == collection.OrderId);

                if (order == null)
                    return false;

                order.CollectionDateTime = collection.CollectionDateTime;
                order.CollectionSite = collection.CollectionSite;
                order.SpecimenType = collection.SpecimenType;
                order.Status = "Collected";

                foreach (var test in order.Tests)
                {
                    test.Status = "Collected";
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating specimen collection");
                return false;
            }
        }

        public async Task<bool> ProcessLabResultsAsync(List<LabResultInput> results)
        {
            try
            {
                foreach (var result in results)
                {
                    var testOrder = await _context.LabTestOrders
                        .Include(t => t.Test)
                        .Include(t => t.LabOrder)
                        .FirstOrDefaultAsync(t => t.Id == result.TestOrderId);

                    if (testOrder == null)
                        continue;

                    var labResult = new LabResultEntity
                    {
                        LabTestOrderId = testOrder.Id,
                        Value = result.Value,
                        Units = result.Units,
                        Status = result.Status,
                        ResultDateTime = DateTime.UtcNow,
                        PerformingTechnologistId = result.TechnologistId,
                        ValidatingProviderId = result.ValidatingProviderId,
                        Method = result.Method,
                        Equipment = result.Equipment,
                        Comments = result.Comments
                    };

                    // Evaluate result against reference ranges
                    await EvaluateResultAsync(labResult, testOrder);

                    _context.LabResults.Add(labResult);
                    
                    // Update test order status
                    testOrder.Status = "Completed";
                    testOrder.CompletionDateTime = DateTime.UtcNow;

                    // Check if all tests are complete
                    if (await AllTestsCompleteAsync(testOrder.LabOrderId))
                    {
                        testOrder.LabOrder.Status = "Completed";
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing lab results");
                return false;
            }
        }

        private async Task EvaluateResultAsync(LabResultEntity result, LabTestOrderEntity testOrder)
        {
            try
            {
                // Get patient age and gender
                var patient = await _context.PatientDemographics
                    .FirstOrDefaultAsync(p => p.PatientId == testOrder.LabOrder.PatientId);

                // Get applicable reference range
                var referenceRange = await _context.LabReferenceRanges
                    .Where(r => r.TestId == testOrder.TestId &&
                           (r.Gender == patient.Gender || r.Gender == "A") &&
                           (r.MinAge == null || patient.Age >= r.MinAge) &&
                           (r.MaxAge == null || patient.Age <= r.MaxAge))
                    .OrderByDescending(r => r.EffectiveDate)
                    .FirstOrDefaultAsync();

                if (referenceRange != null && decimal.TryParse(result.Value, out decimal value))
                {
                    // Evaluate result
                    if (decimal.TryParse(referenceRange.LowValue, out decimal low) &&
                        decimal.TryParse(referenceRange.HighValue, out decimal high))
                    {
                        if (value < low)
                            result.Flag = "Low";
                        else if (value > high)
                            result.Flag = "High";
                        else
                            result.Flag = "Normal";

                        // Check for critical values
                        if ((value < low * 0.75m) || (value > high * 1.25m))
                        {
                            result.IsCritical = true;
                            result.CriticalNotificationDateTime = DateTime.UtcNow;

                            // Create critical result alert
                            await _alertService.CreateAlertAsync(new ClinicalAlert
                            {
                                PatientId = testOrder.LabOrder.PatientId,
                                AlertType = "Critical Lab Result",
                                Severity = "High",
                                Message = $"Critical {testOrder.Test.Name} result: {result.Value} {result.Units}",
                                RequiresAcknowledgment = true
                            });

                            // Send notification
                            await _notificationService.SendNotificationAsync(new Notification
                            {
                                Type = "Critical Lab Result",
                                Priority = "High",
                                Message = $"Critical lab result for patient {testOrder.LabOrder.PatientId}",
                                RecipientIds = new[] { testOrder.LabOrder.OrderingProviderId }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating lab result");
                throw;
            }
        }

        private async Task<bool> AllTestsCompleteAsync(int orderId)
        {
            return await _context.LabTestOrders
                .Where(t => t.LabOrderId == orderId)
                .AllAsync(t => t.Status == "Completed");
        }

        private string GenerateAccessionNumber(int orderId)
        {
            // Format: YYYYMMDD-NNNNNN
            return $"{DateTime.UtcNow:yyyyMMdd}-{orderId:D6}";
        }

        public async Task<List<LabOrderEntity>> GetPatientLabOrdersAsync(
            int patientId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var query = _context.LabOrders
                .Include(o => o.Tests)
                    .ThenInclude(t => t.Test)
                .Include(o => o.Tests)
                    .ThenInclude(t => t.Results)
                .Where(o => o.PatientId == patientId);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDateTime >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDateTime <= endDate.Value);

            return await query
                .OrderByDescending(o => o.OrderDateTime)
                .ToListAsync();
        }

        public async Task<List<LabResultEntity>> GetLabResultsAsync(
            int patientId,
            string testCode = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.LabResults
                .Include(r => r.TestOrder)
                    .ThenInclude(t => t.Test)
                .Include(r => r.TestOrder)
                    .ThenInclude(t => t.LabOrder)
                .Where(r => r.TestOrder.LabOrder.PatientId == patientId);

            if (!string.IsNullOrEmpty(testCode))
                query = query.Where(r => r.TestOrder.Test.Code == testCode);

            if (startDate.HasValue)
                query = query.Where(r => r.ResultDateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.ResultDateTime <= endDate.Value);

            return await query
                .OrderByDescending(r => r.ResultDateTime)
                .ToListAsync();
        }
    }

    public class LabOrderRequest
    {
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public string Priority { get; set; }
        public string ClinicalHistory { get; set; }
        public string Diagnosis { get; set; }
        public bool IsFasting { get; set; }
        public string SpecialInstructions { get; set; }
        public List<int> TestIds { get; set; }
    }

    public class SpecimenCollection
    {
        public int OrderId { get; set; }
        public DateTime CollectionDateTime { get; set; }
        public string CollectionSite { get; set; }
        public string SpecimenType { get; set; }
    }

    public class LabResultInput
    {
        public int TestOrderId { get; set; }
        public string Value { get; set; }
        public string Units { get; set; }
        public string Status { get; set; }
        public int TechnologistId { get; set; }
        public int? ValidatingProviderId { get; set; }
        public string Method { get; set; }
        public string Equipment { get; set; }
        public string Comments { get; set; }
    }
}
