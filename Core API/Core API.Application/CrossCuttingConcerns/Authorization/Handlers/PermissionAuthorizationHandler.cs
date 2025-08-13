using Core_API.Application.Contracts.Services;
using Core_API.Application.CrossCuttingConcerns.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Core_API.Application.CrossCuttingConcerns.Authorization.Handlers
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
