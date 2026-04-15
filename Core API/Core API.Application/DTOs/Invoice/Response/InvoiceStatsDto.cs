namespace Core_API.Application.DTOs.Invoice.Response
{
    public class InvoiceStatsDto
    {
        public StatsItem All { get; set; }
        public StatsItem Draft { get; set; }
        public StatsItem Sent { get; set; }
        public StatsItem Viewed { get; set; }
        public StatsItem PartiallyPaid { get; set; }
        public StatsItem Paid { get; set; }
        public StatsItem Overdue { get; set; }
        public StatsItem Void { get; set; }
        public StatsItem Cancelled { get; set; }
        public StatsItem Refunded { get; set; }
        public StatsItem Pending { get; set; }
    }
    public class StatsItem
    {
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Change { get; set; }
    }
    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int InvoiceCount { get; set; }
    }
    public class TopCustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public int InvoiceCount { get; set; }
    }
}