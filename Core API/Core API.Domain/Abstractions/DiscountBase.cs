using Core_API.Domain.Common;
using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Abstractions
{
    public abstract class DiscountBase : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        [Required]
        public DiscountType DiscountType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }
}