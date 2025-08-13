﻿using Core_API.Domain.Entities;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.DTOs.Customer.Response;

namespace Core_API.Infrastructure.Services
{
    public class CustomerService(IUnitOfWork unitOfWork, ILogger<CustomerService> logger) : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<CustomerService> _logger = logger;
        public async Task<OperationResult<CustomerResponseDto>> CreateAsync(CustomerCreateDto dto, int companyId, string userId)
        {
            try
            {
                if (await _unitOfWork.Customers.ExistsAsync(companyId, dto.Email))
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("A customer with this email already exists.");
                }

                var customer = new Customer
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Address = new Address
                    {
                        Address1 = dto.Address1,
                        Address2 = dto.Address2,
                        City = dto.City,
                        State = dto.State,
                        Country = dto.Country,
                        ZipCode = dto.ZipCode
                    },
                    CompanyId = companyId,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToCustomerResponseDto(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer for company {CompanyId}", companyId);
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to create customer.");
            }
        }
        public async Task<OperationResult<CustomerResponseDto>> UpdateAsync(CustomerUpdateDto dto, int companyId, string userId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == dto.Id && c.CompanyId == companyId && !c.IsDeleted);
                if (customer == null)
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                if (await _unitOfWork.Customers.ExistsAsync(companyId, dto.Email) && customer.Email != dto.Email)
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("A customer with this email already exists.");
                }

                customer.Name = dto.Name;
                customer.Email = dto.Email;
                customer.PhoneNumber = dto.PhoneNumber;
                customer.Address.Address1 = dto.Address1;
                customer.Address.Address2 = dto.Address2;
                customer.Address.City = dto.City;
                customer.Address.State = dto.State;
                customer.Address.Country = dto.Country;
                customer.Address.ZipCode = dto.ZipCode;
                customer.UpdatedBy = userId;
                customer.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync();

                var response = MapToCustomerResponseDto(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId} for company {CompanyId}", dto.Id, companyId);
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to update customer.");
            }
        }
        public async Task<OperationResult<bool>> DeleteAsync(int id, int companyId, string userId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == id && c.CompanyId == companyId && !c.IsDeleted, "Invoices");
                if (customer == null)
                {
                    return OperationResult<bool>.FailureResult("Customer not found or does not belong to your company.");
                }

                if (customer.Invoices.Any())
                {
                    return OperationResult<bool>.FailureResult("Cannot delete customer with associated invoices.");
                }

                customer.IsDeleted = true;
                customer.UpdatedBy = userId;
                customer.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync();

                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId} for company {CompanyId}", id, companyId);
                return OperationResult<bool>.FailureResult("Failed to delete customer.");
            }
        }
        public async Task<OperationResult<CustomerResponseDto>> GetByIdAsync(int id, int companyId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetAsync(c => c.Id == id && c.CompanyId == companyId && !c.IsDeleted);
                if (customer == null)
                {
                    return OperationResult<CustomerResponseDto>.FailureResult("Customer not found or does not belong to your company.");
                }

                var response = MapToCustomerResponseDto(customer);
                return OperationResult<CustomerResponseDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer {CustomerId} for company {CompanyId}", id, companyId);
                return OperationResult<CustomerResponseDto>.FailureResult("Failed to retrieve customer.");
            }
        }
        public async Task<OperationResult<PaginatedResult<CustomerResponseDto>>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string search)
        {
            try
            {
                var result = await _unitOfWork.Customers.GetPagedAsync(companyId, pageNumber, pageSize, search);
                var response = new PaginatedResult<CustomerResponseDto>
                {
                    Items = result.Items.Select(MapToCustomerResponseDto).ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };
                return OperationResult<PaginatedResult<CustomerResponseDto>>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged customers for company {CompanyId}", companyId);
                return OperationResult<PaginatedResult<CustomerResponseDto>>.FailureResult("Failed to retrieve customers.");
            }
        }
        private CustomerResponseDto MapToCustomerResponseDto(Customer customer)
        {
            return new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                CompanyId = (int)customer.CompanyId,
                Address = new AddressDto
                {
                    Address1 = customer.Address.Address1,
                    Address2 = customer.Address.Address2,
                    City = customer.Address.City,
                    State = customer.Address.State,
                    Country = customer.Address.Country,
                    ZipCode = customer.Address.ZipCode
                }
            };
        }
    }
}