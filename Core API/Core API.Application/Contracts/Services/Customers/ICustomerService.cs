using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Contracts.Services.Customers
{
    public interface ICustomerService
    {
        Task<OperationResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto, OperationContext context);
        Task<OperationResult<CustomerResponseDto>> UpdateAsync(UpdateCustomerDto dto, OperationContext context);
        Task<OperationResult<bool>> DeleteAsync(int id, OperationContext context);
        Task<OperationResult<CustomerResponseDto>> GetByIdAsync(int id, OperationContext context);
        Task<OperationResult<PaginatedResult<CustomerResponseDto>>> GetPagedAsync(OperationContext context, CustomerFilterRequestDto filter);
        Task<OperationResult<CustomerStatsDto>> GetStatsAsync(OperationContext context);
    }
}
