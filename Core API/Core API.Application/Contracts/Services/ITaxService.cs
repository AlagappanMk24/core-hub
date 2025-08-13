using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoice.Request;

namespace Core_API.Application.Contracts.Services
{
    public interface ITaxService
    {
        Task<OperationResult<List<TaxTypeDto>>> GetTaxTypesAsync(OperationContext operationContext);
        Task<OperationResult<TaxTypeDto>> CreateTaxTypeAsync(TaxTypeCreateDto dto, OperationContext operationContext);
    }
}