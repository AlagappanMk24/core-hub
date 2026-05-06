namespace Core_API.Application.DTOs.Customer.Response
{
    /// <summary>
    /// DTO for customer invoice list
    /// </summary>
    public class CustomerInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
    }
}