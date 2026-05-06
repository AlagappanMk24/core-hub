using Core_API.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities.Customers
{
    public class CustomerCommunication : BaseEntity
    {
        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // email, sms, call, meeting

        [Required]
        [StringLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Direction { get; set; } = "outbound"; // inbound, outbound

        [Required]
        public DateTime SentAt { get; set; }

        [Required]
        public string SentBy { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "sent"; // sent, delivered, failed, read

        public string? ErrorMessage { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}
