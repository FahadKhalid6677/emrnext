using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EMRNext.Core.Repositories
{
    /// <summary>
    /// Generic repository interface for CRUD operations
    /// </summary>
    /// <typeparam name="TEntity">The type of entity</typeparam>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Get entity by its identifier
        /// </summary>
        Task<TEntity> GetByIdAsync(Guid id);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Find entities based on a predicate
        /// </summary>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Add a new entity
        /// </summary>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Add multiple entities
        /// </summary>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Update an existing entity
        /// </summary>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Remove an entity
        /// </summary>
        Task RemoveAsync(TEntity entity);

        /// <summary>
        /// Remove multiple entities
        /// </summary>
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Check if any entity matches the given predicate
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Count entities matching the predicate
        /// </summary>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
