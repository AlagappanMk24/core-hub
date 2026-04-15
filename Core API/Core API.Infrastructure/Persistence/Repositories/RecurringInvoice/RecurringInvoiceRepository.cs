using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence.RecurringInvoice;
using Core_API.Application.DTOs.RecurringInvoice.Request;
using Core_API.Domain.Enums;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Persistence.Repositories.RecurringInvoice
{
    public class RecurringInvoiceRepository(CoreInvoiceDbContext dbContext, ILogger<RecurringInvoiceRepository> logger) : GenericRepository<Domain.Entities.RecurringInvoice>(dbContext), IRecurringInvoiceRepository
    {
        private readonly CoreInvoiceDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly ILogger<RecurringInvoiceRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<PaginatedResult<Domain.Entities.RecurringInvoice>> GetPagedAsync(
            int companyId,
            RecurringInvoiceFilterDto filter)
        {
            try
            {
                IQueryable<Domain.Entities.RecurringInvoice> query = dbset
                    .Where(r => r.CompanyId == companyId && !r.IsDeleted)
                    .Include(r => r.Customer)
                    .Include(r => r.SourceInvoice);

                // Apply search filter
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    filter.Search = filter.Search.ToLower();
                    query = query.Where(r =>
                        r.Name.ToLower().Contains(filter.Search) ||
                        (r.Description != null && r.Description.ToLower().Contains(filter.Search)) ||
                        r.Customer.Name.ToLower().Contains(filter.Search));
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<RecurringInvoiceStatus>(filter.Status, true, out var status))
                {
                    query = query.Where(r => r.Status == status);
                }

                // Apply frequency filter
                if (!string.IsNullOrEmpty(filter.Frequency) && Enum.TryParse<RecurringFrequency>(filter.Frequency, true, out var frequency))
                {
                    query = query.Where(r => r.Frequency == frequency);
                }

                // Apply customer filter
                if (filter.CustomerId.HasValue)
                {
                    query = query.Where(r => r.CustomerId == filter.CustomerId.Value);
                }

                // Apply date filters
                if (filter.NextDateFrom.HasValue)
                {
                    query = query.Where(r => r.NextInvoiceDate >= filter.NextDateFrom.Value);
                }

                if (filter.NextDateTo.HasValue)
                {
                    query = query.Where(r => r.NextInvoiceDate <= filter.NextDateTo.Value);
                }

                if (filter.StartDateFrom.HasValue)
                {
                    query = query.Where(r => r.StartDate >= filter.StartDateFrom.Value);
                }

                if (filter.StartDateTo.HasValue)
                {
                    query = query.Where(r => r.StartDate <= filter.StartDateTo.Value);
                }

                if (filter.EndDateFrom.HasValue)
                {
                    query = query.Where(r => r.EndDate >= filter.EndDateFrom.Value);
                }

                if (filter.EndDateTo.HasValue)
                {
                    query = query.Where(r => r.EndDate <= filter.EndDateTo.Value);
                }

                // Apply amount filters
                if (filter.MinAmount.HasValue)
                {
                    query = query.Where(r => r.TotalAmount >= filter.MinAmount.Value);
                }

                if (filter.MaxAmount.HasValue)
                {
                    query = query.Where(r => r.TotalAmount <= filter.MaxAmount.Value);
                }

                // Apply auto-send filter
                if (filter.AutoSend.HasValue)
                {
                    query = query.Where(r => r.AutoSend == filter.AutoSend.Value);
                }

                // Apply sorting
                query = ApplySorting(query, filter.SortBy, filter.SortOrder);

                var totalCount = await query.CountAsync();

                var items = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return new PaginatedResult<Domain.Entities.RecurringInvoice>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged recurring invoices for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<List<Domain.Entities.RecurringInvoice>> GetDueInvoicesAsync(DateTime asOfDate)
        {
            try
            {
                return await dbset
                    .Where(r => !r.IsDeleted &&
                                r.Status == RecurringInvoiceStatus.Active &&
                                r.NextInvoiceDate.Date <= asOfDate.Date &&
                                (!r.EndDate.HasValue || r.EndDate.Value.Date >= asOfDate.Date) &&
                                (!r.MaxOccurrences.HasValue || r.OccurrencesGenerated < r.MaxOccurrences.Value))
                    .Include(r => r.Customer)
                    .Include(r => r.Company)
                    .Include(r => r.SourceInvoice)
                        .ThenInclude(i => i.InvoiceItems)
                    .Include(r => r.SourceInvoice)
                        .ThenInclude(i => i.TaxDetails)
                    .Include(r => r.SourceInvoice)
                        .ThenInclude(i => i.Discounts)
                    .OrderBy(r => r.NextInvoiceDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting due recurring invoices as of {AsOfDate}", asOfDate);
                throw;
            }
        }

        public async Task<Domain.Entities.RecurringInvoice> GetWithDetailsAsync(int id, int companyId)
        {
            try
            {
                return await dbset
                    .Where(r => r.Id == id && r.CompanyId == companyId && !r.IsDeleted)
                    .Include(r => r.Customer)
                    .Include(r => r.Company)
                    .Include(r => r.SourceInvoice)
                        .ThenInclude(i => i.InvoiceItems)
                    .Include(r => r.SourceInvoice)
                        .ThenInclude(i => i.TaxDetails)
                    .Include(r => r.SourceInvoice)
                        .ThenInclude(i => i.Discounts)
                    .Include(r => r.GeneratedInvoices)
                        .ThenInclude(g => g.Invoice)
                    .Include(r => r.AuditLogs)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recurring invoice with details for ID {RecurringInvoiceId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int companyId, string name, int? excludeId = null)
        {
            try
            {
                var query = dbset.Where(r => r.CompanyId == companyId && r.Name == name && !r.IsDeleted);

                if (excludeId.HasValue)
                {
                    query = query.Where(r => r.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of recurring invoice with name {Name} for company {CompanyId}", name, companyId);
                throw;
            }
        }

        public async Task<int> GetNextSequenceNumberAsync(int recurringInvoiceId)
        {
            try
            {
                var maxSequence = await _dbContext.RecurringInvoiceInstances
                    .Where(i => i.RecurringInvoiceId == recurringInvoiceId && !i.IsDeleted)
                    .MaxAsync(i => (int?)i.SequenceNumber) ?? 0;

                return maxSequence + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next sequence number for recurring invoice {RecurringInvoiceId}", recurringInvoiceId);
                throw;
            }
        }

        public async Task<List<Domain.Entities.RecurringInvoice>> GetByCustomerAsync(int customerId, int companyId)
        {
            try
            {
                return await dbset
                    .Where(r => r.CustomerId == customerId && r.CompanyId == companyId && !r.IsDeleted)
                    .Include(r => r.Customer)
                    .Include(r => r.GeneratedInvoices)
                        .ThenInclude(g => g.Invoice)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recurring invoices for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetStatusCountsAsync(int companyId)
        {
            try
            {
                var counts = await dbset
                    .Where(r => r.CompanyId == companyId && !r.IsDeleted)
                    .GroupBy(r => r.Status)
                    .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

                // Ensure all statuses are represented with default 0
                foreach (var status in Enum.GetValues<RecurringInvoiceStatus>())
                {
                    var statusName = status.ToString();
                    if (!counts.ContainsKey(statusName))
                    {
                        counts[statusName] = 0;
                    }
                }

                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status counts for company {CompanyId}", companyId);
                throw;
            }
        }

        private IQueryable<Domain.Entities.RecurringInvoice> ApplySorting(
            IQueryable<Domain.Entities.RecurringInvoice> query,
            string sortBy,
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "name" => isDescending
                    ? query.OrderByDescending(r => r.Name)
                    : query.OrderBy(r => r.Name),

                "customer" => isDescending
                    ? query.OrderByDescending(r => r.Customer.Name)
                    : query.OrderBy(r => r.Customer.Name),

                "totalamount" => isDescending
                    ? query.OrderByDescending(r => r.TotalAmount)
                    : query.OrderBy(r => r.TotalAmount),

                "startdate" => isDescending
                    ? query.OrderByDescending(r => r.StartDate)
                    : query.OrderBy(r => r.StartDate),

                "enddate" => isDescending
                    ? query.OrderByDescending(r => r.EndDate)
                    : query.OrderBy(r => r.EndDate),

                "nextinvoicedate" => isDescending
                    ? query.OrderByDescending(r => r.NextInvoiceDate)
                    : query.OrderBy(r => r.NextInvoiceDate),

                "lastinvoicedate" => isDescending
                    ? query.OrderByDescending(r => r.LastInvoiceDate)
                    : query.OrderBy(r => r.LastInvoiceDate),

                "status" => isDescending
                    ? query.OrderByDescending(r => r.Status)
                    : query.OrderBy(r => r.Status),

                "frequency" => isDescending
                    ? query.OrderByDescending(r => r.Frequency)
                    : query.OrderBy(r => r.Frequency),

                "createddate" => isDescending
                    ? query.OrderByDescending(r => r.CreatedDate)
                    : query.OrderBy(r => r.CreatedDate),

                "updateddate" => isDescending
                    ? query.OrderByDescending(r => r.UpdatedDate)
                    : query.OrderBy(r => r.UpdatedDate),

                _ => query.OrderBy(r => r.NextInvoiceDate)
            };
        }
    }
}
