using Core_API.Application.Contracts.Services;
using Core_API.Application.Contracts.Services.File.Excel;
using Core_API.Application.Contracts.Services.File.Pdf;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.User.Response;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Services.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core_API.Web.Controllers.Invoice
{
    [Route("api/invoice/settings")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class InvoiceSettingsController(IInvoiceSettingsService invoiceSettingsService, ILogger<InvoiceController> logger) : BaseApiController
    {
        private readonly IInvoiceSettingsService _invoiceSettingsService = invoiceSettingsService ?? throw new ArgumentNullException(nameof(invoiceSettingsService));

        private readonly ILogger<InvoiceController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Retrieves the invoice settings for the authenticated user's company.
        /// </summary>
        /// <returns>The <see cref="InvoiceSettings"/> object.</returns>
        /// <response code="200">Invoice settings retrieved successfully.</response>
        /// <response code="404">Invoice settings not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error or retrieval failed.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<InvoiceSettings>> GetInvoiceSettings([FromQuery] int? companyId = null)
        {
            var operationContext = CurrentContext;
            try
            {
                _logger.LogInformation("Retrieving invoice settings for company {CompanyId}", operationContext.CompanyId);

                var result = await _invoiceSettingsService.GetInvoiceSettingsAsync(operationContext, companyId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice settings retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Invoice Settings Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice settings retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice settings for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving invoice settings."
                });
            }
        }


        /// <summary>
        /// Saves or updates the invoice settings for the authenticated user's company.
        /// </summary>
        /// <param name="invoiceSettingsDto">The invoice settings data to save.</param>
        /// <returns>A success response if the settings were saved.</returns>
        /// <response code="200">Invoice settings saved successfully.</response>
        /// <response code="400">Invalid request data or settings save failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SaveInvoiceSettings([FromBody] InvoiceSettingsDto invoiceSettingsDto)
        {
            var operationContext = CurrentContext;
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for invoice settings save.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid invoice settings data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                _logger.LogInformation("Saving invoice settings for company {CompanyId} by user {UserId}", operationContext.CompanyId, operationContext.UserId);

                var result = await _invoiceSettingsService.SaveInvoiceSettingsAsync(invoiceSettingsDto, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice settings save failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Settings Save Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice settings save.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving invoice settings for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while saving invoice settings."
                });
            }
        }
    }
}