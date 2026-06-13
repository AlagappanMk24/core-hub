using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Invoices.Requests;

namespace Core_API.Application.Contracts.Services.Taxes
{
    public interface ITaxService
    {
        Task<OperationResult<List<TaxTypeDto>>> GetTaxTypesAsync(OperationContext operationContext);
        Task<OperationResult<TaxTypeDto>> CreateTaxTypeAsync(TaxTypeCreateDto dto, OperationContext operationContext);
    }
}