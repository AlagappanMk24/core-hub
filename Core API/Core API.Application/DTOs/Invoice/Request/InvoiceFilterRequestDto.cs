namespace Core_API.Application.DTOs.Invoice.Request
{
    public class InvoiceFilterRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? InvoiceStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public int? CustomerId { get; set; }
        public int? TaxType { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? InvoiceNumberFrom { get; set; }
        public string? InvoiceNumberTo { get; set; }
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }

        // Optional: Add validation logic
        public bool IsValid()
        {
            if (PageNumber < 1 || PageSize < 1)
            {
                return false;
            }
            if (MinAmount.HasValue && MaxAmount.HasValue && MinAmount > MaxAmount)
            {
                return false;
            }
            return true;
        }
    }
}
