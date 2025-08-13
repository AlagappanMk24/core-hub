using Core_API.Application.Contracts.Persistence;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly CoreAPIDbContext _dbContext;
        internal DbSet<T> dbset;
        public GenericRepository(CoreAPIDbContext dbContext)
        {
            _dbContext = dbContext;
            dbset = _dbContext.Set<T>();
        }

        /// <summary>
        /// Adds a new entity to the database asynchronously.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(T entity)
        {
            await dbset.AddAsync(entity);
        }

        /// <summary>
        /// Retrieves a single entity that matches the specified filter.
        /// </summary>
        /// <param name="filter">The filter to apply when searching for the entity.</param>
        /// <param name="includeProperties">Comma-separated list of related entities to include in the result.</param>
        /// <param name="tracked">Indicates whether to track the retrieved entity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the found entity or null if no entity matches the filter.</returns>
        public async Task<T> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dbset;

            }
            else
            {
                query = dbset.AsNoTracking();

            }
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split([','], StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();

        }

        /// <summary>
        /// Retrieves all entities that match the specified filter.
        /// </summary>
        /// <param name="filter">The filter to apply when searching for entities.</param>
        /// <param name="includeProperties">Comma-separated list of related entities to include in the results.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities that match the filter.</returns>
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbset;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split([','], StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.ToListAsync();
        }

        /// <summary>
        /// Removes an entity from the database asynchronously.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveAsync(T entity)
        {
            dbset.Remove(entity);
            await _dbContext.SaveChangesAsync(); // Ensure changes are saved
        }

        /// <summary>
        /// Removes a range of entities from the database asynchronously.
        /// </summary>
        /// <param name="entities">The collection of entities to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            dbset.RemoveRange(entities);
            await _dbContext.SaveChangesAsync(); // Ensure changes are saved
        }

        /// <summary>
        /// Counts the number of entities that match the specified filter.
        /// </summary>
        /// <param name="filter">The filter to apply when counting entities.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities that match the filter.</returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>> filter = null)
        {
            IQueryable<T> query = dbset;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.CountAsync();
        }
        public IQueryable<T> Query()
        {
            return dbset.AsQueryable();
        }
        public void Update(T entity)
        {
            dbset.Update(entity);
        }

        public void Delete(T entity)
        {
            dbset.Remove(entity);
        }

    }
}