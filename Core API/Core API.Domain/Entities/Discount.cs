using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Entities
{
    public class Discount : BaseEntity
    {
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        [Required]
        [MaxLength(50)]
        public string Description { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; }
    }
}
