using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Core_API.Infrastructure.Services.Common
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private OperationContext _cachedContext;

        public string? UserId => GetClaimValue(ClaimTypes.NameIdentifier) ?? GetClaimValue("uid");

        public int? CompanyId
        {
            get
            {
                var companyIdClaim = GetClaimValue("companyId");
                return int.TryParse(companyIdClaim, out var id) ? id : null;
            }
        }

        public int? CustomerId
        {
            get
            {
                var customerIdClaim = GetClaimValue("customerId");
                return int.TryParse(customerIdClaim, out var id) ? id : null;
            }
        }

        public bool IsAdmin => Roles.Contains("Admin");
        public bool IsSuperAdmin => Roles.Contains("SuperAdmin");

        public List<string> Roles
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();
            }
        }

        public OperationContext GetCurrentContext()
        {
            if (_cachedContext != null)
                return _cachedContext;

            _cachedContext = new OperationContext(
                userId: UserId ?? throw new UnauthorizedAccessException("User not authenticated"),
                companyId: CompanyId,
                customerId: CustomerId,
                isSuperAdmin: IsSuperAdmin,
                roles: Roles
            );

            return _cachedContext;
        }

        private string? GetClaimValue(string claimType)
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        }
    }
}