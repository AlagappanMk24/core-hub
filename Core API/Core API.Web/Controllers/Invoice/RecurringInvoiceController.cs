using Core_API.Application.Contracts.Services.RecurringInvoices;
using Core_API.Application.DTOs.RecurringInvoice.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Web.Controllers.Invoice
{
    [Route("api/recurring-invoices")]
    [ApiController]
    [Authorize(Roles = "Admin, User")]
    public class RecurringInvoiceController(
        IRecurringInvoiceService recurringInvoiceService,
        ILogger<RecurringInvoiceController> logger) : BaseApiController
    {
        private readonly IRecurringInvoiceService _recurringInvoiceService = recurringInvoiceService ?? throw new ArgumentNullException(nameof(recurringInvoiceService));
        private readonly ILogger<RecurringInvoiceController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        #region CRUD Operations

        /// <summary>
        /// Creates a new recurring invoice template
        /// </summary>
        [HttpPost]   // POST api/recurring-invoices
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateRecurringInvoiceDto dto)
        {
            var context = CurrentContext;
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for recurring invoice creation.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid recurring invoice data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                _logger.LogInformation("Creating recurring invoice template for company {CompanyId} by user {UserId}",
                    context.CompanyId, context.UserId);

                var result = await _recurringInvoiceService.CreateAsync(dto, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Recurring invoice creation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Recurring Invoice Creation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during recurring invoice creation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating recurring invoice for company {CompanyId}", context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while creating the recurring invoice in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring invoice for company {CompanyId}", context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while creating the recurring invoice."
                });
            }
        }

        /// <summary>
        /// Updates an existing recurring invoice template
        /// </summary>
        [HttpPut("{id}")]  // PUT api/recurring-invoices/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRecurringInvoiceDto dto)
        {
            var context = CurrentContext;
            try
            {
                if (!ModelState.IsValid || id != dto.Id)
                {
                    _logger.LogWarning("Invalid model state or ID mismatch for recurring invoice update. ID: {Id}, DTO ID: {DtoId}", id, dto.Id);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = id != dto.Id ? "ID in URL does not match DTO ID." : "Invalid recurring invoice data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                _logger.LogInformation("Updating recurring invoice {RecurringInvoiceId} for company {CompanyId} by user {UserId}",
                    id, context.CompanyId, context.UserId);

                var result = await _recurringInvoiceService.UpdateAsync(dto, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Recurring invoice update failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Recurring Invoice Update Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during recurring invoice update.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while updating the recurring invoice in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while updating the recurring invoice."
                });
            }
        }

        /// <summary>
        /// Deletes a recurring invoice template (soft delete)
        /// </summary>
        [HttpDelete("{id}")]    // DELETE api/recurring-invoices/5
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Deleting recurring invoice {RecurringInvoiceId} for company {CompanyId} by user {UserId}",
                    id, context.CompanyId, context.UserId);

                var result = await _recurringInvoiceService.DeleteAsync(id, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Recurring invoice deletion failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Recurring Invoice Deletion Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during recurring invoice deletion.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while deleting the recurring invoice from the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while deleting the recurring invoice."
                });
            }
        }

        /// <summary>
        /// Retrieves a recurring invoice template by ID
        /// </summary>
        [HttpGet("{id}")]  // GET api/recurring-invoices/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Retrieving recurring invoice {RecurringInvoiceId} for company {CompanyId}",
                    id, context.CompanyId);

                var result = await _recurringInvoiceService.GetByIdAsync(id, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Recurring invoice retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Recurring Invoice Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during recurring invoice retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving the recurring invoice."
                });
            }
        }

        /// <summary>
        /// Retrieves a paged list of recurring invoice templates
        /// </summary>
        [HttpGet] // GET api/recurring-invoices
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaged([FromQuery] RecurringInvoiceFilterDto filter)
        {
            var context = CurrentContext;
            try
            {
                if (!filter.IsValid())
                {
                    _logger.LogWarning("Invalid filter parameters: pageNumber={PageNumber}, pageSize={PageSize}",
                        filter.PageNumber, filter.PageSize);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Filter Parameters",
                        Detail = "Page number and page size must be greater than 0, and page size cannot exceed 100.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Apply customer filter if user is a customer
                if (context.CustomerId.HasValue)
                {
                    filter.CustomerId = context.CustomerId.Value;
                }

                _logger.LogInformation("Retrieving paged recurring invoices for company {CompanyId}, page {PageNumber}, size {PageSize}",
                    context.CompanyId, filter.PageNumber, filter.PageSize);

                var result = await _recurringInvoiceService.GetPagedAsync(context, filter);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Paged recurring invoices retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Recurring Invoice Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during paged recurring invoice retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged recurring invoices for company {CompanyId}", context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving recurring invoices."
                });
            }
        }

        #endregion

        #region Status Management

        /// <summary>
        /// Activates a draft recurring invoice template
        /// </summary>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Activate(int id)
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Activating recurring invoice {RecurringInvoiceId} for company {CompanyId} by user {UserId}",
                    id, context.CompanyId, context.UserId);

                var result = await _recurringInvoiceService.ActivateAsync(id, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Recurring invoice activation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Recurring Invoice Activation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new { Message = "Recurring invoice activated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during recurring invoice activation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while activating the recurring invoice."
                });
            }
        }

        /// <summary>
        /// Pauses an active recurring invoice template
        /// </summary>
        [HttpPost("{id}/pause")]     // POST api/recurring-invoices/5/pause
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Pause(int id)
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Pausing recurring invoice {RecurringInvoiceId} for company {CompanyId} by user {UserId}",
                    id, context.CompanyId, context.UserId);

                var result = await _recurringInvoiceService.PauseAsync(id, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Recurring invoice pause failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Recurring Invoice Pause Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new { Message = "Recurring invoice paused successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during recurring invoice pause.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while pausing the recurring invoice."
                });
            }
        }

        /// <summary>
        /// Resumes a paused recurring invoice template
        /// </summary>
        [HttpPost("{id}/resume")]    // POST api/recurring-invoices/5/resume
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Resume(int id)
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Resuming recurring invoice {RecurringInvoiceId} for company {CompanyId} by user {UserId}",
                    id, context.CompanyId, context.UserId);

                var result = await _recurringInvoiceService.ResumeAsync(id, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Recurring invoice resume failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Recurring Invoice Resume Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new { Message = "Recurring invoice resumed successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during recurring invoice resume.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while resuming the recurring invoice."
                });
            }
        }

        /// <summary>
        /// Cancels a recurring invoice template
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin")]    // POST api/recurring-invoices/5/cancel
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cancel(int id)
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Cancelling recurring invoice {RecurringInvoiceId} for company {CompanyId} by user {UserId}",
                    id, context.CompanyId, context.UserId);

                var result = await _recurringInvoiceService.CancelAsync(id, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Recurring invoice cancellation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Recurring Invoice Cancellation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new { Message = "Recurring invoice cancelled successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during recurring invoice cancellation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring invoice {RecurringInvoiceId} for company {CompanyId}", id, context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while cancelling the recurring invoice."
                });
            }
        }

        #endregion

        #region Generation Operations

        /// <summary>
        /// Manually generates an invoice from a recurring template
        /// </summary>
        [HttpPost("{id:int}/generate-now")]   // POST api/recurring-invoices/5/generate-now
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateInvoiceManually([FromBody] GenerateManualDto dto)
        {
            var context = CurrentContext;
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for manual generation.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid generation parameters.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                _logger.LogInformation("Manually generating invoice from recurring template {RecurringInvoiceId} for company {CompanyId}",
                    dto.RecurringInvoiceId, context.CompanyId);

                var result = await _recurringInvoiceService.GenerateInvoiceManuallyAsync(dto, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Manual invoice generation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Generation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during manual invoice generation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error manually generating invoice from recurring template {RecurringInvoiceId}", dto.RecurringInvoiceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while generating the invoice."
                });
            }
        }

        /// <summary>
        /// Updates the next invoice date for a recurring template
        /// </summary>
        [HttpPatch("{id}/next-date")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateNextInvoiceDate(int id, [FromBody] UpdateNextDateRequest request)
        {
            var context = CurrentContext;
            try
            {
                if (request.NextDate < DateTime.UtcNow.Date)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Date",
                        Detail = "Next invoice date cannot be in the past.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                _logger.LogInformation("Updating next invoice date for recurring invoice {RecurringInvoiceId} to {NextDate}",
                    id, request.NextDate);

                var result = await _recurringInvoiceService.UpdateNextInvoiceDateAsync(id, request.NextDate, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Next invoice date update failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Update Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(new { Message = "Next invoice date updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during next date update.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating next invoice date for recurring invoice {RecurringInvoiceId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while updating next invoice date."
                });
            }
        }

        #endregion

        #region Customer-Specific Operations

        /// <summary>
        /// Retrieves recurring invoices for a specific customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin, User, Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerRecurringInvoices(int customerId)
        {
            var context = CurrentContext;
            try
            {
                // Customers can only view their own recurring invoices
                if (User.IsInRole("Customer"))
                {
                    if (!context.CustomerId.HasValue || context.CustomerId.Value != customerId)
                    {
                        return Forbid();
                    }
                }

                _logger.LogInformation("Retrieving recurring invoices for customer {CustomerId}, company {CompanyId}",
                    customerId, context.CompanyId);

                var result = await _recurringInvoiceService.GetCustomerRecurringInvoicesAsync(customerId, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Customer recurring invoices retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Customer Recurring Invoices Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during customer recurring invoices retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring invoices for customer {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving customer recurring invoices."
                });
            }
        }

        #endregion

        #region Statistics and Dashboard

        /// <summary>
        /// Retrieves status counts for recurring invoices
        /// </summary>
        [HttpGet("status-counts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStatusCounts()
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Retrieving recurring invoice status counts for company {CompanyId}", context.CompanyId);

                var result = await _recurringInvoiceService.GetStatusCountsAsync(context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Status counts retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Status Counts Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during status counts retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving status counts for company {CompanyId}", context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving status counts."
                });
            }
        }

        /// <summary>
        /// Retrieves statistics for recurring invoices dashboard
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStats()
        {
            var context = CurrentContext;
            try
            {
                _logger.LogInformation("Retrieving recurring invoice stats for company {CompanyId}", context.CompanyId);

                var result = await _recurringInvoiceService.GetStatsAsync(context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Stats retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Stats Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during stats retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats for company {CompanyId}", context.CompanyId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving stats."
                });
            }
        }

        #endregion
    }
}