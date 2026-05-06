using Core_API.Application.Common.Models;

namespace Core_API.Application.Contracts.Services.Common
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        int? CompanyId { get; }
        int? CustomerId { get; }
        bool IsAdmin { get; }
        bool IsSuperAdmin { get; }
        List<string> Roles { get; }
        OperationContext GetCurrentContext();
    }
}