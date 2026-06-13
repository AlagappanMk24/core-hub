using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Enums;
using MediatR;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoiceStats;

/// <summary>
/// Handler for GetInvoiceStatsQuery
/// </summary>
public sealed class GetInvoiceStatsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetInvoiceStatsQueryHandler> logger) : IRequestHandler<GetInvoiceStatsQuery, OperationResult<InvoiceStatsDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GetInvoiceStatsQueryHandler> _logger = logger;

    public async Task<OperationResult<InvoiceStatsDto>> Handle(
        GetInvoiceStatsQuery request,
        CancellationToken cancellationToken)
    {
        var context = request.Context;

        try
        {
            _logger.LogInformation("Retrieving invoice stats for UserId: {UserId}, IsSuperAdmin: {IsSuperAdmin}, CompanyId: {CompanyId}, CustomerId: {CustomerId}",
                context.UserId, context.IsSuperAdmin, context.CompanyId, context.CustomerId);

            // Build the base query
            var query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);

            // Apply filters based on user role
            if (context.IsSuperAdmin)
            {
                // Super Admin sees ALL invoices across all companies
                _logger.LogInformation("Super Admin stats - showing all invoices across all companies");
                // No filters applied
            }
            else if (context.CompanyId.HasValue)
            {
                // Company Admin/User sees only their company's invoices
                _logger.LogInformation("Company stats - filtering by CompanyId: {CompanyId}", context.CompanyId);
                query = query.Where(i => i.CompanyId == context.CompanyId.Value);

                // If this is a customer role, further filter by CustomerId
                if (context.CustomerId.HasValue && context.Roles.Contains("Customer"))
                {
                    _logger.LogInformation("Customer stats - further filtering by CustomerId: {CustomerId}", context.CustomerId);
                    query = query.Where(i => i.CustomerId == context.CustomerId.Value);
                }
            }
            else
            {
                _logger.LogWarning("No CompanyId provided and user is not Super Admin");
                return OperationResult<InvoiceStatsDto>.FailureResult("Company ID is required for non-super admin users.");
            }

            // Get all invoices for stats calculation
            var invoices = await query.ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} invoices for stats calculation", invoices.Count);

            // Calculate previous period for change (last 30 days vs previous 30 days)
            var today = DateTime.UtcNow.Date;
            var currentPeriodStart = today.AddDays(-30);
            var previousPeriodStart = today.AddDays(-60);
            var currentPeriodEnd = today;

            var currentPeriodInvoices = invoices.Where(i => i.IssueDate >= currentPeriodStart && i.IssueDate < currentPeriodEnd).ToList();
            var previousPeriodInvoices = invoices.Where(i => i.IssueDate >= previousPeriodStart && i.IssueDate < currentPeriodStart).ToList();

            var currentPeriodAmount = currentPeriodInvoices.Sum(i => i.TotalAmount);
            var previousPeriodAmount = previousPeriodInvoices.Sum(i => i.TotalAmount);

            // Calculate stats
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
                Sent = new StatsItem
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

            _logger.LogInformation("Stats calculated successfully: Total Invoices={Count}, Total Amount={Amount}",
                stats.All.Count, stats.All.Amount);

            return OperationResult<InvoiceStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice stats for UserId: {UserId}, CompanyId: {CompanyId}",
                context.UserId, context.CompanyId);
            return OperationResult<InvoiceStatsDto>.FailureResult("Failed to retrieve invoice statistics: " + ex.Message);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculates percentage change between previous and current period amounts
    /// </summary>
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
        // Standard percentage change formula: ((current - previous) / previous) * 100
        return Math.Round((current - previous) / previous * 100, 2);
    }

    /// <summary>
    /// Calculates percentage change for invoice status counts
    /// </summary>
    private static decimal CalculateChangeForStatus(
        List<Domain.Entities.Invoices.Invoice> invoices,
        InvoiceStatus status,
        DateTime previousPeriodStart,
        DateTime currentPeriodStart)
    {
        var previousCount = invoices.Count(i => i.InvoiceStatus == status &&
            i.IssueDate >= previousPeriodStart && i.IssueDate < currentPeriodStart);

        var currentCount = invoices.Count(i => i.InvoiceStatus == status &&
            i.IssueDate >= currentPeriodStart);

        // Handle zero previous count
        if (previousCount == 0)
        {
            if (currentCount == 0) return 0;
            return 100; // 100% increase from zero
        }

        return Math.Round((decimal)(currentCount - previousCount) / previousCount * 100, 2);
    }

    /// <summary>
    /// Calculates percentage change for payment status counts
    /// </summary>
    private static decimal CalculateChangeForPaymentStatus(
        List<Domain.Entities.Invoices.Invoice> invoices,
        PaymentStatus status,
        DateTime previousPeriodStart,
        DateTime currentPeriodStart)
    {
        var previousCount = invoices.Count(i => i.PaymentStatus == status &&
            i.IssueDate >= previousPeriodStart && i.IssueDate < currentPeriodStart);

        var currentCount = invoices.Count(i => i.PaymentStatus == status &&
            i.IssueDate >= currentPeriodStart);

        // Handle zero previous count
        if (previousCount == 0)
        {
            if (currentCount == 0) return 0;
            return 100; // 100% increase from zero
        }

        return Math.Round((decimal)(currentCount - previousCount) / previousCount * 100, 2);
    }

    #endregion
}