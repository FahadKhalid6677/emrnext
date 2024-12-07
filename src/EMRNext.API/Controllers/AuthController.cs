using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Models;
using System.Security.Claims;

namespace EMRNext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenGenerationService _tokenService;
        private readonly IUserService _userService;

        public AuthController(
            ITokenGenerationService tokenService,
            IUserService userService)
        {
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.ValidateCredentialsAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userService.GetUserRolesAsync(user.Id);
            var token = await _tokenService.GenerateTokenAsync(user, roles);

            return Ok(token);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var token = await _tokenService.RefreshTokenAsync(request.Token);
                return Ok(token);
            }
            catch
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserProfile>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserProfile
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = await _userService.GetUserRolesAsync(user.Id)
            });
        }
    }
}
