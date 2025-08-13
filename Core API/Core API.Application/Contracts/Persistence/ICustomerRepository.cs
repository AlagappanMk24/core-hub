using Core_API.Application.Common.Results;
using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<PaginatedResult<Customer>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search = null);
        Task<bool> ExistsAsync(int companyId, string email);
    }
}