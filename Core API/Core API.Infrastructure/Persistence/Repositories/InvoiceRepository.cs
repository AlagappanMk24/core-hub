using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
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
           int pageNumber,
           int pageSize,
           string? search = null,
           string? invoiceStatus = null,
           string? invoicePaymentStatus = null,
           int? customerId = null,
           int? taxType = null,
           decimal? minAmount = null,
           decimal? maxAmount = null,
           string? invoiceNumberFrom = null,
           string? invoiceNumberTo = null,
           DateTime? issueDateFrom = null,
           DateTime? issueDateTo = null,
           DateTime? dueDateFrom = null,
           DateTime? dueDateTo = null)
        {
            IQueryable<Invoice> query = dbset
                .Where(i => i.CompanyId == companyId && !i.IsDeleted)
                .Include(i => i.Customer)
                .Include(i => i.InvoiceItems)
                .Include(i => i.TaxDetails)
                .Include(i => i.Discounts);

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(i => i.InvoiceNumber.ToLower().Contains(search) || i.Customer.Name.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(invoiceStatus) && Enum.TryParse<InvoiceStatus>(invoiceStatus, true, out var parsedInvoiceStatus))
            {
                query = query.Where(i => i.InvoiceStatus == parsedInvoiceStatus);
            }

            if (!string.IsNullOrEmpty(invoicePaymentStatus) && Enum.TryParse<PaymentStatus>(invoicePaymentStatus, true, out var parsedPaymentStatus))
            {
                query = query.Where(i => i.PaymentStatus == parsedPaymentStatus);
            }

            if (customerId.HasValue)
            {
                query = query.Where(i => i.CustomerId == customerId.Value);
            }

            if (taxType.HasValue)
            {
                query = query.Where(i => i.TaxDetails.Any(t => t.Id == taxType.Value));
            }

            if (minAmount.HasValue)
            {
                query = query.Where(i => i.TotalAmount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(i => i.TotalAmount <= maxAmount.Value);
            }

            if (!string.IsNullOrEmpty(invoiceNumberFrom))
            {
                query = query.Where(i => string.Compare(i.InvoiceNumber, invoiceNumberFrom, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrEmpty(invoiceNumberTo))
            {
                query = query.Where(i => string.Compare(i.InvoiceNumber, invoiceNumberTo, StringComparison.OrdinalIgnoreCase) <= 0);
            }

            if (issueDateFrom.HasValue)
            {
                query = query.Where(i => i.IssueDate >= issueDateFrom.Value);
            }

            if (issueDateTo.HasValue)
            {
                query = query.Where(i => i.IssueDate <= issueDateTo.Value);
            }

            if (dueDateFrom.HasValue)
            {
                query = query.Where(i => i.PaymentDue >= dueDateFrom.Value);
            }

            if (dueDateTo.HasValue)
            {
                query = query.Where(i => i.PaymentDue <= dueDateTo.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.IssueDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Invoice>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
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
