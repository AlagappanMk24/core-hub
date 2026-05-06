using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Invoicing.InvoiceStatistics
{
    /// <summary>
    /// Implementation of invoice statistics service
    /// </summary>
    public class InvoiceStatisticsService(IUnitOfWork unitOfWork, ILogger<InvoiceStatisticsService> logger) : IInvoiceStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly ILogger<InvoiceStatisticsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<OperationResult<InvoiceStatsDto>> GetStatsAsync(OperationContext operationContext)
        {
            try
            {
                // Build the base query
                var query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);

                // Apply filters based on user role
                if (operationContext.IsSuperAdmin)
                {
                    // Super Admin sees ALL invoices across all companies
                    _logger.LogInformation("Super Admin stats - showing all invoices across all companies");
                    // No filters applied
                }
                else if (operationContext.CompanyId.HasValue)
                {
                    // Company Admin/User sees only their company's invoices
                    _logger.LogInformation($"Company stats - filtering by CompanyId: {operationContext.CompanyId}");
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);

                    // If this is a customer role, further filter by CustomerId
                    if (operationContext.CustomerId.HasValue && operationContext.Roles.Contains("Customer"))
                    {
                        _logger.LogInformation($"Customer stats - further filtering by CustomerId: {operationContext.CustomerId}");
                        query = query.Where(i => i.CustomerId == operationContext.CustomerId.Value);
                    }
                }
                else
                {
                    _logger.LogWarning("No CompanyId provided and user is not Super Admin");
                    return OperationResult<InvoiceStatsDto>.FailureResult("Company ID is required for non-super admin users.");
                }

                // Get all invoices for the company/customer
                var invoices = await query.ToListAsync();

                _logger.LogInformation("Retrieved {Count} invoices for stats calculation", invoices.Count);

                // Calculate previous period for change (last 30 days vs previous 30 days)
                var today = DateTime.UtcNow.Date;
                var currentPeriodStart = today.AddDays(-30);
                var previousPeriodStart = today.AddDays(-60);
                var currentPeriodEnd = today;
                var previousPeriodEnd = currentPeriodStart;

                var currentPeriodInvoices = invoices.Where(i => i.IssueDate >= currentPeriodStart && i.IssueDate < currentPeriodEnd).ToList();
                var previousPeriodInvoices = invoices.Where(i => i.IssueDate >= previousPeriodStart && i.IssueDate < previousPeriodEnd).ToList();

                var currentPeriodAmount = currentPeriodInvoices.Sum(i => i.TotalAmount);
                var previousPeriodAmount = previousPeriodInvoices.Sum(i => i.TotalAmount);

                var stats = new InvoiceStatsDto
                {
                    All = new StatsItem
                    {
                        Count = invoices.Count,
                        Amount = invoices.Sum(i => i.TotalAmount),
                        Change = CalculateChange(previousPeriodAmount, currentPeriodAmount)
                    },
                    Draft = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Draft),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Draft).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForStatus(invoices, InvoiceStatus.Draft, previousPeriodStart, currentPeriodStart)
                    },
                    Sent = new StatsItem // Same as Open
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Sent),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Sent).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForStatus(invoices, InvoiceStatus.Sent, previousPeriodStart, currentPeriodStart)
                    },
                    Viewed = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Viewed),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Viewed).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForStatus(invoices, InvoiceStatus.Viewed, previousPeriodStart, currentPeriodStart)
                    },
                    PartiallyPaid = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.PartiallyPaid),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.PartiallyPaid).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForPaymentStatus(invoices, PaymentStatus.PartiallyPaid, previousPeriodStart, currentPeriodStart)
                    },
                    Paid = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Paid),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Paid).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForPaymentStatus(invoices, PaymentStatus.Paid, previousPeriodStart, currentPeriodStart)
                    },
                    Overdue = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Overdue),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Overdue).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForPaymentStatus(invoices, PaymentStatus.Overdue, previousPeriodStart, currentPeriodStart)
                    },
                    Void = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Void),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Void).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForStatus(invoices, InvoiceStatus.Void, previousPeriodStart, currentPeriodStart)
                    },
                    Cancelled = new StatsItem
                    {
                        Count = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Void),
                        Amount = invoices.Where(i => i.InvoiceStatus == InvoiceStatus.Void).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForStatus(invoices, InvoiceStatus.Void, previousPeriodStart, currentPeriodStart)
                    },
                    Refunded = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Refunded),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Refunded).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForPaymentStatus(invoices, PaymentStatus.Refunded, previousPeriodStart, currentPeriodStart)
                    },
                    Pending = new StatsItem
                    {
                        Count = invoices.Count(i => i.PaymentStatus == PaymentStatus.Pending),
                        Amount = invoices.Where(i => i.PaymentStatus == PaymentStatus.Pending).Sum(i => i.TotalAmount),
                        Change = CalculateChangeForPaymentStatus(invoices, PaymentStatus.Pending, previousPeriodStart, currentPeriodStart)
                    }
                };
                _logger.LogInformation("Stats calculated: Total Invoices={Count}, Total Amount={Amount}", stats.All.Count, stats.All.Amount);

                return OperationResult<InvoiceStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<InvoiceStatsDto>.FailureResult("Failed to retrieve stats.");
            }
        }

        #region Private Helpers
        private static decimal CalculateChange(decimal previous, decimal current)
        {
            // If previous period had no invoices
            if (previous == 0)
            {
                // If current also has no invoices, no change
                if (current == 0) return 0;
                // If there are invoices now, it's a 100% increase
                return 100;
            }
            // Standard percentage change formula
            // ((current - previous) / previous) * 100
            return Math.Round((current - previous) / previous * 100, 2);
        }
        private decimal CalculateChangeForStatus(List<Domain.Entities.Invoices.Invoice> invoices, InvoiceStatus status, DateTime previousPeriodStart, DateTime currentPeriodStart)
        {
            var previousCount = invoices.Count(i => i.InvoiceStatus == status &&
                i.IssueDate >= previousPeriodStart && i.IssueDate < currentPeriodStart);
            var currentCount = invoices.Count(i => i.InvoiceStatus == status &&
                i.IssueDate >= currentPeriodStart);

            // Industry standard approach
            if (previousCount == 0)
            {
                if (currentCount == 0) return 0;
                return 100; // 100% increase from zero
            }

            return Math.Round((decimal)(currentCount - previousCount) / previousCount * 100, 2);
        }
        private decimal CalculateChangeForPaymentStatus(List<Domain.Entities.Invoices.Invoice> invoices, PaymentStatus status, DateTime previousPeriodStart, DateTime currentPeriodStart)
        {
            var previousCount = invoices.Count(i => i.PaymentStatus == status &&
                i.IssueDate >= previousPeriodStart && i.IssueDate < currentPeriodStart);
            var currentCount = invoices.Count(i => i.PaymentStatus == status &&
                i.IssueDate >= currentPeriodStart);

            // Industry standard approach
            if (previousCount == 0)
            {
                if (currentCount == 0) return 0;
                return 100; // 100% increase from zero
            }
            return Math.Round((decimal)(currentCount - previousCount) / previousCount * 100, 2);
        }
        #endregion
    }
}