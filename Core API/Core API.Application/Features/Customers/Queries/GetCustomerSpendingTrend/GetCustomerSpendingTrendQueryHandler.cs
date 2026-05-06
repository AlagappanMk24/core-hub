using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerSpendingTrend
{
    /// <summary>
    /// Handler for getting customer spending trend
    /// </summary>
    public sealed class GetCustomerSpendingTrendQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCustomerSpendingTrendQueryHandler> logger) : IRequestHandler<GetCustomerSpendingTrendQuery, OperationResult<List<SpendingTrendDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetCustomerSpendingTrendQueryHandler> _logger = logger;

        public async Task<OperationResult<List<SpendingTrendDto>>> Handle(
            GetCustomerSpendingTrendQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                Customer? customer = null;
                // ✅ Super Admin: Can access ANY customer's spending trend
                if (request.Context.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin retrieving spending trend for customer {CustomerId}", request.CustomerId);
                    customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                }
                // ✅ Admin/User: Can only access spending trend from customers in their company
                else if (request.Context.CompanyId.HasValue)
                {
                    _logger.LogInformation("Admin/User retrieving spending trend for customer {CustomerId} in company {CompanyId}",
                        request.CustomerId, request.Context.CompanyId.Value);

                    customer = await _unitOfWork.Customers.GetAsync(
                        c => c.Id == request.CustomerId && c.CompanyId == request.Context.CompanyId.Value && !c.IsDeleted);
                }
                // ✅ Customer: Can only access their own spending trend
                else if (request.Context.CustomerId.HasValue && request.Context.CustomerId.Value == request.CustomerId)
                {
                    _logger.LogInformation("Customer retrieving their own spending trend for customer {CustomerId}", request.CustomerId);
                    customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                }
                else
                {
                    return OperationResult<List<SpendingTrendDto>>.FailureResult("Access denied.");
                }

                if (customer == null)
                {
                    return OperationResult<List<SpendingTrendDto>>.FailureResult("Customer not found or access denied.");
                }

                var trend = await _unitOfWork.Customers.GetCustomerSpendingTrendAsync(request.CustomerId);

                return OperationResult<List<SpendingTrendDto>>.SuccessResult(trend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer spending trend for customer {CustomerId}", request.CustomerId);
                return OperationResult<List<SpendingTrendDto>>.FailureResult("Failed to retrieve customer spending trend");
            }
        }
    }
}
