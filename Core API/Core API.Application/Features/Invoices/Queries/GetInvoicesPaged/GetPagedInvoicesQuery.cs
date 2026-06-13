using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Response;

namespace Core_API.Application.Features.Invoices.Queries.GetInvoicesPaged;

/// <summary>
/// Query to retrieve a paged list of invoices with filtering options.
/// </summary>
public record GetPagedInvoicesQuery : BaseQuery<PaginatedResult<InvoiceResponseDto>>
{
    /// <summary>
    /// Page number (1-indexed)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Search term for invoice number or customer name
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by invoice status (Draft, Sent, Paid, Void, etc.)
    /// </summary>
    public string? InvoiceStatus { get; set; }

    /// <summary>
    /// Filter by payment status (Pending, Paid, PartiallyPaid, Overdue)
    /// </summary>
    public string? PaymentStatus { get; set; }

    /// <summary>
    /// Filter by specific customer ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Filter by tax type ID
    /// </summary>
    public int? TaxType { get; set; }

    /// <summary>
    /// Minimum invoice amount filter
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Maximum invoice amount filter
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Starting invoice number for range filter
    /// </summary>
    public string? InvoiceNumberFrom { get; set; }

    /// <summary>
    /// Ending invoice number for range filter
    /// </summary>
    public string? InvoiceNumberTo { get; set; }

    /// <summary>
    /// Filter invoices issued on or after this date
    /// </summary>
    public DateTime? IssueDateFrom { get; set; }

    /// <summary>
    /// Filter invoices issued on or before this date
    /// </summary>
    public DateTime? IssueDateTo { get; set; }

    /// <summary>
    /// Filter invoices due on or after this date
    /// </summary>
    public DateTime? DueDateFrom { get; set; }

    /// <summary>
    /// Filter invoices due on or before this date
    /// </summary>
    public DateTime? DueDateTo { get; set; }

    /// <summary>
    /// Validates the filter parameters
    /// </summary>
    public bool IsValid()
    {
        if (PageNumber < 1 || PageSize < 1)
            return false;

        if (MinAmount.HasValue && MaxAmount.HasValue && MinAmount > MaxAmount)
            return false;

        return true;
    }
}