using Core_API.Application.Common.Models;
using System.Security.Claims;

namespace Core_API.Web.Services
{
    public class OperationContextService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<OperationContextService> logger) : IOperationContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<OperationContextService> _logger = logger;

        public OperationContext GetCurrentContext()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("No authenticated user found");
                return null;
            }

            return BuildOperationContext(user);
        }

        public async Task<OperationContext> GetCurrentContextAsync()
        {
            // For async operations if needed
            return await Task.FromResult(GetCurrentContext());
        }

        private OperationContext BuildOperationContext(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var companyIdClaim = user.FindFirst("companyId")?.Value;
            var customerIdClaim = user.FindFirst("customerId")?.Value;

            // Get all roles
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isSuperAdmin = roles.Contains("Super Admin");

            int? companyId = int.TryParse(companyIdClaim, out var cId) && cId > 0 ? cId : null;
            int? customerId = int.TryParse(customerIdClaim, out var custId) && custId > 0 ? custId : null;

            _logger.LogDebug("Built OperationContext - UserId: {UserId}, CompanyId: {CompanyId}, CustomerId: {CustomerId}, IsSuperAdmin: {IsSuperAdmin}, Roles: {Roles}",
                userId, companyId, customerId, isSuperAdmin, string.Join(",", roles));

            return new OperationContext(userId, companyId, customerId, isSuperAdmin, roles);
        }
    }
}