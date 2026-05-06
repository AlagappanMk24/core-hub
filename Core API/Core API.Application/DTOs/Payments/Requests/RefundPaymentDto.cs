using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Payments.Requests
{
    /// <summary>
    /// DTO for refund request
    /// </summary>
    public class RefundPaymentDto
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }
    }
}
