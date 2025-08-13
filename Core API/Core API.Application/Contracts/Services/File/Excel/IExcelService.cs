using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;

namespace Core_API.Application.Contracts.Services.File.Excel
{
    public interface IExcelService
    {
        Task<OperationResult<byte[]>> ExportInvoicesExcelAsync(OperationContext operationContext, int pageNumber, int pageSize, string search = null, string status = null);
        byte[] GenerateImportTemplate();
    }
}
