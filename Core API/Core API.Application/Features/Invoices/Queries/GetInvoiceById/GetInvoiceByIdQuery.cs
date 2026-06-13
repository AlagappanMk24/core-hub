using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;
using MediatR;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoiceById;

/// <summary>
/// Query to retrieve an invoice by its ID
/// </summary>
public record GetInvoiceByIdQuery : BaseQuery<InvoiceResponseDto>
{
    /// <summary>
    /// ID of the invoice to retrieve
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Creates a GetInvoiceByIdQuery from invoice ID
    /// </summary>
    public static GetInvoiceByIdQuery FromId(int id)
    {
        return new GetInvoiceByIdQuery
        {
            Id = id
        };
    }
}