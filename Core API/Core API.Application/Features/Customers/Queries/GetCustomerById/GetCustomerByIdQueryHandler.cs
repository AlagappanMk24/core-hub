using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerById
{
    /// <summary>
    /// Handler for getting a customer by ID
    /// </summary>
    public class GetCustomerByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCustomerByIdQueryHandler> logger) : IRequestHandler<GetCustomerByIdQuery, OperationResult<CustomerResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetCustomerByIdQueryHandler> _logger = logger;

        public async Task<OperationResult<CustomerResponseDto>> Handle(
            GetCustomerByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                Customer? customer = null;

                // ✅ Super Admin: Can access ANY customer regardless of company
                if (request.Context.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin retrieving customer {CustomerId}", request.Id);
                    customer = await _unitOfWork.Customers.GetAsync(
                        c => c.Id == request.Id && !c.IsDeleted,
                        "Company,CustomerGroup");
                }
                // ✅ Admin/User: Can only access customers from their own company
                else if (request.Context.CompanyId.HasValue)
                {
                    _logger.LogInformation("Admin/User retrieving customer {CustomerId} for company {CompanyId}",
                        request.Id, request.Context.CompanyId.Value);

                    customer = await _unitOfWork.Customers.GetAsync(
                        c => c.Id == request.Id && c.CompanyId == request.Context.CompanyId.Value && !c.IsDeleted,
                        "Company,CustomerGroup");
                }
                // ✅ Customer themselves: Can only access their own customer record
                else if (request.Context.CustomerId.HasValue)
                {
                    _logger.LogInformation("Customer retrieving their own record {CustomerId}", request.Context.CustomerId.Value);

                    // Customer can only access their own record, ignore the request.Id
                    customer = await _unitOfWork.Customers.GetAsync(
                        c => c.Id == request.Context.CustomerId.Value && !c.IsDeleted,
                        "Company,CustomerGroup");

                    // If customer exists and they're trying to access a different ID, deny access
                    if (customer != null && customer.Id != request.Id)
                    {
                        return OperationResult<CustomerResponseDto>.FailureResult("Access denied. You can only view your own customer record.");
                    }
                }
                else
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("Authentication required to access customer data.");
                }

                if (customer == null)
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("Customer not found.");
                }

                var response = _mapper.Map<CustomerResponseDto>(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer {CustomerId}", request.Id);
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to retrieve customer.");
            }
        }
    }
}
