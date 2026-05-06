namespace Core_API.Application.DTOs.Dashboard.Responses
{
    public class MonthlyInvoiceTrendDto
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
}