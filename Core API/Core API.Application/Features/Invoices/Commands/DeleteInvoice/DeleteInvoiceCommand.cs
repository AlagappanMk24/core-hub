using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;

namespace Core_API.Application.Features.Invoices.Commands.DeleteInvoice;

/// <summary>
/// Command to delete an invoice (soft delete)
/// </summary>
public record DeleteInvoiceCommand : BaseCommand<bool>
{
    /// <summary>
    /// ID of the invoice to delete
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Creates a DeleteInvoiceCommand from invoice ID
    /// </summary>
    public static DeleteInvoiceCommand Create(int id)
    {
        return new DeleteInvoiceCommand { Id = id };
    }
}