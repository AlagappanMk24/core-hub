using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Core_API.Domain.Enums;

namespace Core_API.Domain.Entities
{
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; }
        public string PONumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime PaymentDue { get; set; }
        public InvoiceStatus InvoiceStatus { get; set; } = InvoiceStatus.Draft;
        public InvoiceType InvoiceType { get; set; } = InvoiceType.Standard;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string Currency { get; set; } = "INR";
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? PaymentMethod { get; set; }
        public string ProjectDetail { get; set; }
        public bool IsAutomated { get; set; }   
        public List<InvoiceItem> InvoiceItems { get; set; } = [];
        public List<TaxDetail> TaxDetails { get; set; } = [];
        public List<Discount> Discounts { get; set; } = [];
    }
    public class InvoiceItem : BaseEntity
    {
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string TaxType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }
    }
    public class TaxDetail : BaseEntity
    {
        /// <summary>
        /// Gets or sets the type of tax (e.g., VAT, GST).
        /// </summary>
        [Required(ErrorMessage = "Tax type is required.")]
        [StringLength(50, ErrorMessage = "Tax type cannot exceed {1} characters.")]
        public string TaxType { get; set; }

        /// <summary>
        /// Gets or sets the tax rate (e.g., 20.00 for 20%).
        /// </summary>
        [Required(ErrorMessage = "Tax rate is required.")]
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100%.")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the tax amount.
        /// </summary>
        [Required(ErrorMessage = "Tax amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent invoice.
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Gets or sets the parent invoice.
        /// </summary>
        [ForeignKey("InvoiceId")]
        public Invoice? Invoice { get; set; }
    }
}
