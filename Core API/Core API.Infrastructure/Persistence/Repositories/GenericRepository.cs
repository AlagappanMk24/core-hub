using Core_API.Application.Contracts.Persistence;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T>(CoreAPIDbContext context) : IGenericRepository<T> where T : class
    {
        protected readonly CoreAPIDbContext _context = context;

        /// <summary>
        /// Retrieves an entity by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        public async Task<T> GetByIdAsync(int id)
        {
            // Fetch entity using primary key
            var model = await _context.Set<T>().FindAsync(id);
            return model;
        }

        /// <summary>
        /// Retrieves all entities of type T.
        /// </summary>
        /// <returns>A list of all entities.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Get all records without tracking changes
            var models = await _context.Set<T>().AsNoTracking().ToListAsync();
            return models;
        }

        /// <summary>
        /// Retrieves an entity by its name. Assumes the entity has a "Name" property.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        public async Task<T?> GetByNameAsync(string name)
        {
            return await _context.Set<T>().AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name); // Correct way to query by name
        }

        /// <summary>
        /// Adds a new entity to the database.
        /// </summary>
        /// <param name="model">The entity to be added.</param>
        /// <returns>The added entity.</returns>
        public async Task<T> AddAsync(T model)
        {
            // Add entity asynchronously
            await _context.Set<T>().AddAsync(model);
            return model;
        }

        /// <summary>
        /// Updates an existing entity in the database.
        /// </summary>
        /// <param name="model">The entity with updated values.</param>
        /// <returns>The updated entity if found; otherwise, null.</returns>
        public async Task<T> UpdateAsync(T model)
        {
            var entity = await _context.Set<T>().FindAsync(GetPrimaryKeyValue(model)); // Find entity by primary key
            if (entity != null)
            {
                _context.Entry(entity).CurrentValues.SetValues(model); // Update entity values
                return entity;
            }
            return null;
        }

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the entity.</param>
        /// <returns>The deleted entity if found; otherwise, null.</returns>
        public async Task<T> DeleteAsync(int Id)
        {
            T entity = await _context.Set<T>().FindAsync(Id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                return entity;
            }
            return null;
        }

        /// <summary>
        /// Extracts the primary key value from an entity.
        /// </summary>
        /// <param name="entity">The entity to extract the key from.</param>
        /// <returns>The primary key value.</returns>
        private object GetPrimaryKeyValue(T entity)
        {
            var keyName = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties
                .Select(x => x.Name).FirstOrDefault();

            if (keyName == null)
                throw new InvalidOperationException("Primary key not found for entity.");

            return entity.GetType().GetProperty(keyName)?.GetValue(entity)
                ?? throw new InvalidOperationException("Primary key value not found.");
        }
    }
}