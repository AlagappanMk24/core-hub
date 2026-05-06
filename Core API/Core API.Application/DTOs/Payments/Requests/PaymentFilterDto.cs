namespace Core_API.Application.DTOs.Payments.Requests
{
    /// <summary>
    /// DTO for payment filter
    /// </summary>
    public class PaymentFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Search { get; set; }
        public int? InvoiceId { get; set; }
        public int? CustomerId { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public bool IsValid()
        {
            return PageNumber > 0 && PageSize > 0 && PageSize <= 100;
        }
    }
}