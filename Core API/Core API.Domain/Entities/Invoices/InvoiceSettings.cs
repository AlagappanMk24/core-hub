using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Core_API.Domain.Common;
using Core_API.Domain.Entities.Companies;

namespace Core_API.Domain.Entities.Invoices
{
    public class InvoiceSettings : BaseEntity
    {
        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        [Required]
        public bool IsAutomated { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Invoice prefix cannot exceed 10 characters.")]
        public string InvoicePrefix { get; set; } = "INV";  // User can set any prefix

        public string InvoiceNumberFormat { get; set; } = "{prefix}{number:D4}";

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Starting number must be at least 1.")]
        public int InvoiceStartingNumber { get; set; }
        public int LastUsedNumber { get; set; } = 0;
        public int LastUsedYear { get; set; } = 0; 
        public bool IncludeYear { get; set; } = false;
        public string Separator { get; set; } = "-";  // Separator between parts
        public int NumberPadding { get; set; } = 4;

        // ===== DISCOUNT SETTINGS =====
        [Required]
        public bool EnableItemLevelDiscounts { get; set; } = true;

        [Required]
        public bool EnableOverallDiscounts { get; set; } = false;

        [Required]
        [StringLength(20)]
        public string DefaultDiscountType { get; set; } = "Percentage";

        [Required]
        [Range(0, 100)]
        public int MaxDiscountPercentage { get; set; } = 25; // Standard: 100% is risky. Most businesses require manager approval above 20-30%.

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MaxDiscountAmount { get; set; } = 500; // Standard: A lower safety ceiling prevents catastrophic entry errors.

        [Required]
        public bool AllowMultipleDiscounts { get; set; } = false; // Standard: "Stacking" is usually disabled by default to protect margins.

        [Required]
        public bool ApplyDiscountBeforeTax { get; set; } = true; // Standard: In most jurisdictions (like US/UK), tax is calculated on the discounted subtotal.

        [Required]
        public bool ShowDiscountColumnOnInvoice { get; set; } = true; // Renamed for clarity: Transparency with the customer is standard.
    }
}