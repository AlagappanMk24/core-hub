using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Commands.UpdateCustomer
{
    /// <summary>
    /// Handler for updating a customer
    /// </summary>
    public class UpdateCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateCustomerCommandHandler> logger) : IRequestHandler<UpdateCustomerCommand, OperationResult<CustomerResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateCustomerCommandHandler> _logger = logger;

        public async Task<OperationResult<CustomerResponseDto>> Handle(
            UpdateCustomerCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!request.Context.CompanyId.HasValue)
                    return OperationResult<CustomerResponseDto>.FailureResult("Company ID is required.");

                // Get existing customer
                var customer = await _unitOfWork.Customers.GetAsync(
                    c => c.Id == request.Id && c.CompanyId == request.Context.CompanyId && !c.IsDeleted);

                if (customer == null)
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("Customer not found.");
                }

                // Check email uniqueness if changed
                if (customer.Email.Value != request.Email)
                {
                    var exists = await _unitOfWork.Customers.ExistsAsync(request.Context, request.Email);
                    if (exists)
                    {
                        return OperationResult<CustomerResponseDto>.FailureResult("A customer with this email already exists.");
                    }
                }

                // Update properties
                customer.Name = request.Name;
                customer.Email = new Email(request.Email);
                customer.PhoneNumber = new PhoneNumber(request.PhoneNumber);
                customer.TaxId = request.TaxId;
                customer.Website = request.Website;
                customer.CreditLimit = request.CreditLimit;
                customer.DefaultPaymentTerms = request.DefaultPaymentTerms;
                customer.DefaultCurrency = request.DefaultCurrency;
                customer.CustomerGroupId = request.CustomerGroupId;
                customer.Address = new Address(
                    addressLine1: request.AddressLine1,
                    city: request.City,
                    zipCode: request.ZipCode,
                    countryCode: request.CountryCode,
                    addressLine2: request.AddressLine2,
                    state: request.State
                );
                customer.UpdatedBy = request.Context.UserId;
                customer.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = _mapper.Map<CustomerResponseDto>(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", request.Id);
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to update customer.");
            }
        }
    }
}
