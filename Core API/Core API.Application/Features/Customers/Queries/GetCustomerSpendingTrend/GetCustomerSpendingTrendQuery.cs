using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerSpendingTrend
{
    /// <summary>
    /// Query to get spending trend for a specific customer
    /// </summary>
    public record GetCustomerSpendingTrendQuery : BaseQuery<List<SpendingTrendDto>>
    {
        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        public int CustomerId { get; init; }
    }
}