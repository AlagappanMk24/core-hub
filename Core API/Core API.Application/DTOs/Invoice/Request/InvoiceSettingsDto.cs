using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Invoice.Request
{
    public class InvoiceSettingsDto
    {
        [Required]
        public int CompanyId { get; set; }

        [Required]
        public bool IsAutomated { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Invoice prefix cannot exceed 10 characters.")]
        public string InvoicePrefix { get; set; } = "INV";

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Starting number must be at least 1.")]
        public int InvoiceStartingNumber { get; set; }
        public string InvoiceNumberFormat { get; set; } = "{prefix}{number:D4}";
        public int LastUsedNumber { get; set; }
        public int LastUsedYear { get; set; }
        public bool IncludeYear { get; set; }

        [StringLength(5)]
        public string Separator { get; set; } = "-";

        [Range(1, 10)]
        public int NumberPadding { get; set; } = 4;
    }
}
