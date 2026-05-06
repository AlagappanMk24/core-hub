using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services.Customers;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services.Customer
{
    public class CustomerService(IUnitOfWork unitOfWork, ILogger<CustomerService> logger) : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<CustomerService> _logger = logger;
        public async Task<OperationResult<CustomerStatsDto>> GetStatsAsync(OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                    return OperationResult<CustomerStatsDto>.FailureResult("Company ID is required.");

                var companyId = context.CompanyId.Value;
                // Current counts
                var allCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId);
                var activeCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && !c.IsDeleted);
                var inactiveCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && c.IsDeleted);

                // Historical counts (e.g., 30 days ago)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var historicalAllCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && c.CreatedDate <= thirtyDaysAgo);
                var historicalActiveCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && !c.IsDeleted && c.CreatedDate <= thirtyDaysAgo);
                var historicalInactiveCount = await _unitOfWork.Customers.CountAsync(c => c.CompanyId == companyId && c.IsDeleted && c.CreatedDate <= thirtyDaysAgo);

                // Calculate percentage changes
                double allChange = historicalAllCount > 0 ? (double)(allCount - historicalAllCount) / historicalAllCount * 100 : 0;
                double activeChange = historicalActiveCount > 0 ? (double)(activeCount - historicalActiveCount) / historicalActiveCount * 100 : 0;
                double inactiveChange = historicalInactiveCount > 0 ? (double)(inactiveCount - historicalInactiveCount) / historicalInactiveCount * 100 : 0;

                var stats = new CustomerStatsDto
                {
                    AllCount = allCount,
                    AllChange = Math.Round(allChange, 1),
                    ActiveCount = activeCount,
                    ActiveChange = Math.Round(activeChange, 1),
                    InactiveCount = inactiveCount,
                    InactiveChange = Math.Round(inactiveChange, 1)
                };

                return OperationResult<CustomerStatsDto>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer stats for company {CompanyId}", context.CompanyId);
                return OperationResult<CustomerStatsDto>.FailureResult("Failed to retrieve customer statistics.");
            }
        }
        public async Task<OperationResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto, OperationContext context)
        {
            try
            {
                if (await _unitOfWork.Customers.ExistsAsync(context, dto.Email))
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("A customer with this email already exists.");
                }

                // Create Email value object
                var email = new Email(dto.Email);

                // Create PhoneNumber value object
                var phoneNumber = new PhoneNumber(dto.PhoneNumber);

                // Create Address value object
                var address = new Address(
                    addressLine1: dto.AddressLine1,
                    city: dto.City,
                    zipCode: dto.ZipCode,
                    countryCode: dto.CountryCode,
                    addressLine2: dto.AddressLine2,
                    state: dto.State
                );

                var customer = new Domain.Entities.Customers.Customer
                {
                    Name = dto.Name,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Address = address,
                    CompanyId = context.CompanyId.Value,
                    CreatedBy = context.UserId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToCustomerResponseDto(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer for company {CompanyId}", context.CompanyId);
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to create customer.");
            }
        }
        public async Task<OperationResult<CustomerResponseDto>> UpdateAsync(UpdateCustomerDto dto, OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                    return OperationResult<CustomerResponseDto>.FailureResult("Company ID is required.");

                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == dto.Id && c.CompanyId == context.CompanyId && !c.IsDeleted);
                if (customer == null)
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                // Check email uniqueness if changed
                if (customer.Email.Value != dto.Email)
                {
                    var exists = await _unitOfWork.Customers.ExistsAsync(context, dto.Email);
                    if (exists)
                    {
                        return OperationResult<CustomerResponseDto>.FailureResult("A customer with this email already exists.");
                    }
                }

                // Update properties
                customer.Name = dto.Name;
                customer.Email = new Email(dto.Email);
                customer.PhoneNumber = new PhoneNumber(dto.PhoneNumber);

                // Update Address (create new instance - Value Objects are immutable)
                customer.Address = new Address(
                    addressLine1: dto.AddressLine1,
                    city: dto.City,
                    zipCode: dto.ZipCode,
                    countryCode: dto.CountryCode,
                    addressLine2: dto.AddressLine2,
                    state: dto.State
                );

                customer.UpdatedBy = context.UserId;
                customer.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToCustomerResponseDto(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId} for company {CompanyId}", dto.Id, context.CompanyId);
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to update customer.");
            }
        }
        public async Task<OperationResult<bool>> DeleteAsync(int id, OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                    return OperationResult<bool>.FailureResult("Company ID is required.");

                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == id && c.CompanyId == context.CompanyId && !c.IsDeleted, "Invoices");
                if (customer == null)
                {
                    return OperationResult<bool>.FailureResult("Customer not found or does not belong to your company.");
                }

                // Check if customer has invoices
                var hasInvoices = await _unitOfWork.Customers.HasInvoicesAsync(id);
                if (hasInvoices)
                {
                    return OperationResult<bool>.FailureResult("Cannot delete customer with associated invoices.");
                }

                customer.IsDeleted = true;
                customer.UpdatedBy = context.UserId;
                customer.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync();

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId} for company {CompanyId}", id, context.CompanyId);
                return OperationResult<bool>.FailureResult("Failed to delete customer.");
            }
        }
        public async Task<OperationResult<CustomerResponseDto>> GetByIdAsync(int id, OperationContext context)
        {
            try
            {
                if (!context.CompanyId.HasValue)
                    return OperationResult<CustomerResponseDto>.FailureResult("Company ID is required.");

                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == id && c.CompanyId == context.CompanyId.Value && !c.IsDeleted);
                if (customer == null)
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                var response = MapToCustomerResponseDto(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer {CustomerId} for company {CompanyId}", id, context.CompanyId);
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to retrieve customer.");
            }
        }
        public async Task<OperationResult<PaginatedResult<CustomerResponseDto>>> GetPagedAsync(OperationContext operationContext, CustomerFilterRequestDto filter)
        {
            try
            {
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for retrieving paged customers.");
                    return OperationResult<PaginatedResult<CustomerResponseDto>>.FailureResult("Company ID is required.");
                }

                if (!filter.IsValid())
                {
                    _logger.LogWarning("Invalid filter parameters for retrieving paged customers.");
                    return OperationResult<PaginatedResult<CustomerResponseDto>>.FailureResult("Invalid filter parameters.");
                }

                int companyId = operationContext.CompanyId.Value;

                var result = await _unitOfWork.Customers.GetPagedAsync(companyId, filter);

                var mappedItems = result.Items.Select(MapToCustomerResponseDto).ToList();

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
                _logger.LogError(ex, "Error retrieving paged customers for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<PaginatedResult<CustomerResponseDto>>.FailureResult("Failed to retrieve customers.");
            }
        }
        private CustomerResponseDto MapToCustomerResponseDto(Domain.Entities.Customers.Customer customer)
        {
            return new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email.Value,
                PhoneNumber = customer.PhoneNumber.Value,
                CompanyId = customer.CompanyId,
                AddressLine1 = customer.Address.AddressLine1,
                AddressLine2 = customer.Address.AddressLine2,
                City = customer.Address.City,
                State = customer.Address.State,
                CountryCode = customer.Address.CountryCode,
                CountryName = customer.Address.CountryName,
                ZipCode = customer.Address.ZipCode,
            };
        }
    }
}