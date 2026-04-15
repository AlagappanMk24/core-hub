using Core_API.Application.Common.Models;
using Core_API.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core_API.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        private IOperationContextService _operationContextService;

        protected IOperationContextService OperationContextService =>
            _operationContextService ??= HttpContext.RequestServices.GetRequiredService<IOperationContextService>();

        protected OperationContext CurrentContext =>
            OperationContextService.GetCurrentContext();

        protected async Task<OperationContext> GetCurrentContextAsync() =>
            await OperationContextService.GetCurrentContextAsync();

        protected bool IsSuperAdmin => CurrentContext?.IsSuperAdmin ?? false;
        protected bool IsAdmin => CurrentContext?.Roles.Contains("Admin") ?? false;
        protected bool IsUser => CurrentContext?.Roles.Contains("User") ?? false;
        protected bool IsCustomer => CurrentContext?.Roles.Contains("Customer") ?? false;
    }
}
