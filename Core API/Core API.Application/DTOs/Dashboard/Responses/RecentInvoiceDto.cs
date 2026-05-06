namespace Core_API.Application.DTOs.Dashboard.Responses
{
    public class RecentInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAvatar { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}