namespace Core_API.Application.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public DashboardStatsDto Stats { get; set; }
        public List<RecentInvoiceDto> RecentInvoices { get; set; }
        // Industry standard sections
        public List<InvoiceProgressDto> PaymentProgress { get; set; }    // Partially paid
        public List<InvoiceProgressDto> PendingPayments { get; set; }    // Unpaid but not overdue
        public List<InvoiceProgressDto> OverduePayments { get; set; }    // Overdue - needs action
        public List<RecentInvoiceDto> RecentPayments { get; set; }       // Recently completed
        public List<MonthlyInvoiceTrendDto> MonthlyTrend { get; set; }
        public List<MonthlyInvoiceTrendDto> B2BTrend { get; set; }
        public List<MonthlyInvoiceTrendDto> B2CTrend { get; set; }
        public List<MonthlyInvoiceTrendDto> RetailTrend { get; set; }
    }
}