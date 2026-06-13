using Core_API.Domain.Enums;
using FluentValidation;

namespace Core_API.Application.Features.Invoices.Commands.CreateInvoice;

/// <summary>
/// Validator for CreateInvoiceCommand
/// </summary>
public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    private static readonly HashSet<string> SupportedCurrencies = new()
    {
        "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "INR", "NZD"
    };

    public CreateInvoiceCommandValidator()
    {
        // Invoice Number Validation
        When(x => !x.IsAutomated, () =>
        {
            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("Invoice number is required for non-automated invoices.")
                .MaximumLength(50).WithMessage("Invoice number cannot exceed 50 characters.");
        });

        // Date Validations
        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("Issue date is required.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required.")
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("Due date must be greater than or equal to issue date.");

        // Customer Validation
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required and must be greater than 0.");

        // Currency Validation
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.")
            .Must(BeSupportedCurrency).WithMessage($"Invalid currency. Supported currencies: {string.Join(", ", SupportedCurrencies)}");

        RuleFor(x => x.CurrencyRate)
            .GreaterThan(0).WithMessage("Currency rate must be greater than 0.");

        // Financial Validations
        RuleFor(x => x.ShippingAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Shipping amount cannot be negative.");

        RuleFor(x => x.AdjustmentAmount)
            .GreaterThanOrEqualTo(-999999.99m).WithMessage("Adjustment amount is out of range.")
            .LessThanOrEqualTo(999999.99m).WithMessage("Adjustment amount is out of range.");

        // Collections Validations
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one invoice item is required.")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("At least one invoice item is required.");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.Description)
                    .NotEmpty().WithMessage("Item description is required.")
                    .MaximumLength(500).WithMessage("Item description cannot exceed 500 characters.");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be at least 1.");

                item.RuleFor(i => i.UnitPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("Unit price must be a positive value.");
            });

        // Discount Validations
        RuleForEach(x => x.Discounts)
            .ChildRules(discount =>
            {
                discount.RuleFor(d => d.Description)
                    .MaximumLength(100).WithMessage("Discount description cannot exceed 100 characters.");

                discount.RuleFor(d => d.Amount)
                    .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.");

                discount.RuleFor(d => d.DiscountType)
                    .IsInEnum().WithMessage("Invalid discount type.");

                discount.RuleFor(d => d)
                    .Must(d => !(d.DiscountType == DiscountType.Percentage && d.Amount > 100))
                    .WithMessage("Percentage discount cannot exceed 100%.");
            });

        // Tax Detail Validations
        RuleForEach(x => x.TaxDetails)
            .ChildRules(tax =>
            {
                tax.RuleFor(t => t.TaxName)
                    .NotEmpty().WithMessage("Tax name is required.")
                    .MaximumLength(50).WithMessage("Tax name cannot exceed 50 characters.");

                tax.RuleFor(t => t.Rate)
                    .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100%.");

                tax.RuleFor(t => t.TaxAmount)
                    .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative.");
            });
    }

    private static bool BeSupportedCurrency(string currency)
    {
        return SupportedCurrencies.Contains(currency.ToUpperInvariant());
    }
}