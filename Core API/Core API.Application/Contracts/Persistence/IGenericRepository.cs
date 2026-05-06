using System.Linq.Expressions;

namespace Core_API.Application.Contracts.Persistence
{
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddAsync(T entity);

        /// <summary>
        /// Retrieves all entities that match an optional filter.
        /// </summary>
        /// <param name="filter">A function to filter the results.</param>
        /// <param name="includeProperties">Comma-separated string of navigation properties to include (Eager Loading).</param>
        /// <returns>A collection of entities.</returns>
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        /// <summary>
        /// Retrieves a single entity that matches the specified filter.
        /// </summary>
        /// <param name="filter">A function to filter the entity.</param>
        /// <param name="includeProperties">Comma-separated string of navigation properties to include.</param>
        /// <param name="tracked">Whether or not the entity should be tracked by the Change Tracker.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);

        /// <summary>
        /// Retrieves an entity by its primary key.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity with updated values.</param>
        void Update(T entity);

        /// <summary>
        /// Removes an entity from the database.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveAsync(T entity);

        /// <summary>
        /// Removes a collection of entities from the database.
        /// </summary>
        /// <param name="entities">The collection of entities to remove.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Checks if any entity matches the specified criteria.
        /// </summary>
        /// <param name="predicate">The criteria to check against.</param>
        /// <returns>True if at least one match exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets the count of entities that match the filter.
        /// </summary>
        /// <param name="filter">The criteria to filter the count.</param>
        /// <returns>The number of matching entities.</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

        /// <summary>
        /// Provides access to the raw IQueryable for custom complex queries.
        /// </summary>
        /// <returns>An IQueryable of the entity type.</returns>
        IQueryable<T> Query();

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        void Delete(T entity);
    }
}
