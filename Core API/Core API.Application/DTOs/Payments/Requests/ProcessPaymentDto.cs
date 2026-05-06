using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Payments.Requests
{

    /// <summary>
    /// DTO for processing a payment
    /// </summary>
    public class ProcessPaymentDto
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        public string PaymentGateway { get; set; } // Stripe, PayPal, etc.

        public string PaymentToken { get; set; } // Token from frontend payment gateway
    }
}
