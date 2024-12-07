using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Entities.Financial;
using EMRNext.Core.Domain.Models.Financial;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Services.Interface;
using EMRNext.Core.Services.Document;
using EMRNext.Core.Services.Notification;

namespace EMRNext.Core.Services.Financial
{
    public class ClaimService : IClaimService
    {
        private readonly EMRNextDbContext _context;
        private readonly IEDIService _ediService;
        private readonly IDocumentService _documentService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IInsuranceService _insuranceService;

        public ClaimService(
            EMRNextDbContext context,
            IEDIService ediService,
            IDocumentService documentService,
            INotificationService notificationService,
            IAuditService auditService,
            IInsuranceService insuranceService)
        {
            _context = context;
            _ediService = ediService;
            _documentService = documentService;
            _notificationService = notificationService;
            _auditService = auditService;
            _insuranceService = insuranceService;
        }

        public async Task<Claim> CreateClaimAsync(ClaimRequest request)
        {
            // Validate request
            await ValidateClaimRequestAsync(request);

            var claim = new Claim
            {
                ClaimNumber = await GenerateClaimNumberAsync(),
                PatientId = request.PatientId,
                ProviderId = request.ProviderId,
                EncounterId = request.EncounterId,
                ServiceDate = request.ServiceDate,
                FilingDate = DateTime.UtcNow,
                Status = "Draft",
                Type = request.Type,
                Priority = request.Priority,
                BillingProvider = request.BillingProvider,
                RenderingProvider = request.RenderingProvider,
                FacilityCode = request.FacilityCode,
                PlaceOfService = request.PlaceOfService,
                IsElectronic = request.IsElectronic,
                RequiresAuthorization = request.RequiresAuthorization
            };

            // Add claim items
            foreach (var item in request.Items)
            {
                var claimItem = new ClaimItem
                {
                    ServiceCode = item.ServiceCode,
                    Modifier = item.Modifier,
                    Description = item.Description,
                    UnitPrice = item.UnitPrice,
                    Units = item.Units,
                    TotalAmount = item.UnitPrice * item.Units,
                    ServiceDate = item.ServiceDate,
                    DiagnosisPointers = item.DiagnosisPointers,
                    RevenueCode = item.RevenueCode,
                    NDCCode = item.NDCCode,
                    Status = "Pending"
                };

                claim.Items.Add(claimItem);
            }

            // Calculate totals
            claim.TotalAmount = claim.Items.Sum(i => i.TotalAmount);

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Create initial documents
            if (request.Documents != null)
            {
                await CreateClaimDocumentsAsync(claim, request.Documents);
            }

            // Add to claim history
            await AddClaimHistoryAsync(claim.Id, "Created", "Claim created");

            // Audit trail
            await _auditService.LogActivityAsync(
                "ClaimCreation",
                $"Created claim: {claim.ClaimNumber}",
                claim);

            return claim;
        }

        public async Task<bool> SubmitClaimAsync(int claimId)
        {
            var claim = await GetClaimWithDetailsAsync(claimId);
            if (claim == null)
                throw new NotFoundException("Claim not found");

            // Validate claim before submission
            await ValidateClaimForSubmissionAsync(claim);

            // Check insurance eligibility
            await CheckEligibilityAsync(claimId);

            // Generate EDI if electronic claim
            if (claim.IsElectronic)
            {
                var ediContent = await GenerateEDI837Async(claimId);
                await _ediService.SubmitEDIAsync(ediContent);
            }

            claim.Status = "Submitted";
            claim.FilingDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Add to history
            await AddClaimHistoryAsync(claimId, "Submitted", "Claim submitted for processing");

            // Notify relevant parties
            await SendClaimNotificationAsync(claimId, "Submission");

            return true;
        }

