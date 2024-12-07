using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Identity
{
    public class User : AuditableEntity
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
