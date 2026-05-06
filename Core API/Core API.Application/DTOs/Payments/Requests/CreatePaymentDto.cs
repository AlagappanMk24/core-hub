using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Payments.Requests
{
    /// <summary>
    /// DTO for creating a new payment
    /// </summary>
    public class CreatePaymentDto
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(200)]
        public string TransactionId { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(50)]
        public string CheckNumber { get; set; }

        [StringLength(100)]
        public string BankName { get; set; }

        [StringLength(500)]
        public string CustomerNotes { get; set; }

        [StringLength(500)]
        public string InternalNotes { get; set; }

        public PaymentType PaymentType { get; set; } = PaymentType.Full;
    }
}