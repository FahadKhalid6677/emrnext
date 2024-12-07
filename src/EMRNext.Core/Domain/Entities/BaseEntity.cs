using System;
using EMRNext.Core.Domain.Interfaces;

namespace EMRNext.Core.Domain.Entities
{
    /// <summary>
    /// Abstract base class for entities with standard audit and soft delete capabilities
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key</typeparam>
    public abstract class BaseEntity<TKey> : IEntity<TKey>
    {
        /// <inheritdoc/>
        public virtual TKey Id { get; set; }

        /// <inheritdoc/>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <inheritdoc/>
        public DateTime? UpdatedAt { get; set; }

        /// <inheritdoc/>
        public string CreatedBy { get; set; }

        /// <inheritdoc/>
        public string UpdatedBy { get; set; }

        /// <inheritdoc/>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Soft delete the entity
        /// </summary>
        public virtual void Delete(string deletedBy)
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deletedBy;
        }

        /// <summary>
        /// Update entity with audit information
        /// </summary>
        public virtual void Update(string updatedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }

    /// <summary>
    /// Base entity with integer primary key
    /// </summary>
    public abstract class BaseIntEntity : BaseEntity<int> { }

    /// <summary>
    /// Base entity with string primary key
    /// </summary>
    public abstract class BaseStringEntity : BaseEntity<string> { }
}
