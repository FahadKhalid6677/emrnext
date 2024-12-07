using System;
using System.Linq.Expressions;

namespace EMRNext.Core.Domain.Specifications
{
    /// <summary>
    /// Specification pattern for complex querying
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Criteria to filter entities
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// Include related entities
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// Ordering specification
        /// </summary>
        Expression<Func<T, object>> OrderBy { get; }

        /// <summary>
        /// Descending ordering specification
        /// </summary>
        Expression<Func<T, object>> OrderByDescending { get; }

        /// <summary>
        /// Pagination parameters
        /// </summary>
        int Take { get; }

        /// <summary>
        /// Pagination skip count
        /// </summary>
        int Skip { get; }

        /// <summary>
        /// Enable pagination
        /// </summary>
        bool IsPagingEnabled { get; }
    }

    /// <summary>
    /// Base specification implementation
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>> Criteria { get; }
        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
        public Expression<Func<T, object>> OrderBy { get; private set; }
        public Expression<Func<T, object>> OrderByDescending { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; } = false;

        protected BaseSpecification() { }

        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }
}
