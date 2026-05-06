using Core_API.Application.Contracts.Persistence.Companies;
using Core_API.Domain.Entities.Companies;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Companies
{
    public class CompanyRepository(CoreInvoiceDbContext dbContext) : GenericRepository<Company>(dbContext), ICompanyRepository
    {
        public async Task<Company> GetByIdAsync(int id, bool includeCustomers = false, bool includeInvoices = false)
        {
            var query = _dbSet.AsQueryable();
            if (includeCustomers)
                query = query.Include(c => c.Customers);
            if (includeInvoices)
                query = query.Include(c => c.Invoices);
            return await query.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            var company = await _dbSet
                .Include(c => c.Customers)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (company == null)
                return false;
            return !company.Customers.Any() && !company.Invoices.Any();
        }
    }
}