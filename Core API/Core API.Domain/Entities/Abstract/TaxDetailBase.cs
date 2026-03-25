using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Abstract
{
    /// <summary>
    /// Shared tax breakdown fields reused by <c>InvoiceTaxDetail</c> and equivalent
    /// entities in other modules. Captures the taxable base, rate, computed amount,
    /// and jurisdiction for a single applied tax rule.
    /// </summary>
    public abstract class TaxDetailBase : BaseEntity
    {
        // ── Tax Identity ──────────────────────────────────────────────────────

        /// <summary>
        /// Display name of the tax rule (e.g., "GST", "CGST", "State Sales Tax").
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TaxName { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Tax rate as a percentage (0–100). Stored with two decimal places.
        /// </summary>
        [Required]
        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Rate { get; set; }
    }
}