namespace Core_API.Application.DTOs.Invoice.Response
{
    public class InvoiceStatsDto
    {
        public StatsItem All { get; set; }
        public StatsItem Draft { get; set; }
        public StatsItem Sent { get; set; }
        public StatsItem Approved { get; set; }
        public StatsItem Cancelled { get; set; }
        public StatsItem Pending { get; set; }
        public StatsItem Processing { get; set; }
        public StatsItem Completed { get; set; }
        public StatsItem PartiallyPaid { get; set; }
        public StatsItem Overdue { get; set; }
        public StatsItem Refunded { get; set; }
    }

    public class StatsItem
    {
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Change { get; set; }
    }
}
