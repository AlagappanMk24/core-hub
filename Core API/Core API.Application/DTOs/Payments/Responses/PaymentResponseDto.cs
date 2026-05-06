namespace Core_API.Application.DTOs.Payments.Responses
{
    /// <summary>
    /// DTO for payment response
    /// </summary>
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentType { get; set; }
        public string TransactionId { get; set; }
        public string ReferenceNumber { get; set; }
        public string CustomerNotes { get; set; }
        public string InternalNotes { get; set; }
        public bool IsRefund { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
