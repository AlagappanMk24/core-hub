using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence.Invoice;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.Features.Invoices.Queries.GetInvoicesPaged;
using Core_API.Domain.Enums;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Invoices
{
    public class InvoiceRepository(CoreInvoiceDbContext dbContext) : GenericRepository<Domain.Entities.Invoices.Invoice>(dbContext), IInvoiceRepository
    {
        public async Task<PaginatedResult<Domain.Entities.Invoices.Invoice>> GetPagedAsync(
         int? companyId,
         GetPagedInvoicesQuery filter,
         CancellationToken cancellationToken = default)
        {
            IQueryable<Domain.Entities.Invoices.Invoice> query = _dbSet.Where(i => !i.IsDeleted);

            // Filter by company ONLY if an ID is provided (Super Admin passes null)
            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            query = query
             .Include(i => i.Customer)
             .Include(i => i.Company)
             .Include(i => i.InvoiceItems)
             .Include(i => i.TaxDetails)
             .Include(i => i.Discounts)
             .Include(i => i.InvoiceAttachments)
             .Include(i => i.Payments);

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(i => i.InvoiceNumber.ToLower().Contains(searchLower) ||
                                         i.Customer.Name.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrEmpty(filter.InvoiceStatus))
            {
                var invoiceStatusValue = MapInvoiceStatus(filter.InvoiceStatus);
                if (Enum.TryParse<InvoiceStatus>(invoiceStatusValue, true, out var parsedInvoiceStatus))
                {
                    query = query.Where(i => i.InvoiceStatus == parsedInvoiceStatus);
                }
            }

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
            {
                var paymentStatusValue = MapPaymentStatus(filter.PaymentStatus);
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

            return new PaginatedResult<Domain.Entities.Invoices.Invoice>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            };
        }
        public async Task<bool> InvoiceNumberExistsAsync(int companyId, string invoiceNumber, int? excludeInvoiceId = null)
        {
            var query = _dbSet.Where(i => i.CompanyId == companyId && i.InvoiceNumber == invoiceNumber && !i.IsDeleted);
            if (excludeInvoiceId.HasValue)
            {
                query = query.Where(i => i.Id != excludeInvoiceId.Value);
            }
            return await query.AnyAsync();
        }
        private static string MapInvoiceStatus(string status)
        {
            return status switch
            {
                "Cancelled" => "Void",
                "Approved" => "Sent",
                _ => status
            };
        }
        private static string MapPaymentStatus(string status)
        {
            return status switch
            {
                "Completed" => "Paid",
                "Processing" => "Pending",
                _ => status
            };
        }
    }
}