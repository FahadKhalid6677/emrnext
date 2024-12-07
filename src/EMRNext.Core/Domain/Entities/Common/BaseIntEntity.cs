using System;

namespace EMRNext.Core.Domain.Entities.Common
{
    public abstract class BaseIntEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
