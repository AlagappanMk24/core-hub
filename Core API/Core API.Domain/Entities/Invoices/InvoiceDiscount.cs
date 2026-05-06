using Core_API.Domain.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Invoices
{
    /// <summary>
    /// Represents a discount applied at the invoice level,
    /// extending <see cref="DiscountBase"/> with the invoice foreign key.
    /// </summary>
    public class InvoiceDiscount : DiscountBase
    {
        /// <summary>
        /// Foreign key linking this discount to its parent invoice.
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the parent <see cref="Invoice"/>.
        /// </summary>
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }
    }
}