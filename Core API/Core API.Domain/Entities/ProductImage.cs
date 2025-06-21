using Core_API.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Core_API.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        [Required]
        public string ImageUrl { get; set; }
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [JsonIgnore]
        public Product Product { get; set; }
    }
}
