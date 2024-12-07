using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMRNext.Core.Domain.Entities.Authorization
{
    /// <summary>
    /// Represents an adaptive role with dynamic permission management
    /// </summary>
    public class AdaptiveRole
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Indicates the role's base security level
        /// </summary>
        public SecurityLevel BaseSecurityLevel { get; set; }

        /// <summary>
        /// Machine learning confidence score for role permissions
        /// </summary>
        public double MachineLearningConfidence { get; set; }

        /// <summary>
        /// Dynamically adjustable permissions
        /// </summary>
        public List<AdaptivePermission> Permissions { get; set; }

        /// <summary>
        /// Context-aware access rules
        /// </summary>
        public List<AccessRule> AccessRules { get; set; }

        /// <summary>
        /// Temporal validity of the role
        /// </summary>
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// Indicates if the role is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Represents a dynamic permission with adaptive characteristics
    /// </summary>
    public class AdaptivePermission
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public AdaptiveRole Role { get; set; }

        [Required]
        [StringLength(100)]
        public string PermissionName { get; set; }

        /// <summary>
        /// Granular permission level
        /// </summary>
        public PermissionLevel Level { get; set; }

        /// <summary>
        /// Machine learning confidence for this specific permission
        /// </summary>
        public double MachineLearningConfidence { get; set; }

        /// <summary>
        /// Contextual conditions for permission
        /// </summary>
        public List<PermissionCondition> Conditions { get; set; }
    }

    /// <summary>
    /// Represents a contextual access rule
    /// </summary>
    public class AccessRule
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public AdaptiveRole Role { get; set; }

        [Required]
        [StringLength(100)]
        public string RuleName { get; set; }

        /// <summary>
        /// JSON-serialized rule conditions
        /// </summary>
        public string Conditions { get; set; }

        /// <summary>
        /// Priority of the rule
        /// </summary>
        public int Priority { get; set; }
    }

    /// <summary>
    /// Represents a condition for a permission
    /// </summary>
    public class PermissionCondition
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PermissionId { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public AdaptivePermission Permission { get; set; }

        [Required]
        [StringLength(100)]
        public string ConditionType { get; set; }

        [Required]
        public string ConditionValue { get; set; }
    }

    /// <summary>
    /// Defines the base security level for roles
    /// </summary>
    public enum SecurityLevel
    {
        Minimal = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Defines granular permission levels
    /// </summary>
    public enum PermissionLevel
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 3,
        FullControl = 4
    }
}
