namespace Core_API.Application.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public decimal TotalInvoiceAmount { get; set; }
        public int TotalInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public int DraftInvoices { get; set; }
        public decimal PercentageChangeTotal { get; set; }
        public decimal PercentageChangePending { get; set; }
        public decimal PercentageChangePaid { get; set; }
    }
}