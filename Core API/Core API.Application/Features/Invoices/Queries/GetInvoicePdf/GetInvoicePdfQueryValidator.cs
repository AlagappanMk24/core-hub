using FluentValidation;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoicePdf;

/// <summary>
/// Validator for GetInvoicePdfQuery
/// </summary>
public class GetInvoicePdfQueryValidator : AbstractValidator<GetInvoicePdfQuery>
{
    public GetInvoicePdfQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invoice ID must be greater than 0.");
    }
}