using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class CompanyRepository(CoreAPIDbContext dbContext) : GenericRepository<Company>(dbContext), ICompanyRepository
    {
        public async Task<Company> GetByIdAsync(int id, bool includeCustomers = false, bool includeInvoices = false)
        {
            var query = dbset.AsQueryable();
            if (includeCustomers)
                query = query.Include(c => c.Customers);
            if (includeInvoices)
                query = query.Include(c => c.Invoices);
            return await query.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            var company = await dbset
                .Include(c => c.Customers)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (company == null)
                return false;
            return !company.Customers.Any() && !company.Invoices.Any();
        }
    }
}
