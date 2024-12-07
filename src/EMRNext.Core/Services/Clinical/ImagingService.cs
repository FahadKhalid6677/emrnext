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
    public class ImagingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImagingService> _logger;
        private readonly IClinicalAlertService _alertService;
        private readonly INotificationService _notificationService;
        private readonly IPacsIntegrationService _pacsService;

        public ImagingService(
            ApplicationDbContext context,
            ILogger<ImagingService> logger,
            IClinicalAlertService alertService,
            INotificationService notificationService,
            IPacsIntegrationService pacsService)
        {
            _context = context;
            _logger = logger;
            _alertService = alertService;
            _notificationService = notificationService;
            _pacsService = pacsService;
        }

        public async Task<ImagingOrderEntity> CreateImagingOrderAsync(ImagingOrderRequest request)
        {
            try
            {
                var order = new ImagingOrderEntity
                {
                    PatientId = request.PatientId,
                    OrderingProviderId = request.ProviderId,
                    OrderDateTime = DateTime.UtcNow,
                    Priority = request.Priority,
                    Status = "Ordered",
                    Modality = request.Modality,
                    StudyType = request.StudyType,
                    BodyPart = request.BodyPart,
                    Laterality = request.Laterality,
                    ClinicalHistory = request.ClinicalHistory,
                    OrderDiagnosis = request.Diagnosis,
                    SpecialInstructions = request.SpecialInstructions,
                    Protocol = request.Protocol,
                    ScheduledDateTime = request.ScheduledDateTime
                };

                _context.ImagingOrders.Add(order);
                await _context.SaveChangesAsync();

                // Generate accession number
                order.AccessionNumber = GenerateAccessionNumber(order.Id);
                await _context.SaveChangesAsync();

                // Create notifications for STAT orders
                if (order.Priority == "STAT")
                {
                    await _notificationService.SendNotificationAsync(new Notification
                    {
                        Type = "STAT Imaging Order",
                        Priority = "High",
                        Message = $"New STAT imaging order {order.AccessionNumber}",
                        RecipientRoles = new[] { "Radiologist", "Radiology Technologist" }
                    });
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating imaging order");
                throw;
            }
        }

        public async Task<bool> ProcessImagingResultAsync(ImagingResultInput result)
        {
            try
            {
                var order = await _context.ImagingOrders
                    .FirstOrDefaultAsync(o => o.Id == result.OrderId);

                if (order == null)
                    return false;

                var imagingResult = new ImagingResultEntity
                {
                    ImagingOrderId = order.Id,
                    StudyDateTime = result.StudyDateTime,
                    Status = result.Status,
                    Findings = result.Findings,
                    Impression = result.Impression,
                    Technique = result.Technique,
                    ComparisonStudies = result.ComparisonStudies,
                    IsCritical = result.IsCritical,
                    RadiologistId = result.RadiologistId,
                    ReportDateTime = DateTime.UtcNow
                };

                if (result.IsCritical)
                {
                    imagingResult.CriticalNotificationDateTime = DateTime.UtcNow;
                    imagingResult.NotifiedProviderId = order.OrderingProviderId;

                    // Create critical result alert
                    await _alertService.CreateAlertAsync(new ClinicalAlert
                    {
                        PatientId = order.PatientId,
                        AlertType = "Critical Imaging Finding",
                        Severity = "High",
                        Message = $"Critical finding in {order.Modality} study: {result.Impression}",
                        RequiresAcknowledgment = true
                    });

                    // Send notification
                    await _notificationService.SendNotificationAsync(new Notification
                    {
                        Type = "Critical Imaging Finding",
                        Priority = "High",
                        Message = $"Critical imaging finding for patient {order.PatientId}",
                        RecipientIds = new[] { order.OrderingProviderId }
                    });
                }

                _context.ImagingResults.Add(imagingResult);
                order.Status = "Completed";
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing imaging result");
                return false;
            }
        }

        public async Task<bool> ProcessDicomStudyAsync(DicomStudyInput study)
        {
            try
            {
                var result = await _context.ImagingResults
                    .Include(r => r.Order)
                    .FirstOrDefaultAsync(r => r.Id == study.ResultId);

                if (result == null)
                    return false;

                var imagingStudy = new ImagingStudyEntity
                {
                    ImagingResultId = result.Id,
                    StudyInstanceUid = study.StudyInstanceUid,
                    AccessionNumber = result.Order.AccessionNumber,
                    Modality = study.Modality,
                    StudyDateTime = study.StudyDateTime,
                    Description = study.Description,
                    NumberOfSeries = study.Series.Count,
                    NumberOfInstances = study.Series.Sum(s => s.Instances.Count),
                    StoragePath = study.StoragePath,
                    StorageSize = study.StorageSize,
                    PacsStatus = "Stored",
                    Series = study.Series.Select(s => new ImagingSeriesEntity
                    {
                        SeriesInstanceUid = s.SeriesInstanceUid,
                        Number = s.Number,
                        Modality = s.Modality,
                        Description = s.Description,
                        BodyPart = s.BodyPart,
                        Laterality = s.Laterality,
                        SeriesDateTime = s.SeriesDateTime,
                        NumberOfInstances = s.Instances.Count,
                        StoragePath = s.StoragePath,
                        Instances = s.Instances.Select(i => new ImagingInstanceEntity
                        {
                            SopInstanceUid = i.SopInstanceUid,
                            Number = i.Number,
                            InstanceDateTime = i.InstanceDateTime,
                            StoragePath = i.StoragePath,
                            StorageSize = i.StorageSize,
                            DicomMetadata = i.DicomMetadata
                        }).ToList()
                    }).ToList()
                };

                _context.ImagingStudies.Add(imagingStudy);
                await _context.SaveChangesAsync();

                // Send to PACS
                await _pacsService.StoreDicomStudyAsync(imagingStudy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DICOM study");
                return false;
            }
        }

        private string GenerateAccessionNumber(int orderId)
        {
            // Format: YYYYMMDD-NNNNNN
            return $"{DateTime.UtcNow:yyyyMMdd}-{orderId:D6}";
        }

        public async Task<List<ImagingOrderEntity>> GetPatientImagingOrdersAsync(
            int patientId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.ImagingOrders
                .Include(o => o.Results)
                    .ThenInclude(r => r.Studies)
                .Where(o => o.PatientId == patientId);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDateTime <= endDate.Value);

            return await query
                .OrderByDescending(o => o.OrderDateTime)
                .ToListAsync();
        }

        public async Task<ImagingStudyEntity> GetImagingStudyAsync(string studyInstanceUid)
        {
            return await _context.ImagingStudies
                .Include(s => s.Series)
                    .ThenInclude(s => s.Instances)
                .Include(s => s.Result)
                    .ThenInclude(r => r.Order)
                .FirstOrDefaultAsync(s => s.StudyInstanceUid == studyInstanceUid);
        }
    }

    public class ImagingOrderRequest
    {
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public string Priority { get; set; }
        public string Modality { get; set; }
        public string StudyType { get; set; }
        public string BodyPart { get; set; }
        public string Laterality { get; set; }
        public string ClinicalHistory { get; set; }
        public string Diagnosis { get; set; }
        public string SpecialInstructions { get; set; }
        public string Protocol { get; set; }
        public DateTime? ScheduledDateTime { get; set; }
    }

    public class ImagingResultInput
    {
        public int OrderId { get; set; }
        public DateTime StudyDateTime { get; set; }
        public string Status { get; set; }
        public string Findings { get; set; }
        public string Impression { get; set; }
        public string Technique { get; set; }
        public string ComparisonStudies { get; set; }
        public bool IsCritical { get; set; }
        public int RadiologistId { get; set; }
    }

    public class DicomStudyInput
    {
        public int ResultId { get; set; }
        public string StudyInstanceUid { get; set; }
        public string Modality { get; set; }
        public DateTime StudyDateTime { get; set; }
        public string Description { get; set; }
        public string StoragePath { get; set; }
        public long StorageSize { get; set; }
        public List<DicomSeriesInput> Series { get; set; }
    }

    public class DicomSeriesInput
    {
        public string SeriesInstanceUid { get; set; }
        public string Number { get; set; }
        public string Modality { get; set; }
        public string Description { get; set; }
        public string BodyPart { get; set; }
        public string Laterality { get; set; }
        public DateTime SeriesDateTime { get; set; }
        public string StoragePath { get; set; }
        public List<DicomInstanceInput> Instances { get; set; }
    }

    public class DicomInstanceInput
    {
        public string SopInstanceUid { get; set; }
        public string Number { get; set; }
        public DateTime InstanceDateTime { get; set; }
        public string StoragePath { get; set; }
        public long StorageSize { get; set; }
        public string DicomMetadata { get; set; }
    }
}
