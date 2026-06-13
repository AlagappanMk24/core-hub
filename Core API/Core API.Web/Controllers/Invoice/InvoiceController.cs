using Core_API.Application.Contracts.Services.Files;
using Core_API.Application.Contracts.Services.Invoice;
using Core_API.Application.Contracts.Services.Invoices;
using Core_API.Application.DTOs.Common;
using Core_API.Application.DTOs.Email.Requests;
using Core_API.Application.DTOs.Invoice.Request;
using Core_API.Application.DTOs.Invoice.Response;
using Core_API.Application.DTOs.Invoices.Requests;
using Core_API.Application.Features.Invoices.Commands.CreateInvoice;
using Core_API.Application.Features.Invoices.Commands.DeleteInvoice;
using Core_API.Application.Features.Invoices.Commands.DuplicateInvoice;
using Core_API.Application.Features.Invoices.Commands.SendInvoice;
using Core_API.Application.Features.Invoices.Commands.UpdateInvoice;
using Core_API.Application.Features.Invoices.Queries.GetInvoiceById;
using Core_API.Application.Features.Invoices.Queries.GetInvoicePdf;
using Core_API.Application.Features.Invoices.Queries.GetInvoicesPaged;
using Core_API.Application.Features.Invoices.Queries.GetInvoiceStats;
using Core_API.Application.Features.Invoices.Queries.GetNextInvoiceNumber;
using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Diagnostics;

