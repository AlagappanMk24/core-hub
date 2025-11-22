using Microsoft.AspNetCore.Authorization;

namespace Core_API.Application.CrossCuttingConcerns.Authorization.Requirements
{
    public class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}