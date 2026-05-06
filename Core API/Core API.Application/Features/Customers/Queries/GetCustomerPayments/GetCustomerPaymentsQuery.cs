using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerPayments
{
    /// <summary>
 /// Query to get all payments for a specific customer
 /// </summary>
    public record GetCustomerPaymentsQuery : BaseQuery<List<CustomerPaymentDto>>
    {
        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        public int CustomerId { get; init; }
    }
}