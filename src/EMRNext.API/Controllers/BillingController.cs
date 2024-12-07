using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Services;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Authorization;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EMRNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly ILoggingService _loggingService;

        public BillingController(IBillingService billingService, ILoggingService loggingService)
        {
            _billingService = billingService;
            _loggingService = loggingService;
        }

        [HttpPost("claims")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifyBilling)]
        public async Task<ActionResult<Claim>> CreateClaim([FromBody] Claim claim)
        {
            try
            {
                var result = await _billingService.CreateClaimAsync(claim);
                await _loggingService.LogAuditAsync("CreateClaim", 
                    $"Created claim for encounter {claim.EncounterId}", 
                    User.Identity.Name);
                return CreatedAtAction(nameof(GetClaim), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("CreateClaim", ex.Message, ex);
                return StatusCode(500, "An error occurred while creating the claim");
            }
        }

        [HttpGet("claims/{id}")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewBilling)]
        public async Task<ActionResult<Claim>> GetClaim(int id)
        {
            try
            {
                var claim = await _billingService.GetClaimAsync(id);
                if (claim == null)
                    return NotFound();

                await _loggingService.LogAuditAsync("GetClaim", $"Retrieved claim {id}", User.Identity.Name);
                return Ok(claim);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetClaim", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving the claim");
            }
        }

        [HttpPost("payments")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifyBilling)]
        public async Task<ActionResult<Payment>> ProcessPayment([FromBody] Payment payment)
        {
            try
            {
                var result = await _billingService.ProcessPaymentAsync(payment);
                await _loggingService.LogAuditAsync("ProcessPayment", 
                    $"Processed payment for account {payment.AccountId}", 
                    User.Identity.Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ProcessPayment", ex.Message, ex);
                return StatusCode(500, "An error occurred while processing the payment");
            }
        }

        [HttpGet("accounts/{accountId}/statement")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewBilling)]
        public async Task<ActionResult<Statement>> GenerateStatement(int accountId)
        {
            try
            {
                var statement = await _billingService.GenerateStatementAsync(accountId);
                await _loggingService.LogAuditAsync("GenerateStatement", 
                    $"Generated statement for account {accountId}", 
                    User.Identity.Name);
                return Ok(statement);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GenerateStatement", ex.Message, ex);
                return StatusCode(500, "An error occurred while generating the statement");
            }
        }

        [HttpGet("accounts/{accountId}/transactions")]
        [Authorize(Policy = EMRAuthorizationPolicies.ViewBilling)]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions(int accountId)
        {
            try
            {
                var transactions = await _billingService.GetTransactionsAsync(accountId);
                await _loggingService.LogAuditAsync("GetTransactions", 
                    $"Retrieved transactions for account {accountId}", 
                    User.Identity.Name);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("GetTransactions", ex.Message, ex);
                return StatusCode(500, "An error occurred while retrieving transactions");
            }
        }

        [HttpPost("insurance/verify")]
        [Authorize(Policy = EMRAuthorizationPolicies.ModifyBilling)]
        public async Task<ActionResult<InsuranceVerification>> VerifyInsurance([FromBody] InsuranceVerificationRequest request)
        {
            try
            {
                var result = await _billingService.VerifyInsuranceAsync(request);
                await _loggingService.LogAuditAsync("VerifyInsurance", 
                    $"Verified insurance for patient {request.PatientId}", 
                    User.Identity.Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("VerifyInsurance", ex.Message, ex);
                return StatusCode(500, "An error occurred while verifying insurance");
            }
        }
    }
}
