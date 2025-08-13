using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoice.Request;

namespace Core_API.Application.DTOs.Invoice.Response
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string PONumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public string InvoiceStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string Type { get; set; }
        public int CustomerId { get; set; }
        public string Currency { get; set; }
        public CustomerResponseDto Customer { get; set; }
        public int CompanyId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public string ProjectDetail { get; set; }
        public bool IsAutomated { get; set; }
        public string Notes { get; set; }
        public string PaymentMethod { get; set; }
        public List<InvoiceItemDto> Items { get; set; }
        public List<TaxDetailDto> TaxDetails { get; set; }
        public List<DiscountDto> Discounts { get; set; }
        public MemoryStream PdfStream { get; set; }
    }
}
