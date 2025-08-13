using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Contracts.Services
{
    public interface ICustomerService
    {
        Task<OperationResult<CustomerResponseDto>> CreateAsync(CustomerCreateDto dto, int companyId, string userId);
        Task<OperationResult<CustomerResponseDto>> UpdateAsync(CustomerUpdateDto dto, int companyId, string userId);
        Task<OperationResult<bool>> DeleteAsync(int id, int companyId, string userId);
        Task<OperationResult<CustomerResponseDto>> GetByIdAsync(int id, int companyId);
        Task<OperationResult<PaginatedResult<CustomerResponseDto>>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search = null);
    }
}
