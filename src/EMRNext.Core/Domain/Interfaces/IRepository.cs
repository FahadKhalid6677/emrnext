using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.Specifications;

namespace EMRNext.Core.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface for CRUD operations
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    public interface IRepository<TEntity, TKey> 
        where TEntity : BaseEntity<TKey>
    {
        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<TEntity> GetByIdAsync(TKey id);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IReadOnlyList<TEntity>> ListAllAsync();

        /// <summary>
        /// Find entities based on specification
        /// </summary>
        Task<IReadOnlyList<TEntity>> FindAsync(ISpecification<TEntity> specification);

        /// <summary>
        /// Count entities based on specification
        /// </summary>
        Task<int> CountAsync(ISpecification<TEntity> specification);

        /// <summary>
        /// Add new entity
        /// </summary>
        Task<TEntity> AddAsync(TEntity entity);

        /// <summary>
        /// Add multiple entities
        /// </summary>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Update existing entity
        /// </summary>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        Task DeleteAsync(TEntity entity);

        /// <summary>
        /// Soft delete entity
        /// </summary>
        Task SoftDeleteAsync(TKey id, string deletedBy);

        /// <summary>
        /// Check if entity exists
        /// </summary>
        Task<bool> ExistsAsync(TKey id);
    }

    /// <summary>
    /// Repository for integer-keyed entities
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IIntRepository<TEntity> : IRepository<TEntity, int> 
        where TEntity : BaseIntEntity
    {
    }
}
