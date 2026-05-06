namespace Core_API.Application.DTOs.Dashboard.Responses
{
    public class InvoiceProgressDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal PaidPercentage { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string ProgressColor { get; set; }

        // Computed properties - no setters, calculated at runtime
        public bool IsOverdue => DateTime.UtcNow.Date > DueDate.Date;
        public int DaysUntilDue => (DueDate.Date - DateTime.UtcNow.Date).Days;

        // Additional industry standard fields
        public bool IsUrgent => IsOverdue || DaysUntilDue <= 3;
        public string Priority => IsOverdue ? "Critical" : DaysUntilDue <= 7 ? "High" : "Normal";
    }
}