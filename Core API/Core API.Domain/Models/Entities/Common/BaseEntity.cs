using System.ComponentModel.DataAnnotations;

namespace Core_API.Domain.Models.Entities.Common
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool Deleted { get; set; } = false; // Default to not deleted
        public string? CreatedBy { get; set; } // Nullable string
        public string? UpdatedBy { get; set; } // Nullable string
    }
}
