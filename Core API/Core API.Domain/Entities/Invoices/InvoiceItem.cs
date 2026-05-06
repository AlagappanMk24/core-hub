using Core_API.Domain.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Invoices
{
    /// <summary>
    /// Represents a single line item on an invoice, extending the shared
    /// <see cref="LineItemBase"/> with invoice-specific tax and gross totals.
    /// </summary>
    public class InvoiceItem : LineItemBase
    {
        /// <summary>
        /// Foreign key linking this line item to its parent invoice.
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the parent <see cref="Invoice"/>.
        /// </summary>
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }

        /// <summary>
        /// Tax amount calculated for this specific line item.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Gross amount for this line item inclusive of tax (UnitPrice × Quantity + TaxAmount).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
    }
}
