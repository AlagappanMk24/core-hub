using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository(CoreAPIDbContext dbContext) : GenericRepository<Customer>(dbContext), ICustomerRepository
    {
        public async Task<PaginatedResult<Customer>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search = null)
        {
            var query = dbset.Where(c => c.CompanyId == companyId && !c.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search) || c.Email.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        public async Task<bool> ExistsAsync(int companyId, string email)
        {
            return await dbset.AnyAsync(c => c.CompanyId == companyId && c.Email.ToLower() == email.ToLower() && !c.IsDeleted);
        }
    }
}
