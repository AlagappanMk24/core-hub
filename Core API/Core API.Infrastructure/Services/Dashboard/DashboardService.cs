using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Dashboard;
using Core_API.Application.DTOs.Dashboard.Responses;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Dashboard
{
    public class DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger) : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DashboardService> _logger = logger;
        public async Task<OperationResult<DashboardSummaryDto>> GetAdminDashboardAsync(OperationContext operationContext)
        {
            try
            {
                IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);

                // Super Admin sees ALL invoices across all companies
                if (operationContext.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin dashboard - showing all invoices across all companies");
                    // No company filter - show everything
                }
                // Company Admin/User sees only their company's invoices
                else if (operationContext.CompanyId.HasValue)
                {
                    _logger.LogInformation($"Company Admin dashboard - showing invoices for company {operationContext.CompanyId}");
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);
                }
                else
                {
                    return OperationResult<DashboardSummaryDto>.FailureResult("Company ID is required for non-super admin users");
                }

                var invoices = await query
                      .Include(i => i.Customer)
                      .Include(i => i.InvoiceItems)
                      .ToListAsync();

                // Calculate stats
                var stats = CalculateStats(invoices, operationContext.CompanyId);

                // Get recent invoices
                var recentInvoices = GetRecentInvoicesInternal(invoices, 5);

                // Industry standard: Separate into logical groups
                var paymentProgress = GetPaymentProgressInvoices(invoices);      // Partially paid
                var pendingPayments = GetPendingPaymentsInvoices(invoices);      // Sent/Viewed but unpaid
                var overduePayments = GetOverduePaymentsInvoices(invoices);      // Overdue for collections
                var recentPayments = GetRecentPaidInvoices(invoices);            // Recently paid

                // Get monthly trends
                var monthlyTrend = GetMonthlyTrend(invoices);

                // For Super Admin, show trends by company or overall
                List<MonthlyInvoiceTrendDto> b2bTrend, b2cTrend, retailTrend;

                if (operationContext.IsSuperAdmin)
                {
                    // Super Admin can see B2B, B2C, Retail trends across all companies
                    b2bTrend = GetTrendByType(invoices, "B2B");
                    b2cTrend = GetTrendByType(invoices, "B2C");
                    retailTrend = GetTrendByType(invoices, "Retail");
                }
                else
                {
                    // Company-specific trends
                    b2bTrend = GetTrendByType(invoices, "B2B");
                    b2cTrend = GetTrendByType(invoices, "B2C");
                    retailTrend = GetTrendByType(invoices, "Retail");
                }

                var dashboard = new DashboardSummaryDto
                {
                    Stats = stats,
                    RecentInvoices = recentInvoices,
                    PaymentProgress = paymentProgress,      // In-progress payments
                    PendingPayments = pendingPayments,      // Awaiting payment
                    OverduePayments = overduePayments,      // Collections needed
                    RecentPayments = recentPayments,        // Recently completed
                    MonthlyTrend = monthlyTrend,
                    B2BTrend = b2bTrend,
                    B2CTrend = b2cTrend,
                    RetailTrend = retailTrend
                };

                return OperationResult<DashboardSummaryDto>.SuccessResult(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard");
                return OperationResult<DashboardSummaryDto>.FailureResult("Failed to retrieve dashboard data");
            }
        }
        public async Task<OperationResult<DashboardSummaryDto>> GetCustomerDashboardAsync(OperationContext operationContext)
        {
            try
            {
                IQueryable<Domain.Entities.Invoices.Invoice> query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);
                // Customer sees only their own invoices
                if (operationContext.CustomerId.HasValue)
                {
                    _logger.LogInformation($"Customer dashboard - showing invoices for customer {operationContext.CustomerId}");
                    query = query.Where(i => i.CustomerId == operationContext.CustomerId.Value);
                }
                else
                {
                    return OperationResult<DashboardSummaryDto>.FailureResult("Customer ID is required");
                }

                var invoices = await query
                  .Include(i => i.Customer)
                  .Include(i => i.InvoiceItems)
                  .ToListAsync();

                // Calculate stats
                var stats = CalculateStats(invoices, null);

                // Get recent invoices
                var recentInvoices = GetRecentInvoicesInternal(invoices, 5);

                // Industry standard: Separate into logical groups
                var paymentProgress = GetPaymentProgressInvoices(invoices);      // Partially paid
                var pendingPayments = GetPendingPaymentsInvoices(invoices);      // Sent/Viewed but unpaid
                var overduePayments = GetOverduePaymentsInvoices(invoices);      // Overdue for collections
                var recentPayments = GetRecentPaidInvoices(invoices);            // Recently paid

                // Get monthly trends (simplified for customers)
                var monthlyTrend = GetMonthlyTrend(invoices);

                var dashboard = new DashboardSummaryDto
                {
                    Stats = stats,
                    RecentInvoices = recentInvoices,
                    PaymentProgress = paymentProgress,      // In-progress payments
                    PendingPayments = pendingPayments,      // Awaiting payment
                    OverduePayments = overduePayments,      // Collections needed
                    RecentPayments = recentPayments,        // Recently completed
                    MonthlyTrend = monthlyTrend,
                    B2BTrend = new List<MonthlyInvoiceTrendDto>(),
                    B2CTrend = new List<MonthlyInvoiceTrendDto>(),
                    RetailTrend = new List<MonthlyInvoiceTrendDto>()
                };

                return OperationResult<DashboardSummaryDto>.SuccessResult(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer dashboard");
                return OperationResult<DashboardSummaryDto>.FailureResult("Failed to retrieve dashboard data");
            }
        }
        public async Task<OperationResult<List<RecentInvoiceDto>>> GetRecentInvoicesAsync(OperationContext operationContext, int count = 5)
        {
            try
            {
                var query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);

                if (operationContext.CompanyId.HasValue)
                {
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);
                }

                if (operationContext.CustomerId.HasValue)
                {
                    query = query.Where(i => i.CustomerId == operationContext.CustomerId.Value);
                }

                var invoices = await query
                    .Include(i => i.Customer)
                    .OrderByDescending(i => i.CreatedDate)
                    .Take(count)
                    .ToListAsync();

                var recentInvoices = invoices.Select(invoice => new RecentInvoiceDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CustomerName = invoice.Customer?.Name ?? "Unknown",
                    CustomerEmail = invoice.Customer?.Email.Value ?? "",
                    Amount = invoice.TotalAmount,
                    Status = invoice.PaymentStatus.ToString(),
                    IssueDate = invoice.IssueDate,
                    DueDate = invoice.DueDate
                }).ToList();

                return OperationResult<List<RecentInvoiceDto>>.SuccessResult(recentInvoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent invoices");
                return OperationResult<List<RecentInvoiceDto>>.FailureResult("Failed to retrieve recent invoices");
            }
        }
        public async Task<OperationResult<DashboardStatsDto>> GetStatsAsync(OperationContext operationContext)
        {
            try
            {
                var query = _unitOfWork.Invoices.Query().Where(i => !i.IsDeleted);

                if (operationContext.CompanyId.HasValue)
                {
                    query = query.Where(i => i.CompanyId == operationContext.CompanyId.Value);
                }

                if (operationContext.CustomerId.HasValue)
                {
                    query = query.Where(i => i.CustomerId == operationContext.CustomerId.Value);
                }

                var invoices = await query.ToListAsync();
                var stats = CalculateStats(invoices, operationContext.CompanyId);

                return OperationResult<DashboardStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats");
                return OperationResult<DashboardStatsDto>.FailureResult("Failed to retrieve stats");
            }
        }

        #region Private Helper Methods
        private DashboardStatsDto CalculateStats(List<Domain.Entities.Invoices.Invoice> invoices, int? companyId)
        {
            var currentPeriodStart = DateTime.UtcNow.AddDays(-30);
            var previousPeriodStart = DateTime.UtcNow.AddDays(-60);

            var currentInvoices = invoices.Where(i => i.CreatedDate >= currentPeriodStart).ToList();
            var previousInvoices = invoices.Where(i => i.CreatedDate >= previousPeriodStart && i.CreatedDate < currentPeriodStart).ToList();

            var currentTotal = currentInvoices.Sum(i => i.TotalAmount);
            var previousTotal = previousInvoices.Sum(i => i.TotalAmount);

            return new DashboardStatsDto
            {
                TotalInvoiceAmount = invoices.Sum(i => i.TotalAmount),
                TotalInvoices = invoices.Count,
                PendingInvoices = invoices.Count(i => i.PaymentStatus == PaymentStatus.Pending),
                PaidInvoices = invoices.Count(i => i.PaymentStatus == PaymentStatus.Paid),
                OverdueInvoices = invoices.Count(i => i.PaymentStatus == PaymentStatus.Overdue),
                DraftInvoices = invoices.Count(i => i.InvoiceStatus == InvoiceStatus.Draft),
                PercentageChangeTotal = CalculatePercentageChange(previousTotal, currentTotal),
                PercentageChangePending = CalculatePercentageChange(
                    previousInvoices.Count(i => i.PaymentStatus == PaymentStatus.Pending),
                    currentInvoices.Count(i => i.PaymentStatus == PaymentStatus.Pending)
                ),
                PercentageChangePaid = CalculatePercentageChange(
                    previousInvoices.Count(i => i.PaymentStatus == PaymentStatus.Paid),
                    currentInvoices.Count(i => i.PaymentStatus == PaymentStatus.Paid)
                )
            };
        }
        private List<RecentInvoiceDto> GetRecentInvoicesInternal(List<Domain.Entities.Invoices.Invoice> invoices, int count)
        {
            return invoices
                .OrderByDescending(i => i.CreatedDate)
                .Take(count)
                .Select(invoice => new RecentInvoiceDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CustomerName = invoice.Customer?.Name ?? "Unknown",
                    CustomerEmail = invoice.Customer?.Email.Value ?? "",
                    Amount = invoice.TotalAmount,
                    Status = invoice.PaymentStatus.ToString(),
                    IssueDate = invoice.IssueDate,
                    DueDate = invoice.DueDate
                }).ToList();
        }

        /// <summary>
        /// Invoices with partial payments - shows payment progress
        /// </summary>
        private List<InvoiceProgressDto> GetPaymentProgressInvoices(List<Domain.Entities.Invoices.Invoice> invoices)
        {
            return invoices
                .Where(i => i.PaymentStatus == PaymentStatus.PartiallyPaid)
                .OrderBy(i => i.DueDate)
                .Take(5)
                .Select(invoice => MapToProgressDto(invoice))
                .ToList();
        }

        /// <summary>
        /// Invoices sent but not yet paid - shows what's pending
        /// </summary>
        private List<InvoiceProgressDto> GetPendingPaymentsInvoices(List<Domain.Entities.Invoices.Invoice> invoices)
        {
            return invoices
                .Where(i => i.PaymentStatus == PaymentStatus.Pending &&
                           i.InvoiceStatus != InvoiceStatus.Overdue &&
                           i.InvoiceStatus != InvoiceStatus.Draft)
                .OrderBy(i => i.DueDate)
                .Take(5)
                .Select(invoice => MapToProgressDto(invoice, isPending: true))
                .ToList();
        }
        /// <summary>
        /// Overdue invoices - needs immediate attention/collections
        /// </summary>
        private List<InvoiceProgressDto> GetOverduePaymentsInvoices(List<Domain.Entities.Invoices.Invoice> invoices)
        {
            return invoices
                .Where(i => i.PaymentStatus == PaymentStatus.Overdue ||
                           i.PaymentStatus == PaymentStatus.Pending && i.DueDate < DateTime.UtcNow)
                .OrderBy(i => i.DueDate)
                .Take(5)
                .Select(invoice => MapToProgressDto(invoice, isOverdue: true))
                .ToList();
        }
        /// <summary>
        /// Recently paid invoices - shows completed transactions
        /// Industry standard: Show last 5 paid invoices with payment date
        /// </summary>
        private List<RecentInvoiceDto> GetRecentPaidInvoices(List<Domain.Entities.Invoices.Invoice> invoices)
        {
            return invoices
             .Where(i => i.PaymentStatus == PaymentStatus.Paid && i.PaidDate.HasValue)
             .OrderByDescending(i => i.PaidDate.Value) // Use PaidDate for sorting
                .Take(5)
                .Select(invoice => new RecentInvoiceDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CustomerName = invoice.Customer?.Name ?? "Unknown",
                    CustomerEmail = invoice.Customer?.Email.Value ?? "",
                    Amount = invoice.TotalAmount,
                    Status = "Paid",
                    IssueDate = invoice.IssueDate,
                    DueDate = invoice.DueDate,
                    PaymentDate = invoice.PaidDate
                }).ToList();
        }
        private InvoiceProgressDto MapToProgressDto(Domain.Entities.Invoices.Invoice invoice, bool isPending = false, bool isOverdue = false)
        {
            var paidPercentage = invoice.TotalAmount > 0 ? invoice.AmountPaid / invoice.TotalAmount * 100 : 0;
            return new InvoiceProgressDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.Customer?.Name ?? "Unknown",
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                AmountPaid = invoice.AmountPaid,
                RemainingAmount = invoice.TotalAmount - invoice.AmountPaid,
                PaidPercentage = paidPercentage,
                Status = isOverdue ? "Overdue" : isPending ? "Pending" : invoice.PaymentStatus.ToString(),
                StatusColor = GetStatusColorForDisplay(isOverdue, isPending, invoice.PaymentStatus),
                ProgressColor = GetProgressColor(paidPercentage / 100),
            };
        }
        private string GetStatusColorForDisplay(bool isOverdue, bool isPending, PaymentStatus status)
        {
            if (isOverdue) return "#EF4444";     // Red - Urgent
            if (isPending) return "#F59E0B";     // Orange - Warning
            return status switch
            {
                PaymentStatus.PartiallyPaid => "#8B5CF6",  // Purple - In Progress
                PaymentStatus.Paid => "#10B981",           // Green - Completed
                _ => "#6B7280"                             // Gray - Other
            };
        }
        private string GetProgressColor(decimal percentage)
        {
            if (percentage >= 0.75m) return "#10B981";  // Green - Almost complete
            if (percentage >= 0.5m) return "#F59E0B";   // Orange - Halfway
            if (percentage >= 0.25m) return "#EF4444";  // Red - Just started
            return "#8B5CF6";                            // Purple - New
        }
        private List<MonthlyInvoiceTrendDto> GetMonthlyTrend(List<Domain.Entities.Invoices.Invoice> invoices)
        {
            var currentYear = DateTime.UtcNow.Year;
            var months = Enumerable.Range(1, 12);

            return months.Select(month => new MonthlyInvoiceTrendDto
            {
                Month = new DateTime(currentYear, month, 1).ToString("MMM"),
                Year = currentYear,
                Amount = invoices.Where(i => i.IssueDate.Year == currentYear && i.IssueDate.Month == month)
                                 .Sum(i => i.TotalAmount),
                Count = invoices.Count(i => i.IssueDate.Year == currentYear && i.IssueDate.Month == month)
            }).ToList();
        }
        private List<MonthlyInvoiceTrendDto> GetTrendByType(List<Domain.Entities.Invoices.Invoice> invoices, string type)
        {
            var currentYear = DateTime.UtcNow.Year;
            var months = Enumerable.Range(1, 12);

            return months.Select(month => new MonthlyInvoiceTrendDto
            {
                Month = new DateTime(currentYear, month, 1).ToString("MMM"),
                Year = currentYear,
                Amount = invoices.Where(i => i.IssueDate.Year == currentYear && i.IssueDate.Month == month)
                                 .Sum(i => i.TotalAmount) * GetTypeMultiplier(type),
                Count = invoices.Count(i => i.IssueDate.Year == currentYear && i.IssueDate.Month == month) / 2
            }).ToList();
        }
        private decimal CalculatePercentageChange(decimal previous, decimal current)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round((current - previous) / previous * 100, 2);
        }
        private decimal GetTypeMultiplier(string type)
        {
            return type switch
            {
                "B2B" => 1.0m,
                "B2C" => 0.6m,
                "Retail" => 0.4m,
                _ => 0.5m
            };
        }

        #endregion
    }
}
