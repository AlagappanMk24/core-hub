using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Invoice.Request
{
    public class InvoiceCreateDto
    {
        [Required(ErrorMessage = "Invoice number is required")]
        [StringLength(50, ErrorMessage = "Invoice number cannot exceed 50 characters")]
        public string InvoiceNumber { get; set; }

        [StringLength(50, ErrorMessage = "PO number cannot exceed 50 characters")]
        public string PONumber { get; set; }

        [Required(ErrorMessage = "Issue date is required")]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Due date is required")]
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Invoice type is required")]
        public string Type { get; set; } = "Standard";
        public string? InvoiceStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsAutomated { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }

        [StringLength(100, ErrorMessage = "Payment method cannot exceed 100 characters")]
        public string? PaymentMethod { get; set; }
        public string? ProjectDetail { get; set; }

        [Required(ErrorMessage = "At least one invoice item is required")]
        [MinLength(1, ErrorMessage = "At least one invoice item is required")]
        public List<InvoiceItemDto> Items { get; set; } = [];
        public List<TaxDetailDto> TaxDetails { get; set; } = [];
        public List<DiscountDto> Discounts { get; set; } = [];
        public List<InvoiceAttachmentDto> Attachments { get; set; } = [];
    }
    public class InvoiceItemDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a positive value")]
        public decimal UnitPrice { get; set; }

        [StringLength(50, ErrorMessage = "Tax type cannot exceed 50 characters")]
        public string? TaxType { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public decimal Amount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative")]
        public decimal TaxAmount { get; set; }
    }
    public class TaxDetailDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tax type is required")]
        [StringLength(50, ErrorMessage = "Tax type cannot exceed 50 characters")]
        public string? TaxType { get; set; }

        [Required(ErrorMessage = "Tax rate is required")]
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100%")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Tax amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative")]
        public decimal Amount { get; set; }
    }
    public class DiscountDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Description { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; }
    }
    public class TaxTypeDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Rate { get; set; }
    }
    public class TaxTypeCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [Range(0, 100)]
        public decimal Rate { get; set; }
    }
    public class InvoiceAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public IFormFile? FileContent { get; set; } // For new attachments
        public string? FileUrl { get; set; } // For existing attachments
    }
}