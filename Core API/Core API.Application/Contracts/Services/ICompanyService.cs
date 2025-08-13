using Core_API.Application.DTOs.Company.Request;
using Core_API.Application.DTOs.Company.Response;

namespace Core_API.Application.Contracts.Services
{
    public interface ICompanyService
    {
        Task<CompanyResponseDto> CreateCompanyAsync(CompanyCreateDto companyDto, string userId);
        Task<CompanyResponseDto> GetCompanyByIdAsync(int id);
        Task<bool> DeleteCompanyAsync(int id);
        Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync();
    }
}
