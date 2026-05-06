namespace Core_API.Application.DTOs.Customer.Response
{
    /// <summary>
    /// DTO for customer payment list
    /// </summary>
    public class CustomerPaymentDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public bool IsOnTime { get; set; }
    }
}