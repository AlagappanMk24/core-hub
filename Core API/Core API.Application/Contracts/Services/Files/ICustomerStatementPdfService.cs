using Core_API.Application.Common.Models;
using Core_API.Domain.Entities.Customers;

namespace Core_API.Application.Contracts.Services.Files
{
    public interface ICustomerStatementPdfService
    {
        Task<MemoryStream> GenerateStatementAsync(Customer customer, OperationContext operationContext);
    }
}