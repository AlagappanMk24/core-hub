using Core_API.Application.Common.Models;
using Core_API.Application.DTOs.Invoice.Request;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoicesPaged;

/// <summary>
/// Extension methods for mapping InvoiceFilterRequestDto to GetPagedInvoicesQuery
/// </summary>
public static class GetPagedInvoicesQueryMapper
{
    /// <summary>
    /// Converts InvoiceFilterRequestDto to GetPagedInvoicesQuery
    /// </summary>
    /// <param name="filter">The filter DTO</param>
    /// <param name="context">The operation context</param>
    /// <returns>A populated GetPagedInvoicesQuery</returns>
    public static GetPagedInvoicesQuery ToQuery(this InvoiceFilterRequestDto filter, OperationContext context)
    {
        return new GetPagedInvoicesQuery
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            Search = filter.Search,
            InvoiceStatus = filter.InvoiceStatus,
            PaymentStatus = filter.PaymentStatus,
            CustomerId = filter.CustomerId,
            TaxType = filter.TaxType,
            MinAmount = filter.MinAmount,
            MaxAmount = filter.MaxAmount,
            InvoiceNumberFrom = filter.InvoiceNumberFrom,
            InvoiceNumberTo = filter.InvoiceNumberTo,
            IssueDateFrom = filter.IssueDateFrom,
            IssueDateTo = filter.IssueDateTo,
            DueDateFrom = filter.DueDateFrom,
            DueDateTo = filter.DueDateTo,
            Context = context
        };
    }
}