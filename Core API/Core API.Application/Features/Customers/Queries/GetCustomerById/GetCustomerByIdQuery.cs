using Core_API.Application.Common.Base;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerById
{
    /// <summary>
    /// Query to get a customer by ID
    /// </summary>
    public record GetCustomerByIdQuery : BaseQuery<CustomerResponseDto>
    {
        public int Id { get; init; }
    }
}