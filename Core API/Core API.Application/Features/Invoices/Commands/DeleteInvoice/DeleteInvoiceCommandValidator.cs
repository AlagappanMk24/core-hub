// Core_API.Application/Features/Invoices/Commands/DeleteInvoice/DeleteInvoiceCommandValidator.cs
using FluentValidation;

namespace Core_API.Application.Features.Invoices.Commands.DeleteInvoice;

/// <summary>
/// Validator for DeleteInvoiceCommand
/// </summary>
public sealed class DeleteInvoiceCommandValidator : AbstractValidator<DeleteInvoiceCommand>
{
    public DeleteInvoiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invoice ID must be greater than 0.");
    }
}