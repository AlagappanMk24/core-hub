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
    public class EmailController(IEmailService emailService, ILogger<EmailController> logger) : ControllerBase
    {
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly ILogger<EmailController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private OperationContext GetOperationContext()
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            var customerIdClaim = User.FindFirst("customerId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID claim is missing.");
                throw new UnauthorizedAccessException("User ID claim is missing.");
            }

            int? companyId = null;
            if (!string.IsNullOrEmpty(companyIdClaim) && int.TryParse(companyIdClaim, out var parsedCompanyId) && parsedCompanyId > 0)
            {
                companyId = parsedCompanyId;
            }
            else if (!string.IsNullOrEmpty(companyIdClaim))
            {
                _logger.LogWarning("Invalid CompanyId claim: {CompanyIdClaim}", companyIdClaim);
            }

            int? customerId = null;
            if (!string.IsNullOrEmpty(customerIdClaim) && int.TryParse(customerIdClaim, out var parsedCustomerId) && parsedCustomerId > 0)
            {
                customerId = parsedCustomerId;
            }
            else if (!string.IsNullOrEmpty(customerIdClaim))
            {
                _logger.LogWarning("Invalid CustomerId claim: {CustomerIdClaim}", customerIdClaim);
            }

            return new OperationContext(userId, companyId, customerId);
        }

        [HttpGet("settings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmailSettingsDto>> GetEmailSettings()
        {
            var operationContext = GetOperationContext();
            try
            {
                _logger.LogInformation("Retrieving email settings for company {CompanyId}", operationContext.CompanyId);

                var result = await _emailService.GetEmailSettingsAsync(operationContext);
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
                _logger.LogError(ex, "Error retrieving email settings for company {CompanyId}", operationContext.CompanyId);
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
            var operationContext = GetOperationContext();
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