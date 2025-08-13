using Core_API.Domain.Entities;

namespace Core_API.Application.Contracts.Persistence
{
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        Task<Company> GetByIdAsync(int id, bool includeCustomers = false, bool includeInvoices = false);
        Task<bool> CanDeleteAsync(int id);
    }
}
