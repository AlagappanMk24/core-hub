using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;
using MediatR;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoicePdf;

/// <summary>
/// Query to generate PDF for an invoice
/// </summary>
public record GetInvoicePdfQuery : BaseQuery<InvoiceResponseDto>
{
    /// <summary>
    /// ID of the invoice to generate PDF for
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Creates a GetInvoicePdfQuery from invoice ID
    /// </summary>
    public static GetInvoicePdfQuery FromId(int id)
    {
        return new GetInvoicePdfQuery
        {
            Id = id
        };
    }
}