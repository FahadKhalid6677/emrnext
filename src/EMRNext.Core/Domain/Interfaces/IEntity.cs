using System;

namespace EMRNext.Core.Domain.Interfaces
{
    /// <summary>
    /// Generic interface for entities with audit capabilities
    /// </summary>
    /// <typeparam name="TKey">Type of the primary key</typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        TKey Id { get; set; }

        /// <summary>
        /// Timestamp of entity creation
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp of last update
        /// </summary>
        DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User who created the entity
        /// </summary>
        string CreatedBy { get; set; }

        /// <summary>
        /// User who last updated the entity
        /// </summary>
        string UpdatedBy { get; set; }

        /// <summary>
        /// Indicates if the entity is soft deleted
        /// </summary>
        bool IsDeleted { get; set; }
    }
}
