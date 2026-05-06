using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerActivities
{
    /// <summary>
    /// Handler for getting customer activities
    /// </summary>
    public sealed class GetCustomerActivitiesQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCustomerActivitiesQueryHandler> logger) : IRequestHandler<GetCustomerActivitiesQuery, OperationResult<List<CustomerActivityDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<GetCustomerActivitiesQueryHandler> _logger = logger;

        public async Task<OperationResult<List<CustomerActivityDto>>> Handle(
            GetCustomerActivitiesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                Customer? customer = null;

                // ✅ Super Admin: Can access ANY customer's activities
                if (request.Context.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin retrieving activities for customer {CustomerId}", request.CustomerId);
                    customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                }
                // ✅ Admin/User: Can only access activities for customers in their company
                else if (request.Context.CompanyId.HasValue)
                {
                    _logger.LogInformation("Admin/User retrieving activities for customer {CustomerId} in company {CompanyId}",
                        request.CustomerId, request.Context.CompanyId.Value);

                    customer = await _unitOfWork.Customers.GetAsync(
                        c => c.Id == request.CustomerId && c.CompanyId == request.Context.CompanyId.Value && !c.IsDeleted);
                }
                // ✅ Customer: Can only access their own activities
                else if (request.Context.CustomerId.HasValue && request.Context.CustomerId.Value == request.CustomerId)
                {
                    _logger.LogInformation("Customer retrieving their own activities for customer {CustomerId}", request.CustomerId);
                    customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                }
                else
                {
                    return OperationResult<List<CustomerActivityDto>>.FailureResult("Access denied.");
                }

                if (customer == null)
                {
                    return OperationResult<List<CustomerActivityDto>>.FailureResult("Customer not found or access denied.");
                }

                var activities = await _unitOfWork.Customers.GetCustomerActivitiesAsync(request.CustomerId, request.Count);

                return OperationResult<List<CustomerActivityDto>>.SuccessResult(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer activities for customer {CustomerId}", request.CustomerId);
                return OperationResult<List<CustomerActivityDto>>.FailureResult("Failed to retrieve customer activities");
            }
        }
    }
}
