using System.Linq.Expressions;

namespace Core_API.Application.Contracts.Persistence
{
    public interface IGenericRepository<T> where T : class
    {
        //T - category
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        Task<T> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
        Task AddAsync(T entity);
        Task RemoveAsync(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entity);
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null);
        IQueryable<T> Query();
        void Update(T entity);
        void Delete(T entity);
    }
}
