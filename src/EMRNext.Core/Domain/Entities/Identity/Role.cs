using EMRNext.Core.Domain.Entities.Common;

namespace EMRNext.Core.Domain.Entities.Identity
{
    public class Role : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
