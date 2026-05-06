using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Commands.DeleteCustomer
{
    /// <summary>
    /// Handler for deleting a customer
    /// </summary>
    public class DeleteCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteCustomerCommandHandler> logger) : IRequestHandler<DeleteCustomerCommand, OperationResult<bool>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<DeleteCustomerCommandHandler> _logger = logger;

        public async Task<OperationResult<bool>> Handle(
            DeleteCustomerCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!request.Context.CompanyId.HasValue)
                    return OperationResult<bool>.FailureResult("Company ID is required.");

                var customer = await _unitOfWork.Customers.GetAsync(
                    c => c.Id == request.Id && c.CompanyId == request.Context.CompanyId && !c.IsDeleted);

                if (customer == null)
                {
                    return OperationResult<bool>.FailureResult("Customer not found.");
                }

                // Check if customer has invoices
                var hasInvoices = await _unitOfWork.Customers.HasInvoicesAsync(request.Id);
                if (hasInvoices)
                {
                    return OperationResult<bool>.FailureResult("Cannot delete customer with associated invoices.");
                }

                customer.IsDeleted = true;
                customer.UpdatedBy = request.Context.UserId;
                customer.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", request.Id);
                return OperationResult<bool>.FailureResult("Failed to delete customer.");
            }
        }
    }
}
