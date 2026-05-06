using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerStats
{
    /// <summary>
    /// Query to get customer statistics
    /// </summary>
    public record GetCustomerStatsQuery : BaseQuery<CustomerStatsDto>
    {
    }
}
