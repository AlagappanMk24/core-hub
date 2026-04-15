using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Company.Request;
using Core_API.Application.DTOs.Company.Response;
using Core_API.Application.DTOs.User.Response;

namespace Core_API.Application.Contracts.Services
{
    public interface ICompanyService
    {
        Task<CompanyResponseDto> CreateCompanyAsync(CompanyCreateDto companyDto, string userId);
        Task<CompanyResponseDto> GetCompanyByIdAsync(int id);
        Task<bool> DeleteCompanyAsync(int id);
        Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync();
        Task<OperationResult<List<CompanyDto>>> GetAllCompaniesAsync(OperationContext operationContext);
    }
}
