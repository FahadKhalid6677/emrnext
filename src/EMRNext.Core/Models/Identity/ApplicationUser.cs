using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace EMRNext.Core.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Extended User Properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public string PreferredLanguage { get; set; }

        // Professional Information
        public UserType UserType { get; set; }
        public string ProfessionalId { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }

        // Compliance and Security
        public bool RequirePasswordReset { get; set; }
        public DateTime? PasswordResetRequestedAt { get; set; }
        public int AccessFailedCount { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LockoutEndDate { get; set; }

        // Audit Trail
        public List<UserActivity> ActivityLog { get; set; }

        // Permissions and Roles
        public List<string> Permissions { get; set; }
    }

    // User Activity Tracking
    public class UserActivity
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public ActivityType ActivityType { get; set; }
        public string Description { get; set; }
        public string IPAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeviceInfo { get; set; }
    }

    // Enumerations
    public enum UserType
    {
        Administrator,
        Physician,
        Nurse,
        Staff,
        Patient,
        Researcher,
        External
    }

    public enum ActivityType
    {
        Login,
        Logout,
        PasswordChange,
        ProfileUpdate,
        ResourceAccess,
        AccountLockout,
        PasswordResetRequest
    }

    // Custom Claims for Fine-Grained Authorization
    public static class CustomClaimTypes
    {
        public const string Department = "Department";
        public const string Specialization = "Specialization";
        public const string UserType = "UserType";
        public const string Permissions = "Permissions";
    }
}
