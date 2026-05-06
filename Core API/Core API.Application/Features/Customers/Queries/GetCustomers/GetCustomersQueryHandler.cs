using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Queries.GetCustomers
{

    /// <summary>
    /// Handler for getting paged list of customers
    /// </summary>
    public class GetCustomersQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCustomersQueryHandler> logger) : IRequestHandler<GetCustomersQuery, OperationResult<PaginatedResult<CustomerResponseDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetCustomersQueryHandler> _logger = logger;

        public async Task<OperationResult<PaginatedResult<CustomerResponseDto>>> Handle(
            GetCustomersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!request.Context.CompanyId.HasValue)
                {
                    return OperationResult<PaginatedResult<CustomerResponseDto>>.FailureResult("Company ID is required.");
                }

                var filter = request.ToFilterDto();

                if (!filter.IsValid())
                {
                    return OperationResult<PaginatedResult<CustomerResponseDto>>.FailureResult("Invalid filter parameters.");
                }

                PaginatedResult<Customer> result;

                // ✅ Super Admin: Can see ALL customers across ALL companies
                if (request.Context.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin retrieving all customers across all companies");
                    result = await _unitOfWork.Customers.GetAllCustomersPagedAsync(filter);
                }
                // ✅ Admin/User: Can see only customers from their company
                else if (request.Context.CompanyId.HasValue)
                {
                    _logger.LogInformation("Admin/User retrieving customers for company {CompanyId}", request.Context.CompanyId.Value);
                    result = await _unitOfWork.Customers.GetPagedAsync(request.Context.CompanyId.Value, filter);
                }
                else
                {
                    return OperationResult<PaginatedResult<CustomerResponseDto>>.FailureResult("Company ID is required for non-admin users.");
                }

                var mappedItems = _mapper.Map<List<CustomerResponseDto>>(result.Items);

                var response = new PaginatedResult<CustomerResponseDto>
                {
                    Items = mappedItems,
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                };

                return OperationResult<PaginatedResult<CustomerResponseDto>>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged customers");
                return OperationResult<PaginatedResult<CustomerResponseDto>>.FailureResult("Failed to retrieve customers.");
            }
        }
    }
}