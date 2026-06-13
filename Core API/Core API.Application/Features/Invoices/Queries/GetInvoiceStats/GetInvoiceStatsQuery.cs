using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;
using MediatR;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoiceStats;

/// <summary>
/// Query to retrieve invoice statistics for the authenticated user
/// </summary>
public record GetInvoiceStatsQuery : BaseQuery<InvoiceStatsDto>
{
    /// <summary>
    /// Creates a new instance of GetInvoiceStatsQuery
    /// </summary>
    public static GetInvoiceStatsQuery Create()
    {
        return new GetInvoiceStatsQuery();
    }
}