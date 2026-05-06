namespace Core_API.Application.DTOs.RecurringInvoice.Response
{
    /// <summary>
    /// Statistics DTO for recurring invoices dashboard
    /// </summary>
    public class RecurringInvoiceStatsDto
    {
        // Counts by status
        public int TotalActive { get; set; }
        public int TotalDraft { get; set; }
        public int TotalPaused { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalCancelled { get; set; }
        public int TotalExpired { get; set; }

        // Financial metrics
        public decimal TotalMonthlyValue { get; set; }
        public decimal TotalQuarterlyValue { get; set; }
        public decimal TotalAnnualValue { get; set; }

        // Upcoming generations
        public int DueThisWeek { get; set; }
        public int DueThisMonth { get; set; }
        public int DueNextMonth { get; set; }

        // Breakdown by frequency
        public List<RecurringInvoiceByFrequencyDto> ByFrequency { get; set; } = new();

        // Top customers by value
        public List<RecurringInvoiceByCustomerDto> TopCustomersByValue { get; set; } = new();
    }

    public class RecurringInvoiceByFrequencyDto
    {
        public string Frequency { get; set; }
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class RecurringInvoiceByCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageValue { get; set; }
    }
}
