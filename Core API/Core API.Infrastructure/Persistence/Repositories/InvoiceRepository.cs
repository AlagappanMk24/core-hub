using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class InvoiceRepository(CoreAPIDbContext dbContext) : GenericRepository<Invoice>(dbContext), IInvoiceRepository
    {
        //public async Task<PaginatedResult<Invoice>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search = null, string status = null)
        //{
        //    IQueryable<Invoice> query = dbset
        //       .Where(i => i.CompanyId == companyId && !(i.IsDeleted))
        //       .Include(i => i.Customer)
        //       .Include(i => i.InvoiceItems)
        //       .Include(i => i.TaxDetails)
        //       .Include(i => i.Discounts);

        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        search = search.ToLower();
        //        query = query.Where(i => i.InvoiceNumber.ToLower().Contains(search) || i.Customer.Name.ToLower().Contains(search));
        //    }

        //    if (!string.IsNullOrEmpty(status) && Enum.TryParse<InvoiceStatus>(status, true, out var invoiceStatus))
        //    {
        //        query = query.Where(i => i.Status == invoiceStatus);
        //    }

        //    var totalCount = await query.CountAsync();
        //    var items = await query
        //        .OrderByDescending(i => i.IssueDate)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return new PaginatedResult<Invoice>
        //    {
        //        Items = items,
        //        TotalCount = totalCount,
        //        PageNumber = pageNumber,
        //        PageSize = pageSize
        //    };
        //}

        public async Task<PaginatedResult<Invoice>> GetPagedAsync(
           int companyId,
           InvoiceFilterRequestDto filter)
        {
            IQueryable<Invoice> query = dbset
                .Where(i => i.CompanyId == companyId && !i.IsDeleted)
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .Include(i => i.TaxDetails)
                .Include(i => i.Discounts);

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Search))
            {
                filter.Search = filter.Search.ToLower();
                query = query.Where(i => i.InvoiceNumber.ToLower().Contains(filter.Search) || i.Customer.Name.ToLower().Contains(filter.Search));
            }

            if (!string.IsNullOrEmpty(filter.InvoiceStatus) && Enum.TryParse<InvoiceStatus>(filter.InvoiceStatus, true, out var parsedInvoiceStatus))
            {
                query = query.Where(i => i.InvoiceStatus == parsedInvoiceStatus);
            }

            if (!string.IsNullOrEmpty(filter.PaymentStatus) && Enum.TryParse<PaymentStatus>(filter.PaymentStatus, true, out var parsedPaymentStatus))
            {
                query = query.Where(i => i.PaymentStatus == parsedPaymentStatus);
            }

            if (filter.CustomerId.HasValue)
            {
                query = query.Where(i => i.CustomerId == filter.CustomerId.Value);
            }

            if (filter.TaxType.HasValue)
            {
                query = query.Where(i => i.TaxDetails.Any(t => t.Id == filter.TaxType.Value));
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(i => i.TotalAmount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(i => i.TotalAmount <= filter.MaxAmount.Value);
            }

            if (!string.IsNullOrEmpty(filter.InvoiceNumberFrom))
            {
                query = query.Where(i => string.Compare(i.InvoiceNumber, filter.InvoiceNumberFrom, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrEmpty(filter.InvoiceNumberTo))
            {
                query = query.Where(i => string.Compare(i.InvoiceNumber, filter.InvoiceNumberTo, StringComparison.OrdinalIgnoreCase) <= 0);
            }

            if (filter.IssueDateFrom.HasValue)
            {
                query = query.Where(i => i.IssueDate >= filter.IssueDateFrom.Value);
            }

            if (filter.IssueDateTo.HasValue)
            {
                query = query.Where(i => i.IssueDate <= filter.IssueDateTo.Value);
            }

            if (filter.DueDateFrom.HasValue)
            {
                query = query.Where(i => i.DueDate >= filter.DueDateFrom.Value);
            }

            if (filter.DueDateTo.HasValue)
            {
                query = query.Where(i => i.DueDate <= filter.DueDateTo.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.IssueDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedResult<Invoice>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            };
        }

        public async Task<bool> InvoiceNumberExistsAsync(int companyId, string invoiceNumber, int? excludeInvoiceId = null)
        {
            var query = dbset.Where(i => i.CompanyId == companyId && i.InvoiceNumber == invoiceNumber && !i.IsDeleted);
            if (excludeInvoiceId.HasValue)
            {
                query = query.Where(i => i.Id != excludeInvoiceId.Value);
            }
            return await query.AnyAsync();
        }
    }
}
