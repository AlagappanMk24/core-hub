using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerPayments
{
    /// <summary>
    /// Handler for getting customer payments
    /// </summary>
    public sealed class GetCustomerPaymentsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCustomerPaymentsQueryHandler> logger) : IRequestHandler<GetCustomerPaymentsQuery, OperationResult<List<CustomerPaymentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetCustomerPaymentsQueryHandler> _logger = logger;

        public async Task<OperationResult<List<CustomerPaymentDto>>> Handle(
            GetCustomerPaymentsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {

                Customer? customer = null;

                // ✅ Super Admin: Can access ANY customer's payments
                if (request.Context.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin retrieving payments for customer {CustomerId}", request.CustomerId);
                    customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                }
                // ✅ Admin/User: Can only access payments from customers in their company
                else if (request.Context.CompanyId.HasValue)
                {
                    _logger.LogInformation("Admin/User retrieving payments for customer {CustomerId} in company {CompanyId}",
                        request.CustomerId, request.Context.CompanyId.Value);

                    customer = await _unitOfWork.Customers.GetAsync(
                        c => c.Id == request.CustomerId && c.CompanyId == request.Context.CompanyId.Value && !c.IsDeleted);
                }
                // ✅ Customer: Can only access their own payments
                else if (request.Context.CustomerId.HasValue && request.Context.CustomerId.Value == request.CustomerId)
                {
                    _logger.LogInformation("Customer retrieving their own payments for customer {CustomerId}", request.CustomerId);
                    customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                }
                else
                {
                    return OperationResult<List<CustomerPaymentDto>>.FailureResult("Access denied.");
                }

                if (customer == null)
                {
                    return OperationResult<List<CustomerPaymentDto>>.FailureResult("Customer not found or access denied.");
                }

                var payments = await _unitOfWork.Customers.GetCustomerPaymentsAsync(request.CustomerId);

                var paymentDtos = _mapper.Map<List<CustomerPaymentDto>>(payments);

                return OperationResult<List<CustomerPaymentDto>>.SuccessResult(paymentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer payments for customer {CustomerId}", request.CustomerId);
                return OperationResult<List<CustomerPaymentDto>>.FailureResult("Failed to retrieve customer payments");
            }
        }
    }
}
