using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Invoice.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Web.Controllers.Invoice
{
    /// <summary>
    /// Controller for managing tax types
    /// </summary>
    [Route("api/invoice/tax-types")]
    [ApiController]
    [Authorize(Roles = "Admin, User")]
    public class TaxTypeController(ITaxService taxService, ILogger<TaxTypeController> logger) : BaseApiController
    {
        private readonly ITaxService _taxService = taxService ?? throw new ArgumentNullException(nameof(taxService));
        private readonly ILogger<TaxTypeController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Retrieves a list of available tax types for the authenticated user's company.
        /// </summary>
        /// <returns>A list of <see cref="TaxTypeResponseDto"/>.</returns>
        /// <response code="200">Tax types retrieved successfully.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error or retrieval failed.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetTaxTypes()
        {
            var operationContext = CurrentContext;
            try
            {
                _logger.LogInformation("Retrieving tax types for company {CompanyId}", operationContext.CompanyId);

                var result = await _taxService.GetTaxTypesAsync(operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Tax types retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Tax Types Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during tax types retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tax types for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving tax types."
                });
            }
        }

        /// <summary>
        /// Creates a new tax type for the authenticated user's company.
        /// </summary>
        /// <param name="dto">The tax type data to create.</param>
        /// <returns>The created <see cref="TaxTypeResponseDto"/>.</returns>
        /// <response code="201">Tax type created successfully.</response>
        /// <response code="400">Invalid request data or creation failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTaxType([FromBody] TaxTypeCreateDto dto)
        {
            var operationContext = CurrentContext;
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for tax type creation.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid tax type data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }
                _logger.LogInformation("Creating tax type for company {CompanyId} by user {UserId}", operationContext.CompanyId, operationContext.UserId);

                var result = await _taxService.CreateTaxTypeAsync(dto, operationContext);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Tax type creation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Tax Type Creation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return CreatedAtAction(nameof(GetTaxTypes), null, result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during tax type creation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating tax type for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while creating the tax type in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax type for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while creating the tax type."
                });
            }
        }
    }
}
