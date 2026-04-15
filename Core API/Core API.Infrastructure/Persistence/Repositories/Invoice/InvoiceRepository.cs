using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Domain.Enums;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories.Invoice
{
    public class InvoiceRepository(CoreInvoiceDbContext dbContext) : GenericRepository<Core_API.Domain.Entities.Invoice>(dbContext), IInvoiceRepository
    {
        public async Task<PaginatedResult<Core_API.Domain.Entities.Invoice>> GetPagedAsync(
           int? companyId,
           InvoiceFilterRequestDto filter)
        {
            IQueryable<Core_API.Domain.Entities.Invoice> query = dbset.Where(i => !i.IsDeleted);

            // Filter by company ONLY if an ID is provided (Super Admin passes null)
            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            query = query
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

            if (!string.IsNullOrEmpty(filter.InvoiceStatus))
            {
                var invoiceStatusValue = filter.InvoiceStatus;

                // Map common frontend values to backend enum
                invoiceStatusValue = invoiceStatusValue switch
                {
                    "Cancelled" => "Void",      // Map "Cancelled" to "Void"
                    "Approved" => "Sent",       // Map "Approved" to "Sent" (if needed)
                    _ => invoiceStatusValue
                };

                if (Enum.TryParse<InvoiceStatus>(invoiceStatusValue, true, out var parsedInvoiceStatus))
                {
                    query = query.Where(i => i.InvoiceStatus == parsedInvoiceStatus);
                }
            }

            // Complete fix for GetPagedAsync
            if (!string.IsNullOrEmpty(filter.PaymentStatus))
            {
                var paymentStatusValue = filter.PaymentStatus;

                // Map common frontend values to backend enum
                paymentStatusValue = paymentStatusValue switch
                {
                    "Completed" => "Paid",
                    "Processing" => "Pending",
                    _ => paymentStatusValue
                };

                if (Enum.TryParse<PaymentStatus>(paymentStatusValue, true, out var parsedPaymentStatus))
                {
                    query = query.Where(i => i.PaymentStatus == parsedPaymentStatus);
                }
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

            return new PaginatedResult<Core_API.Domain.Entities.Invoice>
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
