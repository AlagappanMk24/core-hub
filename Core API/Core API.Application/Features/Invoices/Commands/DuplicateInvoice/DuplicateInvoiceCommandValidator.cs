using FluentValidation;

namespace Core_API.Application.Features.Invoices.Commands.DuplicateInvoice;

/// <summary>
/// Validator for DuplicateInvoiceCommand
/// </summary>
public class DuplicateInvoiceCommandValidator : AbstractValidator<DuplicateInvoiceCommand>
{
    public DuplicateInvoiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invoice ID must be greater than 0.");
    }
}