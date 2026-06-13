using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.DTOs.Invoices.Requests;

namespace Core_API.Application.DTOs.Invoice.Response
{
    public class InvoiceResponseDto
    {
        // Identity
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string PONumber { get; set; }

        // Dates
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? PaidDate { get; set; }

        // Status
        public string InvoiceStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string Type { get; set; }

        // Parties
        public int CustomerId { get; set; }
        public CustomerResponseDto Customer { get; set; }
        public int CompanyId { get; set; }

        // ✅ For Super Admin - to know which company the invoice belongs to
        public string? CompanyName { get; set; }

        // Financial
        public string Currency { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public string AdjustmentDescription { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountRefunded { get; set; }

        // Payment
        public string PaymentMethod { get; set; }
        public string PaymentGateway { get; set; }
        public string PaymentTerms { get; set; }

        // Notes
        public string CustomerNotes { get; set; }
        public string InternalNotes { get; set; }
        public string TermsAndConditions { get; set; }
        public string FooterNote { get; set; }
        public string ProjectDetail { get; set; }

        // Automation
        public bool IsAutomated { get; set; }
        public bool IsRecurring { get; set; }
        public int? RecurringInvoiceId { get; set; }
        public string SourceSystem { get; set; }
        public string Notes { get; set; }

        // Collections
        public List<InvoiceItemDto> Items { get; set; } = new();
        public List<InvoiceTaxDetailDto> TaxDetails { get; set; } = new();
        public List<InvoiceDiscountDto> Discounts { get; set; } = new();
        public List<InvoiceAttachmentDto> InvoiceAttachments { get; set; } = new();
        public List<InvoicePaymentDto> Payments { get; set; } = new();
        public List<InvoiceAuditLogDto> AuditLogs { get; set; } = new();

        // Audit
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        // Special
        public MemoryStream PdfStream { get; set; }
    }
    public class InvoicePaymentDto
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentReference { get; set; }
        public string Notes { get; set; }
        public bool IsRefund { get; set; }
    }
    public class InvoiceAuditLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string Changes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}