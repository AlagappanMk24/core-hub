using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;

namespace Core_API.Application.Contracts.Services.Invoice
{
    /// <summary>
    /// Service for invoice statistics and analytics
    /// </summary>
    public interface IInvoiceStatisticsService
    {
        Task<OperationResult<InvoiceStatsDto>> GetStatsAsync(OperationContext operationContext);
    }
}