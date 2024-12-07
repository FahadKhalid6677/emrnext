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
    public class InsuranceService : IInsuranceService
    {
        private readonly EMRNextDbContext _context;
        private readonly IEDIService _ediService;
        private readonly IDocumentService _documentService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IEligibilityService _eligibilityService;

        public InsuranceService(
            EMRNextDbContext context,
            IEDIService ediService,
            IDocumentService documentService,
            INotificationService notificationService,
            IAuditService auditService,
            IEligibilityService eligibilityService)
        {
            _context = context;
            _ediService = ediService;
            _documentService = documentService;
            _notificationService = notificationService;
            _auditService = auditService;
            _eligibilityService = eligibilityService;
        }

        public async Task<Insurance> CreateInsuranceAsync(InsuranceRequest request)
        {
            // Validate request
            await ValidateInsuranceRequestAsync(request);

            var insurance = new Insurance
            {
                PatientId = request.PatientId,
                PayerId = request.PayerId,
                PayerName = request.PayerName,
                PlanName = request.PlanName,
                PlanType = request.PlanType,
                PolicyNumber = request.PolicyNumber,
                GroupNumber = request.GroupNumber,
                SubscriberId = request.SubscriberId,
                SubscriberName = request.SubscriberName,
                EffectiveDate = request.EffectiveDate,
                TerminationDate = request.TerminationDate,
                Priority = request.Priority,
                IsActive = true,
                CoverageLevel = request.CoverageLevel,
                Copay = request.Copay,
                Deductible = request.Deductible,
                OutOfPocketMax = request.OutOfPocketMax,
                AuthorizationPhone = request.AuthorizationPhone,
                ClaimsPhone = request.ClaimsPhone,
                ClaimsAddress = request.ClaimsAddress,
                ElectronicPayerId = request.ElectronicPayerId
            };

            _context.Insurance.Add(insurance);
            await _context.SaveChangesAsync();

            // Create initial verification
            if (request.VerifyImmediately)
            {
                await VerifyInsuranceAsync(insurance.Id);
            }

            // Process documents
            if (request.Documents != null)
            {
                await CreateInsuranceDocumentsAsync(insurance, request.Documents);
            }

            // Audit trail
            await _auditService.LogActivityAsync(
                "InsuranceCreation",
                $"Created insurance for patient: {insurance.PatientId}",
                insurance);

            return insurance;
        }

        public async Task<InsuranceVerification> VerifyInsuranceAsync(int insuranceId)
        {
            var insurance = await GetInsuranceWithDetailsAsync(insuranceId);
            if (insurance == null)
                throw new NotFoundException("Insurance not found");

            // Generate EDI 270
            var edi270 = await GenerateEDI270Async(insuranceId);

            // Send EDI and get response
            var edi271 = await _ediService.SendEDI270Async(edi270);

            // Process EDI 271 response
            var verificationResult = await ProcessEDI271Async(edi271);

            var verification = new InsuranceVerification
            {
                InsuranceId = insuranceId,
                VerificationDate = DateTime.UtcNow,
                Method = "EDI",
                Status = verificationResult.Status,
                ResponseCode = verificationResult.ResponseCode,
                VerifiedBy = "System",
                Coverage = verificationResult.Coverage,
                CopayAmount = verificationResult.CopayAmount,
                DeductibleAmount = verificationResult.DeductibleAmount,
                DeductibleMet = verificationResult.DeductibleMet,
                OutOfPocketMax = verificationResult.OutOfPocketMax,
                OutOfPocketMet = verificationResult.OutOfPocketMet,
                Notes = verificationResult.Notes
            };

            _context.InsuranceVerifications.Add(verification);

            // Update insurance status
            insurance.IsVerified = true;
            insurance.LastVerificationDate = DateTime.UtcNow;
            insurance.VerificationMethod = "EDI";

            await _context.SaveChangesAsync();

            // Send notification
            await SendVerificationNotificationAsync(verification.Id);

            return verification;
        }

        public async Task<InsuranceAuthorization> RequestAuthorizationAsync(AuthorizationRequest request)
        {
            var insurance = await GetInsuranceWithDetailsAsync(request.InsuranceId);
            if (insurance == null)
                throw new NotFoundException("Insurance not found");

            // Validate service code
            await ValidateServiceCodeAsync(request.ServiceCode);

            // Generate EDI 278
            var edi278 = await GenerateEDI278Async(request);

            // Send EDI and get response
            var edi278Response = await _ediService.SendEDI278Async(edi278);

            // Process EDI 278 response
            var authResult = await ProcessEDI278ResponseAsync(edi278Response);

            var authorization = new InsuranceAuthorization
            {
                InsuranceId = request.InsuranceId,
                AuthorizationNumber = authResult.AuthorizationNumber,
                Type = request.Type,
                ServiceCode = request.ServiceCode,
                RequestDate = DateTime.UtcNow,
                ApprovalDate = authResult.IsApproved ? DateTime.UtcNow : null,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                UnitsApproved = authResult.UnitsApproved,
                UnitsUsed = 0,
                Status = authResult.Status,
                Notes = authResult.Notes
            };

            _context.InsuranceAuthorizations.Add(authorization);
            await _context.SaveChangesAsync();

            // Send notification
            await SendAuthorizationNotificationAsync(authorization.Id);

            return authorization;
        }

        public async Task<InsuranceReport> GenerateInsuranceReportAsync(int insuranceId)
        {
            var insurance = await GetInsuranceWithDetailsAsync(insuranceId);
            if (insurance == null)
                throw new NotFoundException("Insurance not found");

            var report = new InsuranceReport
            {
                InsuranceId = insurance.Id,
                PatientInfo = await GetPatientInfoAsync(insurance.PatientId),
                PayerInfo = new PayerInfo
                {
                    PayerId = insurance.PayerId,
                    PayerName = insurance.PayerName,
                    PlanName = insurance.PlanName,
                    PlanType = insurance.PlanType
                },
                PolicyInfo = new PolicyInfo
                {
                    PolicyNumber = insurance.PolicyNumber,
                    GroupNumber = insurance.GroupNumber,
                    SubscriberId = insurance.SubscriberId,
                    SubscriberName = insurance.SubscriberName,
                    EffectiveDate = insurance.EffectiveDate,
                    TerminationDate = insurance.TerminationDate
                },
                CoverageInfo = new CoverageInfo
                {
                    CoverageLevel = insurance.CoverageLevel,
                    Copay = insurance.Copay,
                    Deductible = insurance.Deductible,
                    DeductibleMet = insurance.DeductibleMet,
                    OutOfPocketMax = insurance.OutOfPocketMax,
                    OutOfPocketMet = insurance.OutOfPocketMet
                },
                Verifications = insurance.Verifications.Select(v => new VerificationInfo
                {
                    Date = v.VerificationDate,
                    Status = v.Status,
                    Method = v.Method,
                    VerifiedBy = v.VerifiedBy
                }).ToList(),
                Authorizations = insurance.Authorizations.Select(a => new AuthorizationInfo
                {
                    Number = a.AuthorizationNumber,
                    Type = a.Type,
                    ServiceCode = a.ServiceCode,
                    Status = a.Status,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    UnitsApproved = a.UnitsApproved,
                    UnitsUsed = a.UnitsUsed
                }).ToList()
            };

            return report;
        }

        private async Task ValidateInsuranceRequestAsync(InsuranceRequest request)
        {
            // Validate patient
            var patient = await _context.Patients.FindAsync(request.PatientId);
            if (patient == null)
                throw new ValidationException("Invalid patient");

            // Validate payer
            var payer = await _context.Payers.FindAsync(request.PayerId);
            if (payer == null)
                throw new ValidationException("Invalid payer");

            // Check for duplicate active insurance
            var existingInsurance = await _context.Insurance
                .Where(i => i.PatientId == request.PatientId &&
                           i.PayerId == request.PayerId &&
                           i.IsActive)
                .FirstOrDefaultAsync();

            if (existingInsurance != null)
                throw new ValidationException("Active insurance already exists for this payer");
        }

        private async Task<Insurance> GetInsuranceWithDetailsAsync(int insuranceId)
        {
            return await _context.Insurance
                .Include(i => i.Verifications)
                .Include(i => i.Authorizations)
                .Include(i => i.Documents)
                .FirstOrDefaultAsync(i => i.Id == insuranceId);
        }

        private async Task CreateInsuranceDocumentsAsync(
            Insurance insurance,
            IEnumerable<DocumentRequest> documents)
        {
            foreach (var doc in documents)
            {
                var document = new InsuranceDocument
                {
                    InsuranceId = insurance.Id,
                    DocumentType = doc.DocumentType,
                    Description = doc.Description,
                    UploadDate = DateTime.UtcNow,
                    UploadedBy = "System" // Replace with actual user
                };

                // Process document content
                document.DocumentPath = await _documentService.StoreDocumentAsync(
                    doc.Content,
                    doc.DocumentType);

                _context.InsuranceDocuments.Add(document);
            }

            await _context.SaveChangesAsync();
        }
    }
}
