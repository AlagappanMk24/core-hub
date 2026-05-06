namespace Core_API.Application.DTOs.RecurringInvoice.Request
{
    /// <summary>
    /// Filter DTO for paged list of recurring invoices
    /// </summary>
    public class RecurringInvoiceFilterDto
    {
        /// <summary>Page number (1-based)</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Page size (max 100)</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>Search term for name or description</summary>
        public string? Search { get; set; }

        /// <summary>Filter by status (Draft, Active, Paused, etc.)</summary>
        public string? Status { get; set; }

        /// <summary>Filter by frequency</summary>
        public string? Frequency { get; set; }

        /// <summary>Filter by customer ID</summary>
        public int? CustomerId { get; set; }

        /// <summary>Filter by next invoice date from</summary>
        public DateTime? NextDateFrom { get; set; }

        /// <summary>Filter by next invoice date to</summary>
        public DateTime? NextDateTo { get; set; }

        /// <summary>Filter by start date from</summary>
        public DateTime? StartDateFrom { get; set; }

        /// <summary>Filter by start date to</summary>
        public DateTime? StartDateTo { get; set; }

        /// <summary>Filter by end date from</summary>
        public DateTime? EndDateFrom { get; set; }

        /// <summary>Filter by end date to</summary>
        public DateTime? EndDateTo { get; set; }

        /// <summary>Filter by minimum amount</summary>
        public decimal? MinAmount { get; set; }

        /// <summary>Filter by maximum amount</summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>Filter by auto-send setting</summary>
        public bool? AutoSend { get; set; }

        /// <summary>Sort field (Name, NextInvoiceDate, TotalAmount, Status, etc.)</summary>
        public string SortBy { get; set; } = "NextInvoiceDate";

        /// <summary>Sort order (asc or desc)</summary>
        public string SortOrder { get; set; } = "asc";

        /// <summary>Validate filter parameters</summary>
        public bool IsValid()
        {
            return PageNumber > 0 &&
                   PageSize > 0 &&
                   PageSize <= 100 &&
                   (!MinAmount.HasValue || !MaxAmount.HasValue || MinAmount <= MaxAmount);
        }
    }
}
