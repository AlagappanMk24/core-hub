using FluentValidation;

namespace Core_API.Application.Features.Invoices.Queries.GetNextInvoiceNumber;

/// <summary>
/// Validator for GetNextInvoiceNumberQuery
/// </summary>
public class GetNextInvoiceNumberQueryValidator : AbstractValidator<GetNextInvoiceNumberQuery>
{
    public GetNextInvoiceNumberQueryValidator()
    {
        // No specific validation required as this query has no parameters
        // The context validation will be handled in the handler
    }
}