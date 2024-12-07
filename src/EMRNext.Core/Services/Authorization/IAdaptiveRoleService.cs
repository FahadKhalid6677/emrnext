using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities.Authorization;

namespace EMRNext.Core.Services.Authorization
{
    /// <summary>
    /// Service for managing adaptive roles with machine learning capabilities
    /// </summary>
    public interface IAdaptiveRoleService
    {
        /// <summary>
        /// Create a new adaptive role
        /// </summary>
        Task<AdaptiveRole> CreateRoleAsync(AdaptiveRole role);

        /// <summary>
        /// Update an existing role
        /// </summary>
        Task<AdaptiveRole> UpdateRoleAsync(AdaptiveRole role);

        /// <summary>
        /// Get role by ID
        /// </summary>
        Task<AdaptiveRole> GetRoleByIdAsync(Guid roleId);

        /// <summary>
        /// Get all roles
        /// </summary>
        Task<IEnumerable<AdaptiveRole>> GetAllRolesAsync();

        /// <summary>
        /// Add a permission to a role
        /// </summary>
        Task<AdaptivePermission> AddPermissionToRoleAsync(Guid roleId, AdaptivePermission permission);

        /// <summary>
        /// Remove a permission from a role
        /// </summary>
        Task RemovePermissionFromRoleAsync(Guid permissionId);

        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        Task<bool> HasPermissionAsync(Guid userId, string permissionName);

        /// <summary>
        /// Evaluate access based on context
        /// </summary>
        Task<bool> EvaluateAccessAsync(Guid userId, string resourceType, string action);

        /// <summary>
        /// Train role permissions based on historical access patterns
        /// </summary>
        Task TrainRolePermissionsAsync();
    }
}
