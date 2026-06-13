using FluentValidation;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoicesPaged;

/// <summary>
/// Validator for GetPagedInvoicesQuery
/// </summary>
public class GetPagedInvoicesQueryValidator : AbstractValidator<GetPagedInvoicesQuery>
{
    public GetPagedInvoicesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        RuleFor(x => x)
            .Must(x => !x.MinAmount.HasValue || !x.MaxAmount.HasValue || x.MinAmount <= x.MaxAmount)
            .WithMessage("Minimum amount cannot be greater than maximum amount");
    }
}