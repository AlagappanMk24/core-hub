using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Queries.GetCustomerInvoices
{
    /// <summary>
    /// Handler for getting customer invoices
    /// </summary>
    public sealed class GetCustomerInvoicesQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCustomerInvoicesQueryHandler> logger) : IRequestHandler<GetCustomerInvoicesQuery, OperationResult<List<CustomerInvoiceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetCustomerInvoicesQueryHandler> _logger = logger;

        public async Task<OperationResult<List<CustomerInvoiceDto>>> Handle(
            GetCustomerInvoicesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                Customer? customer = null;

                // ✅ Super Admin: Can access ANY customer's invoices
                if (request.Context.IsSuperAdmin)
                {
                    _logger.LogInformation("Super Admin retrieving invoices for customer {CustomerId}", request.CustomerId);
                    customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                }
                // ✅ Admin/User: Can only access invoices from customers in their company
                else if (request.Context.CompanyId.HasValue)
                {
                    _logger.LogInformation("Admin/User retrieving invoices for customer {CustomerId} in company {CompanyId}",
                        request.CustomerId, request.Context.CompanyId.Value);

                    customer = await _unitOfWork.Customers.GetAsync(
                        c => c.Id == request.CustomerId && c.CompanyId == request.Context.CompanyId.Value && !c.IsDeleted);
                }
                // ✅ Customer: Can only access their own invoices
                else if (request.Context.CustomerId.HasValue && request.Context.CustomerId.Value == request.CustomerId)
                {
                    _logger.LogInformation("Customer retrieving their own invoices for customer {CustomerId}", request.CustomerId);
                    customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                }
                else
                {
                    return OperationResult<List<CustomerInvoiceDto>>.FailureResult("Access denied.");
                }

                if (customer == null)
                {
                    return OperationResult<List<CustomerInvoiceDto>>.FailureResult("Customer not found or access denied.");
                }

                var invoices = await _unitOfWork.Customers.GetCustomerInvoicesAsync(request.CustomerId);

                var invoiceDtos = _mapper.Map<List<CustomerInvoiceDto>>(invoices);

                return OperationResult<List<CustomerInvoiceDto>>.SuccessResult(invoiceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer invoices for customer {CustomerId}", request.CustomerId);
                return OperationResult<List<CustomerInvoiceDto>>.FailureResult("Failed to retrieve customer invoices");
            }
        }
    }
}