namespace Core_API.Application.DTOs.Customer.Response
{
    /// <summary>
    /// DTO for customer spending trend
    /// </summary>
    public class SpendingTrendDto
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
    }
}