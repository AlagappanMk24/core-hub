using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using MediatR;

namespace Core_API.Application.Features.Invoices.Queries.GetNextInvoiceNumber;

/// <summary>
/// Query to retrieve the next available invoice number
/// </summary>
public record GetNextInvoiceNumberQuery : BaseQuery<string>
{
    /// <summary>
    /// Creates a new instance of GetNextInvoiceNumberQuery
    /// </summary>
    public static GetNextInvoiceNumberQuery Create()
    {
        return new GetNextInvoiceNumberQuery();
    }
}