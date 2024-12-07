using System;

namespace EMRNext.Core.Domain.Common
{
    /// <summary>
    /// Base abstract class for all domain entities with generic identifier
    /// </summary>
    /// <typeparam name="TId">Type of the identifier</typeparam>
    public abstract class Entity<TId> : IEquatable<Entity<TId>>
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public TId Id { get; protected set; }

        /// <summary>
        /// Timestamp of entity creation
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp of last entity update
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indicates if the entity is soft deleted
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Equality comparison based on ID
        /// </summary>
        public bool Equals(Entity<TId> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Id, other.Id);
        }

        /// <summary>
        /// Override of object equality
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Entity<TId>)obj);
        }

        /// <summary>
        /// Generate hash code based on ID
        /// </summary>
        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(Entity<TId> a, Entity<TId> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(Entity<TId> a, Entity<TId> b)
        {
            return !(a == b);
        }
    }
}
