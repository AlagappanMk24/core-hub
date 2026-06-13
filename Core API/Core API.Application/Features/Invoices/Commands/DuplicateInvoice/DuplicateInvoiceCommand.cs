using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;
using MediatR;

namespace Core_API.Application.Features.Invoices.Commands.DuplicateInvoice;

/// <summary>
/// Command to duplicate an existing invoice
/// </summary>
public record DuplicateInvoiceCommand : BaseCommand<InvoiceResponseDto>
{
    /// <summary>
    /// ID of the invoice to duplicate
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Creates a DuplicateInvoiceCommand from invoice ID
    /// </summary>
    public static DuplicateInvoiceCommand FromId(int id)
    {
        return new DuplicateInvoiceCommand
        {
            Id = id
        };
    }
}