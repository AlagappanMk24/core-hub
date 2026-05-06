using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Enums;
using Core_API.Domain.Exceptions;
using Core_API.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.Features.Customers.Commands.CreateCustomer
{
    /// <summary>
    /// Handler for creating a new customer
    /// </summary>

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCustomerCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for database operations.</param>
    /// <param name="mapper">The AutoMapper instance for object mapping.</param>
    /// <param name="logger">The logger instance.</param>
    public sealed class CreateCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateCustomerCommandHandler> logger) : IRequestHandler<CreateCustomerCommand, OperationResult<CustomerResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<CreateCustomerCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Handles the create customer command.
        /// </summary>
        /// <param name="request">The create customer command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The operation result containing the created customer DTO.</returns>
        public async Task<OperationResult<CustomerResponseDto>> Handle(
            CreateCustomerCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Context is already injected! Use it directly
                var context = request.Context;

                _logger.LogInformation("Creating customer for company {CompanyId} by user {UserId}",
                    context.CompanyId, context.UserId);

                // Validate company context
                if (!request.Context.CompanyId.HasValue)
                    return OperationResult<CustomerResponseDto>.FailureResult("Company ID is required.");

                // Check if customer already exists
                var exists = await _unitOfWork.Customers.ExistsAsync(context, request.Email);

                if (exists)
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("A customer with this email already exists.");
                }

                // Create value objects from primitive types
                var email = new Email(request.Email);
                var phoneNumber = new PhoneNumber(request.PhoneNumber);
                var address = new Address(
                    addressLine1: request.AddressLine1,
                    city: request.City,
                    zipCode: request.ZipCode,
                    countryCode: request.CountryCode,
                    addressLine2: request.AddressLine2,
                    state: request.State
                );

                // Create customer entity
                var customer = new Customer
                {
                    Name = request.Name,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    TaxId = request.TaxId,
                    Website = request.Website,
                    CreditLimit = request.CreditLimit,
                    DefaultPaymentTerms = request.DefaultPaymentTerms,
                    DefaultCurrency = request.DefaultCurrency,
                    CustomerGroupId = request.CustomerGroupId,
                    Status = CustomerStatus.Active,
                    ActiveSince = DateTime.UtcNow,
                    CompanyId = request.Context.CompanyId.Value,
                    CreatedBy = request.Context.UserId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = _mapper.Map<CustomerResponseDto>(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (InvalidEmailException ex)
            {
                _logger.LogWarning(ex, "Invalid email while creating customer");
                return OperationResult<CustomerResponseDto>.FailureResult(ex.Message);
            }
            catch (InvalidPhoneException ex)
            {
                _logger.LogWarning(ex, "Invalid phone number while creating customer");
                return OperationResult<CustomerResponseDto>.FailureResult(ex.Message);
            }
            catch (InvalidAddressException ex)
            {
                _logger.LogWarning(ex, "Invalid address while creating customer");
                return OperationResult<CustomerResponseDto>.FailureResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to create customer.");
            }
        }
    }
}
