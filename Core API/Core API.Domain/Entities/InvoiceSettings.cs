using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class InvoiceSettings : BaseEntity
    {
        [Required]
        public bool IsAutomated { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Invoice prefix cannot exceed 10 characters.")]
        public string InvoicePrefix { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Starting number must be at least 1.")]
        public int InvoiceStartingNumber { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}
