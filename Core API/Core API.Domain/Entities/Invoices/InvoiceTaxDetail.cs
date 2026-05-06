using Core_API.Domain.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Invoices
{
    /// <summary>
    /// Stores the tax breakdown for a specific tax applied to an invoice,
    /// extending <see cref="TaxDetailBase"/> with the invoice foreign key.
    /// </summary>
    public class InvoiceTaxDetail : TaxDetailBase
    {
        /// <summary>
        /// Foreign key linking this tax detail to its parent invoice.
        /// </summary>
        [Required]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Navigation property to the parent <see cref="Invoice"/>.
        /// </summary>
        [ForeignKey("InvoiceId")]
        public Invoice? Invoice { get; set; }
    }
}
