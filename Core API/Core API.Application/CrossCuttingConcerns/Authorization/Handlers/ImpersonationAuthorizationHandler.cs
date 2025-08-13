using Core_API.Application.Common.Constants;
using Core_API.Application.CrossCuttingConcerns.Authorization.Requirements;
using Core_API.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Core_API.Application.CrossCuttingConcerns.Authorization.Handlers
{
    public class ImpersonationAuthorizationHandler(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor, ILogger<ImpersonationAuthorizationHandler> logger) : AuthorizationHandler<ImpersonationAuthorizationRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<ImpersonationAuthorizationHandler> _logger = logger;

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ImpersonationAuthorizationRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                _logger.LogError("HttpContext is null in ImpersonationAuthorizationHandler.");
                context.Fail();
                return;
            }

            var username = httpContext.User?.Identity?.Name ?? "Anonymous";
            _logger.LogInformation("Impersonation authorization check for user: {Username}", username);

            var originalAdminId = httpContext.Session.GetString("OriginalAdminId");
            _logger.LogInformation("OriginalAdminId in session: {OriginalAdminId}, Session keys: {SessionKeys}",
                originalAdminId, string.Join(", ", httpContext.Session.Keys));

            // If there's no OriginalAdminId in the session, deny access
            if (string.IsNullOrEmpty(originalAdminId))
            {
                _logger.LogWarning("No OriginalAdminId found in session for user: {Username}", username);
                context.Fail();
                return;
            }

            // Verify that the original admin has the Role_Admin role
            var originalAdmin = await _userManager.FindByIdAsync(originalAdminId);
            if (originalAdmin == null || !await _userManager.IsInRoleAsync(originalAdmin, AppConstants.Role_Admin))
            {
                _logger.LogWarning("Original admin not found or lacks Role_Admin. OriginalAdminId: {OriginalAdminId}, User: {Username}",
                originalAdminId, username);
                context.Fail();
                return;
            }
            _logger.LogInformation("Impersonation authorization successful for user: {Username}, OriginalAdminId: {OriginalAdminId}",
            username, originalAdminId);
            // Authorization successful
            context.Succeed(requirement);
        }
    }
}
