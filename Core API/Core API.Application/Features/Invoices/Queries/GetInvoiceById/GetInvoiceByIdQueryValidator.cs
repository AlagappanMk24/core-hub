using FluentValidation;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoiceById;

/// <summary>
/// Validator for GetInvoiceByIdQuery
/// </summary>
public class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Invoice ID must be greater than 0.");
    }
}