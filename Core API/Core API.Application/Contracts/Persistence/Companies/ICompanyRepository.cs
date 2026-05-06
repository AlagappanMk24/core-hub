using Core_API.Domain.Entities.Companies;

namespace Core_API.Application.Contracts.Persistence.Companies
{
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        Task<Company> GetByIdAsync(int id, bool includeCustomers = false, bool includeInvoices = false);
        Task<bool> CanDeleteAsync(int id);
    }
}
