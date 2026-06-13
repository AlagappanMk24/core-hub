using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Email.Requests;
using MediatR;

namespace Core_API.Application.Features.Invoices.Commands.SendInvoice;

/// <summary>
/// Command to send an invoice to its recipient
/// </summary>
public record SendInvoiceCommand : BaseCommand<bool>
{
    /// <summary>
    /// ID of the invoice to send
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Email data for sending the invoice
    /// </summary>
    public EmailDataDto EmailData { get; set; } = new();

    /// <summary>
    /// Creates a SendInvoiceCommand from invoice ID and email data
    /// </summary>
    public static SendInvoiceCommand Create(int id, EmailDataDto emailData)
    {
        return new SendInvoiceCommand
        {
            Id = id,
            EmailData = emailData
        };
    }
}