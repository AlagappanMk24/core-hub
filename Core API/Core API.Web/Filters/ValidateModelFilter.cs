using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Core_API.Web.Filters;

/// <summary>
/// Action filter that automatically validates model state before executing controller actions.
/// </summary>
/// <remarks>
/// This filter intercepts requests before they reach controller actions and checks
/// if the model state is valid. If validation fails, it returns a 400 Bad Request
/// response with validation error details, preventing the action from executing.
/// </remarks>
public class ValidateModelFilter : IActionFilter
{
    #region Public Methods

    /// <summary>
    /// Called before the action method executes. Validates the model state.
    /// </summary>
    /// <param name="context">The context containing the current action execution details.</param>
    /// <remarks>
    /// If the model state is invalid, this method:
    /// <list type="number">
    /// <item><description>Sets the action result to a <see cref="BadRequestObjectResult"/></description></item>
    /// <item><description>Returns the model state errors to the client</description></item>
    /// <item><description>Short-circuits the pipeline so the action method is not executed</description></item>
    /// </list>
    /// </remarks>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        #region Model State Validation

        // Check if the model state is valid (no validation errors)
        if (!context.ModelState.IsValid)
        {
            // Return 400 Bad Request with validation errors
            context.Result = new BadRequestObjectResult(context.ModelState);
        }

        #endregion
    }

    /// <summary>
    /// Called after the action method executes.
    /// </summary>
    /// <param name="context">The context containing the current action execution details.</param>
    /// <remarks>
    /// This method is intentionally left empty because no post-execution processing is required.
    /// It exists only to satisfy the <see cref="IActionFilter"/> interface requirements.
    /// </remarks>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No post-execution processing needed - intentionally empty
    }

    #endregion
}