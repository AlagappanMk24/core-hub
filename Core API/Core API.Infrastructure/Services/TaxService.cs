using AutoMapper;
using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Services
{
    public class TaxService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TaxService> logger) : ITaxService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<TaxService> _logger = logger;
        public async Task<OperationResult<List<TaxTypeDto>>> GetTaxTypesAsync(OperationContext operationContext)
        {
            try
            {
                var taxTypes = await _unitOfWork.TaxTypes.Query()
                  .Where(t => t.CompanyId == operationContext.CompanyId && !t.IsDeleted)
                  .ToListAsync();

                var response = _mapper.Map<List<TaxTypeDto>>(taxTypes);
                return OperationResult<List<TaxTypeDto>>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax types for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<List<TaxTypeDto>>.FailureResult("Failed to retrieve tax types.");
            }
        }
        public async Task<OperationResult<TaxTypeDto>> CreateTaxTypeAsync(TaxTypeCreateDto dto, OperationContext operationContext)
        {
            try
            {
                // Validate CompanyId
                if (!operationContext.CompanyId.HasValue)
                {
                    _logger.LogWarning("Company ID is required for creating tax types");
                    return OperationResult<TaxTypeDto>.FailureResult("Company ID is required.");
                }
                int companyId = operationContext.CompanyId.Value;
                var existingTaxType = await _unitOfWork.TaxTypes.GetAsync(t => t.Name == dto.Name && t.CompanyId == companyId && !t.IsDeleted);
                if (existingTaxType != null)
                {
                    return OperationResult<TaxTypeDto>.FailureResult("Tax type with this name already exists.");
                }

                var taxType = _mapper.Map<TaxType>(dto);
                taxType.CompanyId = companyId;
                taxType.CreatedBy = operationContext.UserId;
                taxType.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.TaxTypes.AddAsync(taxType);
                await _unitOfWork.SaveChangesAsync();

                var response = _mapper.Map<TaxTypeDto>(taxType);
                return OperationResult<TaxTypeDto>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax type for company {CompanyId}", operationContext.CompanyId);
                return OperationResult<TaxTypeDto>.FailureResult("Failed to create tax type.");
            }
        }

    }
}