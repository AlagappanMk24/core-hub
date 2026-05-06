using Core_API.Application.Common.Base;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Application.Features.Customers.Queries.GetCustomers
{
    /// <summary>
    /// Query to get paged list of customers
    /// </summary>
    public record GetCustomersQuery : BaseQuery<PaginatedResult<CustomerResponseDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? Search { get; init; }
        public string? Status { get; init; }

        public CustomerFilterRequestDto ToFilterDto()
        {
            return new CustomerFilterRequestDto
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                Search = Search,
                Status = Status
            };
        }
    }
}
