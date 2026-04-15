using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<PaginatedResult<Customer>> GetPagedAsync(int companyId, CustomerFilterRequestDto filter);
        Task<bool> ExistsAsync(OperationContext context, string email);
    }
}