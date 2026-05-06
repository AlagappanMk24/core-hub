using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerActivities
{
    /// <summary>
    /// Query to get recent activities for a specific customer
    /// </summary>
    public record GetCustomerActivitiesQuery : BaseQuery<List<CustomerActivityDto>>
    {
        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        public int CustomerId { get; init; }

        /// <summary>
        /// Gets or sets the number of activities to retrieve (default: 10)
        /// </summary>
        public int Count { get; init; } = 10;
    }
}