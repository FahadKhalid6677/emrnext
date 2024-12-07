using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;

namespace EMRNext.Core.Security.Services
{
    /// <summary>
    /// Comprehensive authorization service for managing user permissions and access control
    /// </summary>
    public class AuthorizationService
    {
        private readonly ILogger<AuthorizationService> _logger;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Role> _roleRepository;

        public AuthorizationService(
            ILogger<AuthorizationService> logger,
            IGenericRepository<User> userRepository,
            IGenericRepository<Role> roleRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        /// <summary>
        /// Validate user access to a specific resource
        /// </summary>
        public async Task<bool> AuthorizeResourceAccessAsync(
            Guid userId, 
            string resourceType, 
            string operation)
        {
            try 
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User {userId} not found during authorization check");
                    return false;
                }

                var userRoles = await GetUserRolesAsync(userId);
                return userRoles.Any(role => 
                    HasPermissionForResource(role, resourceType, operation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Authorization error for user {userId}");
                return false;
            }
        }

        /// <summary>
        /// Get all roles for a specific user
        /// </summary>
        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Roles ?? Enumerable.Empty<Role>();
        }

        /// <summary>
        /// Check if a role has permission for a specific resource and operation
        /// </summary>
        private bool HasPermissionForResource(Role role, string resourceType, string operation)
        {
            return role.Permissions.Any(p => 
                p.ResourceType == resourceType && 
                p.AllowedOperations.Contains(operation));
        }

        /// <summary>
        /// Create a new role with specific permissions
        /// </summary>
        public async Task<Role> CreateRoleAsync(
            string roleName, 
            List<Permission> permissions)
        {
            var newRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                Permissions = permissions
            };

            await _roleRepository.AddAsync(newRole);
            await _roleRepository.SaveChangesAsync();

            _logger.LogInformation($"Created new role: {roleName}");
            return newRole;
        }

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        public async Task AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var role = await _roleRepository.GetByIdAsync(roleId);

            if (user == null || role == null)
            {
                throw new ArgumentException("User or Role not found");
            }

            user.Roles.Add(role);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation($"Assigned role {roleId} to user {userId}");
        }

        /// <summary>
        /// Audit user access attempts
        /// </summary>
        public async Task LogAccessAttemptAsync(
            Guid userId, 
            string resourceType, 
            string operation, 
            bool isAuthorized)
        {
            // In a real-world scenario, this would be logged to a secure audit database
            _logger.LogInformation(
                $"Access Attempt - User: {userId}, " +
                $"Resource: {resourceType}, " +
                $"Operation: {operation}, " +
                $"Authorized: {isAuthorized}");
        }
    }

    /// <summary>
    /// Represents a permission for a specific resource type
    /// </summary>
    public class Permission
    {
        public Guid Id { get; set; }
        public string ResourceType { get; set; }
        public List<string> AllowedOperations { get; set; } = new List<string>();
    }

    /// <summary>
    /// Predefined operation types for consistent permission checking
    /// </summary>
    public static class OperationType
    {
        public const string Read = "Read";
        public const string Write = "Write";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Execute = "Execute";
    }
}