        public async Task<bool> ProcessClaimResponseAsync(ClaimResponse response)
        {
            var claim = await GetClaimWithDetailsAsync(response.ClaimId);
            if (claim == null)
                throw new NotFoundException("Claim not found");

            // Update claim status
            claim.Status = response.Status;
            claim.AllowedAmount = response.AllowedAmount;
            claim.PaidAmount = response.PaidAmount;
            claim.PatientResponsibility = response.PatientResponsibility;

            // Update claim items
            foreach (var itemResponse in response.Items)
            {
                var claimItem = claim.Items.FirstOrDefault(i => i.Id == itemResponse.ClaimItemId);
                if (claimItem != null)
                {
                    claimItem.Status = itemResponse.Status;
                    claimItem.AllowedAmount = itemResponse.AllowedAmount;
                    claimItem.PaidAmount = itemResponse.PaidAmount;
                    claimItem.RejectionReason = itemResponse.RejectionReason;
                }
            }

            // Add adjustments if any
            if (response.Adjustments != null)
            {
                foreach (var adjustment in response.Adjustments)
                {
                    claim.Adjustments.Add(new ClaimAdjustment
                    {
                        Type = adjustment.Type,
                        Reason = adjustment.Reason,
                        Amount = adjustment.Amount,
                        AdjustmentDate = DateTime.UtcNow,
                        ProcessedBy = adjustment.ProcessedBy
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Add to history
            await AddClaimHistoryAsync(claim.Id, "Response", "Processed claim response");

            // Send notifications
            await SendClaimNotificationAsync(claim.Id, "Response");

            return true;
        }

        public async Task<ClaimReport> GenerateClaimReportAsync(int claimId)
        {
            var claim = await GetClaimWithDetailsAsync(claimId);
            if (claim == null)
                throw new NotFoundException("Claim not found");

            var report = new ClaimReport
            {
                ClaimNumber = claim.ClaimNumber,
                PatientInfo = await GetPatientInfoAsync(claim.PatientId),
                ProviderInfo = await GetProviderInfoAsync(claim.ProviderId),
                ServiceDate = claim.ServiceDate,
                FilingDate = claim.FilingDate,
                Status = claim.Status,
                TotalAmount = claim.TotalAmount,
                AllowedAmount = claim.AllowedAmount,
                PaidAmount = claim.PaidAmount,
                PatientResponsibility = claim.PatientResponsibility,
                Items = claim.Items.Select(i => new ClaimItemReport
                {
                    ServiceCode = i.ServiceCode,
                    Description = i.Description,
                    Units = i.Units,
                    UnitPrice = i.UnitPrice,
                    TotalAmount = i.TotalAmount,
                    AllowedAmount = i.AllowedAmount,
                    PaidAmount = i.PaidAmount,
                    Status = i.Status
                }).ToList(),
                Adjustments = claim.Adjustments.Select(a => new AdjustmentReport
                {
                    Type = a.Type,
                    Reason = a.Reason,
                    Amount = a.Amount,
                    Date = a.AdjustmentDate
                }).ToList(),
                History = claim.History.Select(h => new ClaimHistoryReport
                {
                    Action = h.Action,
                    Description = h.Description,
                    Date = h.ActionDate,
                    User = h.ActionBy
                }).ToList()
            };

            return report;
        }

        private async Task ValidateClaimRequestAsync(ClaimRequest request)
        {
            // Validate patient
            var patient = await _context.Patients.FindAsync(request.PatientId);
            if (patient == null)
                throw new ValidationException("Invalid patient");

            // Validate provider
            var provider = await _context.Providers.FindAsync(request.ProviderId);
            if (provider == null)
                throw new ValidationException("Invalid provider");

            // Validate encounter if provided
            if (request.EncounterId.HasValue)
            {
                var encounter = await _context.Encounters.FindAsync(request.EncounterId.Value);
                if (encounter == null)
                    throw new ValidationException("Invalid encounter");
            }

            // Validate service codes
            foreach (var item in request.Items)
            {
                await ValidateServiceCodeAsync(item.ServiceCode);
            }

            // Validate insurance if needed
            if (request.RequiresAuthorization)
            {
                await ValidateAuthorizationAsync(request);
            }
        }

        private async Task<string> GenerateClaimNumberAsync()
        {
            var prefix = "CLM";
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var sequence = await _context.Claims
                .Where(c => c.ClaimNumber.StartsWith($"{prefix}{date}"))
                .CountAsync() + 1;

            return $"{prefix}{date}{sequence:D4}";
        }

        private async Task AddClaimHistoryAsync(int claimId, string action, string description)
        {
            var history = new ClaimHistory
            {
                ClaimId = claimId,
                Action = action,
                Description = description,
                ActionDate = DateTime.UtcNow,
                ActionBy = "System" // Replace with actual user
            };

            _context.ClaimHistory.Add(history);
            await _context.SaveChangesAsync();
        }

        private async Task<Claim> GetClaimWithDetailsAsync(int claimId)
        {
            return await _context.Claims
                .Include(c => c.Items)
                .Include(c => c.Adjustments)
                .Include(c => c.Documents)
                .Include(c => c.History)
                .FirstOrDefaultAsync(c => c.Id == claimId);
        }

        public async Task<string> ProcessEDI999Async(string ediFile)
        {
            try 
            {
                // Implement EDI 999 processing logic
                var result = await _ediService.ProcessEDI999Async(ediFile);
                await _auditService.LogEDIProcessingAsync(ediFile, "EDI999");
                return result;
            }
            catch (Exception ex)
            {
                await _notificationService.SendSystemAlertAsync($"EDI 999 Processing Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ProcessClaimBatchAsync(IEnumerable<ClaimRequest> claims)
        {
            var processedClaims = new List<Claim>();
            foreach (var claimRequest in claims)
            {
                var claim = await CreateClaimAsync(claimRequest);
                await SubmitClaimAsync(claim.Id);
                processedClaims.Add(claim);
            }
            
            await _notificationService.SendBatchProcessingNotificationAsync(processedClaims.Count);
            return true;
        }

        public async Task<bool> ProcessPaymentBatchAsync(IEnumerable<ClaimPayment> payments)
        {
            foreach (var payment in payments)
            {
                await ProcessClaimPaymentAsync(payment);
            }
            
            await _notificationService.SendPaymentBatchNotificationAsync(payments.Count());
            return true;
        }

        public async Task<string> ProcessEDIBatchAsync(string ediBatchFile)
        {
            try 
            {
                var result = await _ediService.ProcessEDIBatchAsync(ediBatchFile);
                await _auditService.LogEDIProcessingAsync(ediBatchFile, "EDIBatch");
                return result;
            }
            catch (Exception ex)
            {
                await _notificationService.SendSystemAlertAsync($"EDI Batch Processing Error: {ex.Message}");
                throw;
            }
        }

        public async Task SendClaimNotificationAsync(int claimId, string notificationType)
        {
            var claim = await GetClaimAsync(claimId);
            await _notificationService.SendClaimNotificationAsync(claim, notificationType);
        }

        public async Task SendPaymentNotificationAsync(int claimId)
        {
            var claim = await GetClaimAsync(claimId);
            await _notificationService.SendPaymentNotificationAsync(claim);
        }

        public async Task SendDenialNotificationAsync(int claimId)
        {
            var claim = await GetClaimAsync(claimId);
            await _notificationService.SendDenialNotificationAsync(claim);
        }

        public async Task<AnalyticsReport> GenerateAnalyticsReportAsync(AnalyticsRequest request)
        {
            // Implement analytics report generation
            var claims = await _context.Claims
                .Where(c => c.ServiceDate >= request.StartDate && c.ServiceDate <= request.EndDate)
                .ToListAsync();
            
            return new AnalyticsReport 
            {
                TotalClaims = claims.Count,
                TotalAmount = claims.Sum(c => c.TotalAmount),
                // Add more analytics calculations
            };
        }

        public async Task<IEnumerable<TrendReport>> GetTrendReportsAsync(DateTime startDate, DateTime endDate)
        {
            var claims = await _context.Claims
                .Where(c => c.ServiceDate >= startDate && c.ServiceDate <= endDate)
                .GroupBy(c => c.ServiceDate.Month)
                .Select(g => new TrendReport 
                {
                    Month = g.Key,
                    TotalClaims = g.Count(),
                    TotalAmount = g.Sum(c => c.TotalAmount)
                })
                .ToListAsync();
            
            return claims;
        }

        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            var totalClaims = await _context.Claims.CountAsync();
            var processedClaims = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Processed);
            var deniedClaims = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Denied);
            
            return new PerformanceMetrics 
            {
                TotalClaims = totalClaims,
                ProcessedClaimsPercentage = (double)processedClaims / totalClaims * 100,
                DeniedClaimsPercentage = (double)deniedClaims / totalClaims * 100
            };
        }
    }
}
