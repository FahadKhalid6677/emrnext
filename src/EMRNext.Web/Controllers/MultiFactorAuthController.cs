using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Identity;
using EMRNext.Core.Identity.MultiFactorAuthentication;

namespace EMRNext.Web.Controllers
{
    /// <summary>
    /// Controller for Multi-Factor Authentication management
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MultiFactorAuthController : ControllerBase
    {
        private readonly MfaService _mfaService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MultiFactorAuthController(
            MfaService mfaService,
            UserManager<ApplicationUser> userManager)
        {
            _mfaService = mfaService ?? throw new ArgumentNullException(nameof(mfaService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <summary>
        /// Generate MFA setup details
        /// </summary>
        [HttpGet("setup")]
        public IActionResult GenerateMfaSetup()
        {
            var secretKey = _mfaService.GenerateMfaSecretKey();
            return Ok(new { SecretKey = secretKey });
        }

        /// <summary>
        /// Enable Multi-Factor Authentication
        /// </summary>
        [HttpPost("enable")]
        public async Task<IActionResult> EnableMfa([FromBody] MfaEnableRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
                return Unauthorized();

            var result = await _mfaService.EnableMfaAsync(user, request.SecretKey);

            if (result.Success)
            {
                return Ok(new 
                { 
                    BackupCodes = result.BackupCodes 
                });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        /// <summary>
        /// Disable Multi-Factor Authentication
        /// </summary>
        [HttpPost("disable")]
        public async Task<IActionResult> DisableMfa()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
                return Unauthorized();

            var result = await _mfaService.DisableMfaAsync(user);

            return result 
                ? Ok() 
                : BadRequest(new { Message = "Failed to disable MFA" });
        }

        /// <summary>
        /// Validate MFA code
        /// </summary>
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateMfaCode([FromBody] MfaValidationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
                return Unauthorized();

            var result = await _mfaService.ValidateMfaCodeAsync(user, request.Code);

            if (result.Success)
            {
                return Ok(new 
                { 
                    Message = result.UsedBackupCode 
                        ? "Backup code used successfully" 
                        : "MFA code validated" 
                });
            }

            return Unauthorized(new { Message = "Invalid MFA code" });
        }
    }

    /// <summary>
    /// Request model for enabling MFA
    /// </summary>
    public class MfaEnableRequest
    {
        public string SecretKey { get; set; }
    }

    /// <summary>
    /// Request model for MFA validation
    /// </summary>
    public class MfaValidationRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
