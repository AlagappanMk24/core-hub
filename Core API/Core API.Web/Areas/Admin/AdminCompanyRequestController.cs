using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Authentication.Request.CompanyRequest;
using Core_API.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Core_API.Web.Areas.Admin
{
    /// <summary>
    /// Controller for managing company registration requests by administrators.
    /// </summary>
    [Route("api/admin/company-requests")]
    [ApiController]
    [Authorize(Roles = "Admin,Super Admin")]
    [Produces("application/json")]
    public class AdminCompanyRequestController(
        ICompanyRequestService companyRequestService,
        ILogger<AdminCompanyRequestController> logger) : ControllerBase
    {
        /// <param name="companyRequestService">Service for handling company request operations.</param>
        /// <param name="logger">Logger for recording events and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
        private readonly ILogger<AdminCompanyRequestController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ICompanyRequestService _companyRequestService = companyRequestService ?? throw new ArgumentNullException(nameof(companyRequestService));

        /// <summary>
        /// Retrieves a paginated list of company registration requests with optional filtering.
        /// </summary>
        /// <param name="page">The page number to retrieve (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10, max: 100).</param>
        /// <param name="status">Filter by request status (Pending, Approved, Rejected).</param>
        /// <param name="search">Search term to filter by name, email, or company name.</param>
        /// <returns>A paginated list of company requests with summary statistics.</returns>
        /// <response code="200">Requests retrieved successfully.</response>
        /// <response code="400">Invalid query parameters.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have required role (Admin/Super Admin).</response>
        /// <response code="500">Internal server error during retrieval.</response>
        [HttpGet]
        [ProducesResponseType(typeof(CompanyRequestListResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CompanyRequestListResponseDto>> GetRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? search = null)
        {
            // Validate pagination parameters
            if (page < 1)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Parameter",
                    Detail = "Page must be greater than or equal to 1.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Parameter",
                    Detail = "Page size must be between 1 and 100.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Validate status if provided
            if (!string.IsNullOrEmpty(status) &&
                !Enum.TryParse<CompanyRequestStatus>(status, true, out _))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Parameter",
                    Detail = "Status must be one of: Pending, Approved, Rejected.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            try
            {
                _logger.LogInformation(
                    "Admin retrieving company requests - Page: {Page}, PageSize: {PageSize}, Status: {Status}, Search: {Search}",
                    page, pageSize, status ?? "All", search ?? "None");

                var result = await _companyRequestService.GetPagedRequestsAsync(page, pageSize, status, search);

                _logger.LogInformation(
                    "Successfully retrieved {Count} company requests. Total: {Total}, Pending: {Pending}, Approved: {Approved}, Rejected: {Rejected}",
                    result.Requests.Count, result.TotalCount, result.PendingCount, result.ApprovedCount, result.RejectedCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company requests. Page: {Page}, PageSize: {PageSize}, Status: {Status}, Search: {Search}",
                    page, pageSize, status ?? "All", search ?? "None");

                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving company requests.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Retrieves a specific company registration request by its ID.
        /// </summary>
        /// <param name="id">The ID of the company request to retrieve.</param>
        /// <returns>The company request details.</returns>
        /// <response code="200">Request retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have required role (Admin/Super Admin).</response>
        /// <response code="404">Request with the specified ID not found.</response>
        /// <response code="500">Internal server error during retrieval.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CompanyRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CompanyRequestResponseDto>> GetRequest([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin retrieving company request with ID: {RequestId}", id);

                var request = await _companyRequestService.GetRequestByIdAsync(id);

                if (request == null)
                {
                    _logger.LogWarning("Company request with ID {RequestId} not found", id);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Request Not Found",
                        Detail = $"Company request with ID {id} was not found.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Successfully retrieved company request with ID: {RequestId}", id);
                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company request with ID: {RequestId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving the company request.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Approves a pending company registration request.
        /// </summary>
        /// <param name="id">The ID of the company request to approve.</param>
        /// <returns>Approval result with created company and user IDs.</returns>
        /// <response code="200">Request approved successfully.</response>
        /// <response code="400">Request cannot be approved (not pending or invalid state).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have required role (Admin/Super Admin).</response>
        /// <response code="404">Request with the specified ID not found.</response>
        /// <response code="409">Conflict during approval (e.g., company already exists).</response>
        /// <response code="500">Internal server error during approval.</response>
        [HttpPost("{id:int}/approve")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveRequest([FromRoute] int id)
        {
            //var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminId = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                _logger.LogWarning("Admin ID claim not found in token for approve request {RequestId}", id);
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Admin ID claim is missing from the token.",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            try
            {
                _logger.LogInformation("Admin {AdminId} approving company request {RequestId}", adminId, id);

                var result = await _companyRequestService.ApproveRequestAsync(id, adminId);

                if (!result.Success)
                {
                    _logger.LogWarning("Company request {RequestId} approval failed: {Message}", id, result.Message);

                    return result.Message.Contains("not found")
                        ? NotFound(new ProblemDetails
                        {
                            Title = "Request Not Found",
                            Detail = result.Message,
                            Status = StatusCodes.Status404NotFound
                        })
                        : result.Message.Contains("already")
                            ? Conflict(new ProblemDetails
                            {
                                Title = "Conflict",
                                Detail = result.Message,
                                Status = StatusCodes.Status409Conflict
                            })
                            : BadRequest(new ProblemDetails
                            {
                                Title = "Approval Failed",
                                Detail = result.Message,
                                Status = StatusCodes.Status400BadRequest
                            });
                }

                _logger.LogInformation(
                    "Company request {RequestId} approved successfully by admin {AdminId}. Company ID: {CompanyId}, User ID: {UserId}",
                    id, adminId, result.CompanyId, result.UserId);

                return Ok(new
                {
                    message = result.Message,
                    companyId = result.CompanyId,
                    userId = result.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving company request {RequestId} by admin {AdminId}", id, adminId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while approving the company request.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Rejects a pending company registration request with a reason.
        /// </summary>
        /// <param name="id">The ID of the company request to reject.</param>
        /// <param name="dto">The rejection details containing the reason for rejection.</param>
        /// <returns>Rejection result with status message.</returns>
        /// <response code="200">Request rejected successfully.</response>
        /// <response code="400">Invalid rejection data or request cannot be rejected.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have required role (Admin/Super Admin).</response>
        /// <response code="404">Request with the specified ID not found.</response>
        /// <response code="500">Internal server error during rejection.</response>
        [HttpPost("{id:int}/reject")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectRequest([FromRoute] int id, [FromBody] RejectCompanyRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for reject request {RequestId}: {Errors}",
                    id, JsonSerializer.Serialize(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Title = "Validation Error",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "Rejection reason is required.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (dto.Reason.Length > 500)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "Rejection reason cannot exceed 500 characters.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                _logger.LogWarning("Admin ID claim not found in token for reject request {RequestId}", id);
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Admin ID claim is missing from the token.",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            try
            {
                _logger.LogInformation("Admin {AdminId} rejecting company request {RequestId}. Reason: {Reason}",
                    adminId, id, dto.Reason);

                var result = await _companyRequestService.RejectRequestAsync(id, dto.Reason, adminId);

                if (!result.Success)
                {
                    _logger.LogWarning("Company request {RequestId} rejection failed: {Message}", id, result.Message);

                    return result.Message.Contains("not found")
                        ? NotFound(new ProblemDetails
                        {
                            Title = "Request Not Found",
                            Detail = result.Message,
                            Status = StatusCodes.Status404NotFound
                        })
                        : BadRequest(new ProblemDetails
                        {
                            Title = "Rejection Failed",
                            Detail = result.Message,
                            Status = StatusCodes.Status400BadRequest
                        });
                }

                _logger.LogInformation("Company request {RequestId} rejected successfully by admin {AdminId}", id, adminId);
                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting company request {RequestId} by admin {AdminId}", id, adminId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while rejecting the company request.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}