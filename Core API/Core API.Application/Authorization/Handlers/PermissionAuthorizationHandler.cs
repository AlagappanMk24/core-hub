using Core_API.Application.Authorization.Requirements;
using Core_API.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;

namespace Core_API.Application.Authorization.Handlers
{
    public class PermissionAuthorizationHandler(IPermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService = permissionService;
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return; // Not authenticated, so can't have any permissions
            }

            // Check if the user has required permission
            var hasPermission = await _permissionService.UserHasPermissionAsync(
                context.User, requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
