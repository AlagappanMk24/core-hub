using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace Core_API.Web.Controllers
{
    /// <summary>
    /// Abstract base controller that provides common functionality and access to application context.
    /// </summary>
    /// <remarks>
    /// This base controller provides:
    /// <list type="bullet">
    /// <item><description>Access to the current authenticated user's context via <see cref="ICurrentUserService"/></description></item>
    /// <item><description>Convenience properties for common user context information</description></item>
    /// <item><description>Standard API routing configuration</description></item>
    /// </list>
    /// All API controllers should inherit from this base class to ensure consistent access to user context.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        #region Private Fields

        private ICurrentUserService? _currentUserService;

        #endregion

        // Use ICurrentUserService from Application layer contract
        protected ICurrentUserService CurrentUserService =>
            _currentUserService ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        #region Protected Properties

        /// <summary>
        /// Gets the current user service instance from the HTTP context.
        /// </summary>
        /// <remarks>
        /// This property uses lazy initialization to resolve the service from the request's service provider.
        /// This ensures the service is only resolved when first accessed, improving performance.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <see cref="ICurrentUserService"/> cannot be resolved from the service provider.
        /// </exception>
        /// <summary>
        /// Gets the complete operation context for the current user.
        /// </summary>
        /// <returns>
        /// An <see cref="OperationContext"/> object containing user ID, company ID, customer ID, roles, and permissions.
        /// </returns>
        protected OperationContext CurrentContext => CurrentUserService.GetCurrentContext();

        /// <summary>
        /// Gets the unique identifier of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// The user ID as a string, or null if no authenticated user is present.
        /// </returns>
        protected string? CurrentUserId => CurrentUserService.UserId;

        /// <summary>
        /// Gets the company ID associated with the current user.
        /// </summary>
        /// <returns>
        /// The company ID as an integer, or null if the user is not associated with a company.
        /// </returns>
        protected int? CurrentCompanyId => CurrentUserService.CompanyId;

        /// <summary>
        /// Gets the customer ID associated with the current user.
        /// </summary>
        /// <returns>
        /// The customer ID as an integer, or null if the user is not associated with a customer.
        /// </returns>
        protected int? CurrentCustomerId => CurrentUserService.CustomerId;

        /// <summary>
        /// Gets a value indicating whether the current user has Super Administrator privileges.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the user is a Super Admin; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsSuperAdmin => CurrentUserService.IsSuperAdmin;

        /// <summary>
        /// Gets a value indicating whether the current user has Administrator privileges.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the user is an Admin; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsAdmin => CurrentUserService.IsAdmin;

        /// <summary>
        /// Gets the list of roles assigned to the current user.
        /// </summary>
        /// <returns>
        /// A list of role names (e.g., "Admin", "User", "Customer").
        /// </returns>
        protected List<string> UserRoles => CurrentUserService.Roles;

        #endregion
    }
}