using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Email.EmailSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core_API.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController(IEmailService emailService, ILogger<EmailController> logger) : BaseApiController
    {
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly ILogger<EmailController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmailSettingsDto>> GetEmailSettings()
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Retrieving email settings for company {CompanyId}", context.CompanyId);

                var result = await _emailService.GetEmailSettingsAsync(context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Email settings retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Email Settings Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during email settings retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email settings for company {CompanyId}", context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving email settings."
                });
            }
        }

        [HttpPost("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SaveEmailSettings([FromBody] EmailSettingsDto emailSettingsDto)
        {
            var operationContext = CurrentContext;
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for email settings save.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid email settings data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                _logger.LogInformation("Saving email settings for company {CompanyId} by user {UserId}", operationContext.CompanyId, operationContext.UserId);

                var result = await _emailService.SaveEmailSettingsAsync(emailSettingsDto, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Email settings save failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Email Settings Save Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during email settings save.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving email settings for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while saving email settings."
                });
            }
        }
    }
}