using Core_API.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Payments.Requests
{
    /// <summary>
    /// DTO for updating a payment
    /// </summary>
    public class UpdatePaymentDto
    {
        public int Id { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Amount { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(200)]
        public string TransactionId { get; set; }

        [StringLength(100)]
        public string ReferenceNumber { get; set; }

        [StringLength(500)]
        public string CustomerNotes { get; set; }

        [StringLength(500)]
        public string InternalNotes { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }
    }
}
