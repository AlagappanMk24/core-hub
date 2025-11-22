using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository(CoreAPIDbContext dbContext) : GenericRepository<Customer>(dbContext), ICustomerRepository
    {
        public async Task<PaginatedResult<Customer>> GetPagedAsync(int companyId, CustomerFilterRequestDto filter)
        {
            var query = dbset.Where(c => c.CompanyId == companyId);

            // Apply status filter
            if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "All")
            {
                bool isActive = filter.Status == "Active";
                query = query.Where(c => c.IsDeleted == !isActive);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.Search))
            {
                // Convert the search term to lowercase once
                var searchLower = filter.Search.ToLower();

                // Apply ToLower() to the database columns as well, then use the basic Contains()
                query = query.Where(c => c.Name.ToLower().Contains(searchLower) || c.Email.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Name)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
        public async Task<bool> ExistsAsync(int companyId, string email)
        {
            return await dbset.AnyAsync(c => c.CompanyId == companyId && c.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase) && !c.IsDeleted);
        }
    }
}