using Core_API.Application.Common.Results;
using Core_API.Application.Features.Companies.DTOs;
using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Services
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyDto>> GetAllCompaniesAsync();
        //Task<PaginatedResult<CompanyDto>> GetCompaniesPaginatedAsync(CompanyQueryParameters parameters);
        Task<Company> GetCompanyByIdAsync(int id);
        Task AddCompanyAsync(Company company);
        Task UpdateCompanyAsync(Company company);
        Task DeleteCompanyAsync(Company company);
        Task<List<string>> GetCompanyStates();
    }
}
