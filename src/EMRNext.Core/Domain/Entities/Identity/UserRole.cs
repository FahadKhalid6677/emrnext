using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Identity
{
    public class UserRole : BaseEntity
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}
