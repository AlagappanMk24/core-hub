using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Dashboard;

namespace Core_API.Application.Contracts.Services
{
    public interface IDashboardService
    {
        Task<OperationResult<DashboardSummaryDto>> GetAdminDashboardAsync(OperationContext operationContext);
        Task<OperationResult<DashboardSummaryDto>> GetCustomerDashboardAsync(OperationContext operationContext);
        Task<OperationResult<List<RecentInvoiceDto>>> GetRecentInvoicesAsync(OperationContext operationContext, int count = 5);
        Task<OperationResult<DashboardStatsDto>> GetStatsAsync(OperationContext operationContext);
    }
}