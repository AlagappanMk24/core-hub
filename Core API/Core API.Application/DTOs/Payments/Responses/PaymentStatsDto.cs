using Core_API.Application.DTOs.Payments.Requests;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Payments.Responses
{
    /// <summary>
    /// DTO for payment summary/stats
    /// </summary>
    public class PaymentStatsDto
    {
        public decimal TotalPayments { get; set; }
        public int TotalPaymentCount { get; set; }
        public decimal TotalRefunds { get; set; }
        public int TotalRefundCount { get; set; }
        public decimal NetCollected { get; set; }

        // Payment method breakdown
        public Dictionary<string, decimal> PaymentMethodBreakdown { get; set; }

        // Monthly trend
        public List<MonthlyPaymentTrendDto> MonthlyTrend { get; set; }

        // Status breakdown
        public Dictionary<string, int> StatusBreakdown { get; set; }

        /// <summary>
        /// DTO for monthly payment trend
        /// </summary>
        public class MonthlyPaymentTrendDto
        {
            public string Month { get; set; }
            public int Year { get; set; }
            public decimal Amount { get; set; }
            public int Count { get; set; }
        }
    }
}
