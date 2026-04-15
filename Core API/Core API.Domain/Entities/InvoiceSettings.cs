using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
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
    }
}