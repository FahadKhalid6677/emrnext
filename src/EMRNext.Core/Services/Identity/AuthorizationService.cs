using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EMRNext.Core.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EMRNext.Core.Services.Identity
{
    public class AuthorizationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AuthorizationService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // Create a new role with specific permissions
        public async Task<IdentityResult> CreateRoleAsync(string roleName, List<string> permissions)
        {
            try
            {
                // Check if role already exists
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    return IdentityResult.Failed(
                        new IdentityError { Description = "Role already exists." }
                    );
                }

                // Create new role
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);

                // Add permissions as claims if role creation is successful
                if (result.Succeeded)
                {
                    foreach (var permission in permissions)
                    {
                        await _roleManager.AddClaimAsync(role, 
                            new Claim(CustomClaimTypes.Permissions, permission));
                    }
                }

                _logger.LogInformation($"Role {roleName} created successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating role {roleName}");
                return IdentityResult.Failed(
                    new IdentityError { Description = ex.Message }
                );
            }
        }

        // Assign role to user
        public async Task<IdentityResult> AssignRoleToUserAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed(
                        new IdentityError { Description = "User not found." }
                    );
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {userId} assigned to role {roleName}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role {roleName} to user {userId}");
                return IdentityResult.Failed(
                    new IdentityError { Description = ex.Message }
                );
            }
        }

        // Check if user has specific permission
        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
        {
            try
            {
                var applicationUser = await _userManager.GetUserAsync(user);
                if (applicationUser == null)
                {
                    return false;
                }

                // Check user's direct permissions
                var userPermissions = await _userManager.GetClaimsAsync(applicationUser);
                if (userPermissions.Any(c => 
                    c.Type == CustomClaimTypes.Permissions && 
                    c.Value == permission))
                {
                    return true;
                }

                // Check roles permissions
                var userRoles = await _userManager.GetRolesAsync(applicationUser);
                foreach (var roleName in userRoles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    var roleClaims = await _roleManager.GetClaimsAsync(role);

                    if (roleClaims.Any(c => 
                        c.Type == CustomClaimTypes.Permissions && 
                        c.Value == permission))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permission {permission}");
                return false;
            }
        }

        // Revoke user's role
        public async Task<IdentityResult> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed(
                        new IdentityError { Description = "User not found." }
                    );
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Role {roleName} removed from user {userId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing role {roleName} from user {userId}");
                return IdentityResult.Failed(
                    new IdentityError { Description = ex.Message }
                );
            }
        }

        // Get user's roles and permissions
        public async Task<UserAuthorizationDetails> GetUserAuthorizationDetailsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userPermissions = await _userManager.GetClaimsAsync(user);
                var rolePermissions = new List<string>();

                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    rolePermissions.AddRange(
                        roleClaims
                            .Where(c => c.Type == CustomClaimTypes.Permissions)
                            .Select(c => c.Value)
                    );
                }

                return new UserAuthorizationDetails
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = roles.ToList(),
                    DirectPermissions = userPermissions
                        .Where(c => c.Type == CustomClaimTypes.Permissions)
                        .Select(c => c.Value)
                        .ToList(),
                    RolePermissions = rolePermissions.Distinct().ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving authorization details for user {userId}");
                return null;
            }
        }
    }

    // Authorization Details DTO
    public class UserAuthorizationDetails
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
        public List<string> DirectPermissions { get; set; }
        public List<string> RolePermissions { get; set; }
    }
}
