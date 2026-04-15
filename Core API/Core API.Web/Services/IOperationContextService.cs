using Core_API.Application.Common.Models;

namespace Core_API.Web.Services
{
    public interface IOperationContextService
    {
        OperationContext GetCurrentContext();
        Task<OperationContext> GetCurrentContextAsync();
    }
}
