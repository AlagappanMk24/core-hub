using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; }
        public string PONumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime PaymentDue { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public InvoiceType InvoiceType { get; set; } = InvoiceType.Standard;
        public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public List<InvoiceAttachments> InvoiceAttachments { get; set; } = new List<InvoiceAttachments>();
        public string? Notes { get; set; }
        public string? PaymentTerms { get; set; }
        public string? PaymentMethod { get; set; }
        public List<TaxDetail> TaxDetails { get; set; } = new List<TaxDetail>();
        public int? RecurringInvoiceId { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public int LocationId { get; set; }
        public Location? Location { get; set; }
        public int? OrderId { get; set; }
        public OrderHeader? Order { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? ExternalReference { get; set; }
    }
    public class InvoiceItem : BaseEntity
    {
        public string? Service { get; set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
    }
    public class InvoiceAttachments : BaseEntity
    {
        public string? AttachmentName { get; set; }
        public string? AttachmentContent { get; set; }
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
    }
    /// <summary>
    /// Represents the status of an invoice.
    /// </summary>
    public enum InvoiceStatus
    {
        Draft, // 0
        Sent, // 1
        PartiallyPaid, // 2
        Paid, // 3
        Overdue, // 4
        Void // 5
    }

    /// <summary>
    /// Represents the type of invoice.
    /// </summary>
    public enum InvoiceType
    {
        Standard, // 0
        Recurring, // 1
        Proforma, // 2
        CreditNote // 3
    }
    /// <summary>
    /// Represents a tax component applied to an invoice.
    /// </summary>
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
    public class InvoiceUpsertVM
    {
        // Invoice Properties
        public int? Id { get; set; } // Nullable for new invoices

        [Required(ErrorMessage = "Invoice number is required")]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "Issue date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Issue Date")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(30);

        [Required(ErrorMessage = "Invoice type is required")]
        [Display(Name = "Invoice Type")]
        public string Type { get; set; } = "Standard"; // Default to Standard

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive value")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        // List of Customers for Selection
        public List<CustomerVM> Customers { get; set; } = new List<CustomerVM>();

        // List of Invoice Items
        public List<InvoiceItemVM> Items { get; set; } = new List<InvoiceItemVM>();
    }

    // View Model for Customer (used in customer selection)
    public class CustomerVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string Phone { get; set; }
    }

    // View Model for Invoice Items
    public class InvoiceItemVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a positive value")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }
    }
}
