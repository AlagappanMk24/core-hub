using FluentValidation;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoiceStats;

/// <summary>
/// Validator for GetInvoiceStatsQuery
/// </summary>
public class GetInvoiceStatsQueryValidator : AbstractValidator<GetInvoiceStatsQuery>
{
    public GetInvoiceStatsQueryValidator()
    {
        // No specific validation required as this query has no parameters
        // The context validation will be handled in the handler
    }
}