namespace Core_API.Web.Controllers.Invoice
{
    /// <summary>
    /// Controller for managing invoice-related operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="InvoiceController"/> class.
    /// </remarks>
    /// <param name="invoiceService">The invoice service for business logic.</param>
    /// <param name="logger">The logger for logging controller actions.</param>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, User, Customer")]
    public class InvoiceController(IInvoiceService invoiceService, IMediator mediator, IInvoiceNumberService invoiceNumberService, IInvoiceStatisticsService invoiceStatisticsService, IExcelService excelService,IPdfService pdfService, ILogger<InvoiceController> logger) : BaseApiController
    {
        private readonly IInvoiceService _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly IInvoiceNumberService _invoiceNumberService = invoiceNumberService ?? throw new ArgumentNullException(nameof(invoiceNumberService));
        private readonly IInvoiceStatisticsService _invoiceStatisticsService = invoiceStatisticsService ?? throw new ArgumentNullException(nameof(invoiceStatisticsService));
        private readonly IExcelService _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        private readonly IPdfService _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        private readonly ILogger<InvoiceController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Creates a new invoice for the authenticated user's company.
        /// </summary>
        /// <param name="dto">The invoice data to create.</param>
        /// <returns>A <see cref="InvoiceResponseDto"/> representing the created invoice.</returns>
        /// <response code="201">Invoice created successfully.</response>
        /// <response code="400">Invalid request data or invoice creation failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Create([FromForm] CreateInvoiceDto dto)
        {
            var operationContext = CurrentContext;
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for invoice creation.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid invoice data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                _logger.LogInformation("Creating invoice for company {CompanyId} by user {UserId}", operationContext.CompanyId, operationContext.UserId);

                if (dto.IsAutomated)
                {
                    dto.InvoiceNumber = $"INV{DateTime.UtcNow.Ticks}";
                }
                var command = CreateInvoiceCommand.FromDto(dto);
                command = command with { Context = CurrentContext };

                // ✅ Send through MediatR
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice creation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Creation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice creation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating invoice for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while creating the invoice in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while creating the invoice."
                });
            }
        }

        /// <summary>
        /// Updates an existing invoice.
        /// </summary>
        /// <param name="id">The ID of the invoice to update.</param>
        /// <param name="dto">The updated invoice data.</param>
        /// <returns>The updated <see cref="InvoiceResponseDto"/>.</returns>
        /// <response code="200">Invoice updated successfully.</response>
        /// <response code="400">Invalid request data or invoice not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateInvoiceDto dto)
        {
            var operationContext = CurrentContext;
            try
            {
                if (!ModelState.IsValid || id != dto.Id)
                {
                    _logger.LogWarning("Invalid model state or ID mismatch for invoice update. ID: {Id}, DTO ID: {DtoId}", id, dto.Id);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = id != dto.Id ? "ID in URL does not match DTO ID." : "Invalid invoice data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }
                _logger.LogInformation("Updating invoice {InvoiceId} for company {CompanyId} by user {UserId}", id, operationContext.CompanyId, operationContext.UserId);

                var command = UpdateInvoiceCommand.FromDto(dto);
                command = command with { Context = CurrentContext };

                // Send command through MediatR
                var result = await _mediator.Send(command);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice update failed: {ErrorMessage}", result.ErrorMessage);

                    return BadRequest(ApiResponse<object>.Error(
                        result.ErrorMessage,
                        400,
                        new ErrorDetails { Code = "UPDATE_FAILED", Description = result.ErrorMessage }));
                }

                return Ok(ApiResponse<InvoiceResponseDto>.Ok(result.Data, "Invoice updated successfully"));

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice update.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while updating the invoice in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId} for company {CompanyId}", id, operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while updating the invoice."
                });
            }
        }

        /// <summary>
        /// Deletes an invoice by ID (soft delete).
        /// </summary>
        /// <param name="id">The ID of the invoice to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">Invoice deleted successfully.</response>
        /// <response code="400">Invoice not found or deletion failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var traceId = HttpContext.TraceIdentifier;
            try
            {
                _logger.LogInformation("Deleting invoice {InvoiceId} for company {CompanyId} by user {UserId}",
          id, CurrentContext.CompanyId, CurrentContext.UserId);

                // Create command with context
                var command = DeleteInvoiceCommand.Create(id);

                // Send command through MediatR
                var result = await _mediator.Send(command);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice deletion failed: {ErrorMessage}. TraceId: {TraceId}",
                        result.ErrorMessage, traceId);

                    return BadRequest(ApiResponse<object>.Error(
                        result.ErrorMessage,
                        400,
                        new ErrorDetails { Code = "DELETION_FAILED", Description = result.ErrorMessage }));
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice deletion.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting invoice {InvoiceId}. TraceId: {TraceId}", id, traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while deleting the invoice from the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId}. TraceId: {TraceId}", id, traceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while deleting the invoice."
                });
            }
        }

        /// <summary>
        /// Retrieves an invoice by ID.
        /// </summary>
        /// <param name="id">The ID of the invoice to retrieve.</param>
        /// <returns>The <see cref="InvoiceResponseDto"/> for the specified invoice.</returns>
        /// <response code="200">Invoice retrieved successfully.</response>
        /// <response code="404">Invoice not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var traceId = HttpContext.TraceIdentifier;
            try
            {
                _logger.LogInformation("Retrieving invoice {InvoiceId} for company {CompanyId}, UserId: {UserId}",
            id, CurrentContext.CompanyId, CurrentContext.UserId);

                // Create query with context
                var query = GetInvoiceByIdQuery.FromId(id);
                query.Context = CurrentContext;

                // Send query through MediatR
                var result = await _mediator.Send(query);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice retrieval failed: {ErrorMessage}. TraceId: {TraceId}",
                        result.ErrorMessage, traceId);

                    return NotFound(ApiResponse<object>.Error(
                        result.ErrorMessage,
                        404,
                        new ErrorDetails { Code = "NOT_FOUND", Description = result.ErrorMessage }));
                }

                return Ok(ApiResponse<InvoiceResponseDto>.Ok(result.Data, "Invoice retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice retrieval. TraceId: {TraceId}", traceId);
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice {InvoiceId}. TraceId: {TraceId}", id, traceId);

                return StatusCode(500, ApiResponse<object>.Error(
                    "An unexpected error occurred while retrieving the invoice.",
                    500,
                    new ErrorDetails { Code = "INTERNAL_ERROR", Description = "Please try again later" }));
            }
        }

        /// <summary>
        /// Retrieves a paged list of invoices for the authenticated user's company.
        /// </summary>
        /// <param name="filter">The filter parameters for retrieving invoices.</param>
        /// <returns>A <see cref="PaginatedResult{InvoiceResponseDto}"/> containing the invoices.</returns>
        /// <response code="200">Invoices retrieved successfully.</response>
        /// <response code="400">Invalid filter parameters or retrieval failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaged([FromQuery] InvoiceFilterRequestDto filter)
        {
            var traceId = HttpContext.TraceIdentifier;
            try
            {
                // Validate query parameters
                if (!filter.IsValid())
                {
                    _logger.LogWarning("Invalid filter parameters: pageNumber={PageNumber}, pageSize={PageSize}",
                        filter.PageNumber, filter.PageSize);

                    return BadRequest(ApiResponse<object>.Error(
                        "Invalid filter parameters. Page number and page size must be greater than 0, and minAmount must not exceed maxAmount.",
                        400,
                        new ErrorDetails
                        {
                            Code = "INVALID_PARAMETERS",
                            Description = "Validation failed for the provided query parameters"
                        }));
                }

                // Extension method to map filter to query
                var query = filter.ToQuery(CurrentContext);

                // Send query through MediatR
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during paged invoice retrieval. TraceId: {TraceId}", traceId);

                return Unauthorized(ApiResponse<object>.Error(
                    "Unauthorized access to invoices",
                    401,
                    new ErrorDetails { Code = "UNAUTHORIZED", Description = ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged invoices. TraceId: {TraceId}", traceId);

                return StatusCode(500, ApiResponse<object>.Error(
                    "An unexpected error occurred while retrieving invoices.",
                    500,
                    new ErrorDetails { Code = "INTERNAL_ERROR", Description = "Please try again later" }));
            }
        }

        /// <summary>
        /// Duplicates an existing invoice
        /// </summary>
        /// <param name="id">The ID of the invoice to duplicate</param>
        /// <returns>The newly created duplicate invoice</returns>
        /// <response code="201">Invoice duplicated successfully.</response>
        /// <response code="400">Invalid request or invoice not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("{id}/duplicate")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Duplicate(int id)
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                _logger.LogInformation("Duplicating invoice {InvoiceId} for company {CompanyId} by user {UserId}",
            id, CurrentContext.CompanyId, CurrentContext.UserId);

                // Create command with context
                var command = DuplicateInvoiceCommand.FromId(id);
                command.Context = CurrentContext;

                // Send command through MediatR
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice duplication failed: {ErrorMessage}. TraceId: {TraceId}",
                        result.ErrorMessage, traceId);

                    return BadRequest(ApiResponse<object>.Error(
                        result.ErrorMessage,
                        400,
                        new ErrorDetails { Code = "DUPLICATION_FAILED", Description = result.ErrorMessage }));
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id },
                    ApiResponse<InvoiceResponseDto>.Ok(result.Data, "Invoice duplicated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice duplication. TraceId: {TraceId}", traceId);

                return Unauthorized(ApiResponse<object>.Error(
                    "Unauthorized access to duplicate invoice",
                    401,
                    new ErrorDetails { Code = "UNAUTHORIZED", Description = ex.Message }));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error duplicating invoice {InvoiceId}. TraceId: {TraceId}", id, traceId);

                return StatusCode(500, ApiResponse<object>.Error(
                    "An error occurred while duplicating the invoice in the database.",
                    500,
                    new ErrorDetails { Code = "DATABASE_ERROR", Description = "Please try again later" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating invoice {InvoiceId}. TraceId: {TraceId}", id, traceId);

                return StatusCode(500, ApiResponse<object>.Error(
                    "An unexpected error occurred while duplicating the invoice.",
                    500,
                    new ErrorDetails { Code = "INTERNAL_ERROR", Description = "Please try again later" }));
            }
        }

        /// <summary>
        /// Retrieves the next available invoice number for the authenticated user's company.
        /// </summary>
        /// <returns>The next invoice number as a string.</returns>
        /// <response code="200">Next invoice number retrieved successfully.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error or retrieval failed.</response>
        [HttpGet("next-invoice-number")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<string>> GetNextInvoiceNumber()
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                _logger.LogInformation("Retrieving next invoice number for company {CompanyId}, UserId: {UserId}",
                    CurrentContext.CompanyId, CurrentContext.UserId);

                // Create query with context
                var query = GetNextInvoiceNumberQuery.Create();
                query.Context = CurrentContext;

                // Send query through MediatR
                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Next invoice number retrieval failed: {ErrorMessage}. TraceId: {TraceId}",
                        result.ErrorMessage, traceId);

                    return BadRequest(ApiResponse<object>.Error(
                        result.ErrorMessage,
                        400,
                        new ErrorDetails { Code = "RETRIEVAL_FAILED", Description = result.ErrorMessage }));
                }

                return Ok(ApiResponse<string>.Ok(result.Data, "Next invoice number retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during next invoice number retrieval. TraceId: {TraceId}", traceId);

                return Unauthorized(ApiResponse<object>.Error(
                    "Unauthorized access to retrieve next invoice number",
                    401,
                    new ErrorDetails { Code = "UNAUTHORIZED", Description = ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving next invoice number for company {CompanyId}. TraceId: {TraceId}",
                    CurrentContext.CompanyId, traceId);

                return StatusCode(500, ApiResponse<object>.Error(
                    "An unexpected error occurred while retrieving the next invoice number.",
                    500,
                    new ErrorDetails { Code = "INTERNAL_ERROR", Description = "Please try again later" }));
            }
        }

        /// <summary>
        /// Sends an invoice to its recipient.
        /// </summary>
        /// <param name="id">The ID of the invoice to send.</param>
        /// <returns>A success message if the invoice was sent.</returns>
        /// <response code="200">Invoice sent successfully.</response>
        /// <response code="400">Invoice not found or sending failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("{id}/send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendInvoice(int id, [FromBody] EmailDataDto emailData)
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                _logger.LogInformation("Sending invoice {InvoiceId} for company {CompanyId} by user {UserId}",
                    id, CurrentContext.CompanyId, CurrentContext.UserId);

                // Validate email data
                if (emailData.To == null || emailData.To.Count == 0 || emailData.To.All(e => string.IsNullOrWhiteSpace(e)))
                {
                    return BadRequest(ApiResponse<object>.Error(
                        "At least one valid 'To' email address is required.",
                        400,
                        new ErrorDetails { Code = "INVALID_EMAIL", Description = "No valid recipients provided." }));
                }

                // Create command with context
                var command = SendInvoiceCommand.Create(id, emailData);
                command.Context = CurrentContext;

                // Send command through MediatR
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice sending failed: {ErrorMessage}. TraceId: {TraceId}",
                        result.ErrorMessage, traceId);

                    return BadRequest(ApiResponse<object>.Error(
                        result.ErrorMessage,
                        400,
                        new ErrorDetails { Code = "SEND_FAILED", Description = result.ErrorMessage }));
                }

                return Ok(ApiResponse<object>.Ok(new { Message = "Invoice sent successfully." }, "Invoice sent successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice sending. TraceId: {TraceId}", traceId);

                return Unauthorized(ApiResponse<object>.Error(
                    "Unauthorized access to send invoice",
                    401,
                    new ErrorDetails { Code = "UNAUTHORIZED", Description = ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice {InvoiceId}. TraceId: {TraceId}", id, traceId);

                return StatusCode(500, ApiResponse<object>.Error(
                    "An unexpected error occurred while sending the invoice.",
                    500,
                    new ErrorDetails { Code = "INTERNAL_ERROR", Description = "Please try again later" }));
            }
        }

        /// <summary>
        /// Retrieves various statistics related to invoices for the authenticated user's company.
        /// </summary>
        /// <returns>Invoice statistics.</returns>
        /// <response code="200">Invoice stats retrieved successfully.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error or retrieval failed.</response>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStats()
        {
            var traceId = HttpContext.TraceIdentifier;
            try
            {
                var context = CurrentContext;

                if (context == null)
                {
                    _logger.LogWarning("Unable to retrieve operation context - user not authenticated. TraceId: {TraceId}", traceId);

                    return Unauthorized(ApiResponse<object>.Error(
                        "Unable to determine user context. Please ensure you are authenticated.",
                        401,
                        new ErrorDetails { Code = "UNAUTHORIZED", Description = "User context not found" }));
                }

                // Log the context for debugging
                _logger.LogInformation("Retrieving invoice stats - Context: {Context}, TraceId: {TraceId}",
                    context.ToString(), traceId);

                // Validate based on role
                if (!context.IsSuperAdmin && !context.CompanyId.HasValue)
                {
                    _logger.LogWarning("Stats access denied: Non-super admin without company ID. UserId: {UserId}, Roles: {Roles}, TraceId: {TraceId}",
                        context.UserId, string.Join(",", context.Roles), traceId);

                    return Unauthorized(ApiResponse<object>.Error(
                        "Company ID is required for non-super admin users.",
                        401,
                        new ErrorDetails { Code = "UNAUTHORIZED", Description = "Company ID missing in user context" }));
                }

                // Create query with context
                var query = GetInvoiceStatsQuery.Create();
                query.Context = context;

                // Send query through MediatR
                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Stats retrieval failed: {ErrorMessage}. TraceId: {TraceId}",
                        result.ErrorMessage, traceId);

                    return BadRequest(ApiResponse<object>.Error(
                        result.ErrorMessage,
                        400,
                        new ErrorDetails { Code = "STATS_FAILED", Description = result.ErrorMessage }));
                }

                return Ok(ApiResponse<InvoiceStatsDto>.Ok(result.Data, "Statistics retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during stats retrieval. TraceId: {TraceId}", traceId);

                return Unauthorized(ApiResponse<object>.Error(
                    "Unauthorized access to retrieve statistics",
                    401,
                    new ErrorDetails { Code = "UNAUTHORIZED", Description = ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice stats. TraceId: {TraceId}", traceId);

                return StatusCode(500, ApiResponse<object>.Error(
                    "An unexpected error occurred while retrieving statistics.",
                    500,
                    new ErrorDetails { Code = "INTERNAL_ERROR", Description = "Please try again later" }));
            }
        }

        [HttpGet("template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetImportTemplate()
        {
            try
            {
                _logger.LogInformation("Generating import template for company {CompanyId}", CurrentContext.CompanyId);
                var templateBytes = _excelService.GenerateImportTemplate();
                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "invoice-import-template.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating import template for company {CompanyId}", CurrentContext.CompanyId);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Failed to generate template."
                });
            }
        }

        /// <summary>
        /// Generates PDF for a single invoice
        /// </summary>
        [HttpGet("{id}/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoicePdf(int id)
        {
            var traceId = HttpContext.TraceIdentifier;

            try
            {
                _logger.LogInformation("Generating PDF for invoice {InvoiceId} for company {CompanyId}, UserId: {UserId}",
                    id, CurrentContext.CompanyId, CurrentContext.UserId);

                // Create query with context
                var query = GetInvoicePdfQuery.FromId(id);
                query.Context = CurrentContext;

                // Send query through MediatR
                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice PDF generation failed: {ErrorMessage}. TraceId: {TraceId}",
                        result.ErrorMessage, traceId);

                    return NotFound(ApiResponse<object>.Error(
                        result.ErrorMessage,
                        404,
                        new ErrorDetails { Code = "NOT_FOUND", Description = result.ErrorMessage }));
                }

                var pdfStream = result.Data.PdfStream;

                if (pdfStream == null || !pdfStream.CanRead || pdfStream.Length == 0)
                {
                    _logger.LogError("PDF stream is null or invalid for invoice {InvoiceId}. TraceId: {TraceId}", id, traceId);

                    return StatusCode(500, ApiResponse<object>.Error(
                        "The generated PDF stream is invalid.",
                        500,
                        new ErrorDetails { Code = "PDF_ERROR", Description = "Unable to generate PDF" }));
                }

                // Convert to byte array
                byte[] pdfBytes = pdfStream.ToArray();
                pdfStream.Position = 0;

                _logger.LogInformation("PDF generated successfully for invoice {InvoiceNumber} (ID: {InvoiceId})",
                    result.Data.InvoiceNumber, id);

                return File(pdfBytes, "application/pdf", $"invoice_{result.Data.InvoiceNumber}.pdf");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice PDF generation. TraceId: {TraceId}", traceId);

                return Unauthorized(ApiResponse<object>.Error(
                    "Unauthorized access to generate invoice PDF",
                    401,
                    new ErrorDetails { Code = "UNAUTHORIZED", Description = ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for invoice {InvoiceId}. TraceId: {TraceId}", id, traceId);

                return StatusCode(500, ApiResponse<object>.Error(
                    "An unexpected error occurred while generating the invoice PDF.",
                    500,
                    new ErrorDetails { Code = "INTERNAL_ERROR", Description = "Please try again later" }));
            }
        }

        /// <summary>
        /// Exports multiple invoices to Excel
        /// </summary>
        //[HttpGet("export/excel")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> ExportExcel([FromQuery] InvoiceFilterRequestDto invoiceFilterRequestDto)
        //{
        //    var operationContext = CurrentContext;
        //    try
        //    {
        //        _logger.LogInformation("Exporting invoices to Excel for company {CompanyId}", operationContext.CompanyId);

        //        var result = await _excelService.ExportInvoicesExcelAsync(operationContext, invoiceFilterRequestDto);
        //        if (!result.IsSuccess)
        //        {
        //            _logger.LogWarning("Excel export failed: {ErrorMessage}", result.ErrorMessage);
        //            return BadRequest(new ProblemDetails
        //            {
        //                Title = "Excel Export Failed",
        //                Detail = result.ErrorMessage,
        //                Status = StatusCodes.Status400BadRequest
        //            });
        //        }

        //        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"invoices_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        _logger.LogWarning(ex, "Unauthorized access during Excel export.");
        //        return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error exporting invoices to Excel for company {CompanyId}", operationContext.CompanyId);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        //        {
        //            Title = "Internal Server Error",
        //            Detail = "An unexpected error occurred while exporting invoices to Excel."
        //        });
        //    }
        //}

        /// <summary>
        /// Exports multiple invoices to PDF
        /// </summary>
        [HttpGet("export/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportPdf([FromQuery] GetPagedInvoicesQuery invoiceFilterRequestDto)
        {
            var operationContext = CurrentContext;
            try
            {
                _logger.LogInformation("Exporting invoices to PDF for company {CompanyId}", operationContext.CompanyId);

                var result = await _pdfService.ExportInvoicesPdfAsync(operationContext, invoiceFilterRequestDto);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("PDF export failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "PDF Export Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return File(result.Data, "application/pdf", $"invoices_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during PDF export.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoices to PDF for company {CompanyId}", operationContext.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while exporting invoices to PDF."
                });
            }
        }
    }
}