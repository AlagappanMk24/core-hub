using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence.Customers;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Entities.Invoices;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Repositories.Customers
{
    /// <inheritdoc cref="ICustomerRepository"/>
    public class CustomerRepository(CoreInvoiceDbContext dbContext) : GenericRepository<Customer>(dbContext), ICustomerRepository
    {
        private readonly CoreInvoiceDbContext _dbContext = dbContext;

        /// <inheritdoc />
        public async Task<PaginatedResult<Customer>> GetPagedAsync(int companyId, CustomerFilterRequestDto filter)
        {
            // Start with non-deleted customers only
            var query = _dbSet.Where(c => c.CompanyId == companyId && !c.IsDeleted);

            // Apply status filter (Active/Inactive based on IsDeleted)
            if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "All")
            {
                bool isActive = filter.Status == "Active";
                // Active = not deleted, Inactive = deleted
                query = query.Where(c => c.IsDeleted == !isActive);
            }

            // Apply search filter with case-insensitive search
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchLower = filter.Search.ToLower();

                // For owned types, we need to use EF.Property or navigate correctly
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchLower) ||
                    EF.Property<string>(c.Email, "Value").ToLower().Contains(searchLower) ||
                    EF.Property<string>(c.PhoneNumber, "Value").Contains(filter.Search)
                );
            }

            var totalCount = await query.CountAsync();

            // ✅ Industry standard ordering: Active customers first, then by Name
            var items = await query
                .OrderBy(c => c.IsDeleted)      // Active (false) first, then Inactive (true)
                .ThenBy(c => c.Name)             // Then alphabetically by name
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            };
        }

        /// <inheritdoc />
        public async Task<PaginatedResult<Customer>> GetAllCustomersPagedAsync(CustomerFilterRequestDto filter)
        {
            // Start with all non-deleted customers across ALL companies
            var query = _dbSet.Where(c => !c.IsDeleted);

            // Apply status filter (Active/Inactive based on IsDeleted)
            if (!string.IsNullOrEmpty(filter.Status) && filter.Status != "All")
            {
                bool isActive = filter.Status == "Active";
                query = query.Where(c => c.IsDeleted == !isActive);
            }

            // Apply search filter with case-insensitive search
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchLower) ||
                    EF.Property<string>(c.Email, "Value").ToLower().Contains(searchLower) ||
                    EF.Property<string>(c.PhoneNumber, "Value").Contains(filter.Search)
                );
            }

            var totalCount = await query.CountAsync();

            // Super Admin ordering: By Company name first, then Customer name
            var items = await query
                .Include(c => c.Company)  // Include company for Super Admin view
                .OrderBy(c => c.Company.Name)  // Group by company
                .ThenBy(c => c.IsDeleted)      // Active first, then inactive
                .ThenBy(c => c.Name)           // Then by customer name
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedResult<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
            };
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(OperationContext context, string email)
        {
            if (!context.CompanyId.HasValue)
                return false;

            return await _dbSet.AnyAsync(c =>
                 c.CompanyId == context.CompanyId.Value &&
                 EF.Property<string>(c.Email, "Value").ToLower() == email.ToLower() &&
                 !c.IsDeleted);
        }

        /// <inheritdoc />
        public async Task<bool> HasInvoicesAsync(int customerId)
        {
            return await _dbContext.Invoices
                .AnyAsync(i => i.CustomerId == customerId && !i.IsDeleted);
        }

        /// <inheritdoc />
        public async Task<List<Domain.Entities.Invoices.Invoice>> GetCustomerInvoicesAsync(int customerId)
        {
            return await _dbContext.Invoices
                .Where(i => i.CustomerId == customerId && !i.IsDeleted)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<InvoicePayment>> GetCustomerPaymentsAsync(int customerId)
        {
            return await _dbContext.InvoicePayments
                .Where(p => p.CustomerId == customerId && !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<CustomerActivityDto>> GetCustomerActivitiesAsync(int customerId, int count = 10)
        {
            var activities = new List<CustomerActivityDto>();

            // Get invoice activities
            var invoices = await _dbContext.Invoices
                .Where(i => i.CustomerId == customerId && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedDate)
                .Take(count)
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                activities.Add(new CustomerActivityDto
                {
                    Id = invoice.Id,
                    Icon = "receipt",
                    Action = "Invoice Created",
                    Description = $"Invoice {invoice.InvoiceNumber} was created for ${invoice.TotalAmount:F2}",
                    Date = invoice.CreatedDate,
                    Color = "#8B5CF6"
                });
            }

            // Get payment activities
            var payments = await _dbContext.InvoicePayments
                .Where(p => p.CustomerId == customerId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .ToListAsync();

            foreach (var payment in payments)
            {
                var invoice = await _dbContext.Invoices.FindAsync(payment.InvoiceId);
                activities.Add(new CustomerActivityDto
                {
                    Id = payment.Id,
                    Icon = "payment",
                    Action = "Payment Received",
                    Description = $"Payment of ${payment.Amount:F2} received for invoice {invoice?.InvoiceNumber ?? "N/A"}",
                    Date = payment.PaymentDate,
                    Color = "#10B981"
                });
            }

            // Get invoice status changes (if you have audit logs)
            var auditLogs = await _dbContext.InvoiceAuditLogs
                .Where(a => a.Invoice.CustomerId == customerId)
                .OrderByDescending(a => a.CreatedDate)
                .Take(count)
                .ToListAsync();

            foreach (var log in auditLogs)
            {
                activities.Add(new CustomerActivityDto
                {
                    Id = log.Id,
                    Icon = "edit",
                    Action = "Status Updated",
                    Description = log.Description ?? $"Invoice status changed to {log.Action}",
                    Date = log.CreatedDate,
                    Color = "#F59E0B"
                });
            }

            return activities
                .OrderByDescending(a => a.Date)
                .Take(count)
                .ToList();
        }

        /// <inheritdoc />
        public async Task<List<SpendingTrendDto>> GetCustomerSpendingTrendAsync(int customerId)
        {
            var trend = new List<SpendingTrendDto>();
            var startDate = DateTime.UtcNow.AddMonths(-11);
            var endDate = DateTime.UtcNow;

            var invoices = await _dbContext.Invoices
                .Where(i => i.CustomerId == customerId &&
                            i.IssueDate >= startDate &&
                            i.IssueDate <= endDate &&
                            !i.IsDeleted)
                .ToListAsync();

            for (var date = startDate; date <= endDate; date = date.AddMonths(1))
            {
                var monthlyTotal = invoices
                    .Where(i => i.IssueDate.Year == date.Year && i.IssueDate.Month == date.Month)
                    .Sum(i => i.TotalAmount);

                trend.Add(new SpendingTrendDto
                {
                    Month = date.ToString("MMM"),
                    Year = date.Year,
                    Amount = monthlyTotal
                });
            }

            return trend;
        }
    }
}