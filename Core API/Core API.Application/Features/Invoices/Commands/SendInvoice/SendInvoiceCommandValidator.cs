using FluentValidation;
using Core_API.Application.DTOs.Email.Requests;

namespace Core_API.Application.Features.Invoices.Commands.SendInvoice;

/// <summary>
/// Validator for SendInvoiceCommand
/// </summary>
public class SendInvoiceCommandValidator : AbstractValidator<SendInvoiceCommand>
{
    public SendInvoiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invoice ID must be greater than 0.");

        RuleFor(x => x.EmailData)
            .NotNull()
            .WithMessage("Email data is required.");

        RuleFor(x => x.EmailData.To)
            .NotEmpty()
            .WithMessage("At least one recipient email address is required.")
            .Must(to => to != null && to.Any(e => !string.IsNullOrWhiteSpace(e)))
            .WithMessage("At least one valid 'To' email address is required.");

        RuleForEach(x => x.EmailData.To)
              .EmailAddress()
              .WithMessage("Invalid email address format in 'To' field.");

        RuleForEach(x => x.EmailData.Cc)
             .EmailAddress()
             .WithMessage("Invalid email address format in 'Cc' field.");

        RuleForEach(x => x.EmailData.Bcc)
            .EmailAddress()
            .WithMessage("Invalid email address format in 'Bcc' field.");

        RuleFor(x => x.EmailData.Subject)
            .MaximumLength(500)
            .WithMessage("Subject cannot exceed 500 characters.");

        RuleFor(x => x.EmailData.Message)
            .MaximumLength(5000)
            .WithMessage("Message cannot exceed 5000 characters.");
    }
}