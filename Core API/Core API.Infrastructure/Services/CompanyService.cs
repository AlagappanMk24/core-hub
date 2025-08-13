using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Company.Request;
using Core_API.Application.DTOs.Company.Response;
using Core_API.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services
{
    public class CompanyService(IUnitOfWork unitOfWork, ILogger<CompanyService> logger) : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<CompanyService> _logger = logger;

        public async Task<CompanyResponseDto> CreateCompanyAsync(CompanyCreateDto companyDto, string userId)
        {
            if (string.IsNullOrWhiteSpace(companyDto.Name) || companyDto.Name.Length > 100)
                throw new ArgumentException("Company name is required and must be less than 100 characters.");

            var existingCompany = await _unitOfWork.Companies.GetAsync(c => c.Name == companyDto.Name && !c.IsDeleted);
            if (existingCompany != null)
                throw new InvalidOperationException("A company with this name already exists.");

            var company = new Company
            {
                Name = companyDto.Name,
                TaxId = companyDto.TaxId,
                Address = companyDto.Address,
                Email = companyDto.Email,
                PhoneNumber = companyDto.PhoneNumber,
                CreatedByUserId = userId
            };

            await _unitOfWork.Companies.AddAsync(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Company {CompanyName} created by user {UserId}", companyDto.Name, userId);

            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                TaxId = company.TaxId,
                Address = company.Address,
                Email = company.Email,
                PhoneNumber = company.PhoneNumber,
                IsDeleted = company.IsDeleted,
                CreatedByUserId = company.CreatedByUserId,
                CreatedAt = (DateTime)company.CreatedDate,
                UpdatedAt = company.UpdatedDate
            };
        }

        public async Task<CompanyResponseDto> GetCompanyByIdAsync(int id)
        {
            var company = await _unitOfWork.Companies.GetByIdAsync(id);
            if (company == null)
                throw new KeyNotFoundException($"Company with ID {id} not found.");

            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                TaxId = company.TaxId,
                Address = company.Address,
                Email = company.Email,
                PhoneNumber = company.PhoneNumber,
                IsDeleted = company.IsDeleted,
                CreatedByUserId = company.CreatedByUserId,
                CreatedAt = (DateTime)company.CreatedDate,
                UpdatedAt = company.UpdatedDate
            };
        }
        public async Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            var companies = await _unitOfWork.Companies.GetAllAsync(c => !c.IsDeleted);
            return companies.Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                TaxId = c.TaxId,
                Address = c.Address,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                IsDeleted = c.IsDeleted,
                CreatedByUserId = c.CreatedByUserId,
                CreatedAt = (DateTime)c.CreatedDate,
                UpdatedAt = c.UpdatedDate
            }).ToList();
        }
        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var company = await _unitOfWork.Companies.GetByIdAsync(id);
            if (company == null)
                return false;

            var canDelete = await _unitOfWork.Companies.CanDeleteAsync(id);
            if (!canDelete)
                throw new InvalidOperationException("Cannot delete company with associated customers or invoices.");

            company.IsDeleted = true;
            _unitOfWork.Companies.Update(company);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Company {CompanyId} soft-deleted", id);
            return true;
        }
    }
}
