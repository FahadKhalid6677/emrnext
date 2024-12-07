using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Identity
{
    /// <summary>
    /// Extended Application User for EMRNext
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        /// <summary>
        /// Reference to associated Patient
        /// </summary>
        public Guid? PatientId { get; set; }
        public Patient? Patient { get; set; }

        /// <summary>
        /// User's full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// User's professional role
        /// </summary>
        public UserRole UserRole { get; set; }

        /// <summary>
        /// Account creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Account status
        /// </summary>
        public UserStatus Status { get; set; } = UserStatus.Active;

        /// <summary>
        /// User's access permissions
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// Indicates if Multi-Factor Authentication is enabled
        /// </summary>
        public bool MfaEnabled { get; set; }

        /// <summary>
        /// Secret key for Time-based One-Time Password (TOTP)
        /// </summary>
        public string MfaSecretKey { get; set; }

        /// <summary>
        /// Hashed backup recovery codes for MFA
        /// </summary>
        public string MfaBackupCodes { get; set; }
    }

    /// <summary>
    /// User professional roles
    /// </summary>
    public enum UserRole
    {
        Patient,
        Physician,
        Nurse,
        Administrator,
        Researcher
    }

    /// <summary>
    /// User account status
    /// </summary>
    public enum UserStatus
    {
        Active,
        Suspended,
        Locked,
        Inactive
    }
}
