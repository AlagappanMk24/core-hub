using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Customer.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Core_API.Web.Controllers
{
    /// <summary>
    /// Controller for managing customer-related operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CustomerController"/> class.
    /// </remarks>
    /// <param name="customerService">The customer service for business logic.</param>
    /// <param name="logger">The logger for logging controller actions.</param>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(ICustomerService customerService, ILogger<CustomerController> logger) : ControllerBase
    {
        private readonly ICustomerService _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        private readonly ILogger<CustomerController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Gets the company ID from the JWT token claims.
        /// </summary>
        /// <returns>The company ID.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when company ID is not found or invalid in the token.</exception>
        private int GetCompanyId()
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out var companyId))
            {
                throw new UnauthorizedAccessException("Company ID not found or invalid in token.");
            }
            return companyId;
        }

        /// <summary>
        /// Gets the user ID from the JWT token claims.
        /// </summary>
        /// <returns>The user ID.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user ID is not found in the token.</exception>
        private string GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }

        /// <summary>
        /// Creates a new customer for the authenticated user's company.
        /// </summary>
        /// <param name="dto">The customer data to create.</param>
        /// <returns>A <see cref="CustomerResponseDto"/> representing the created customer.</returns>
        /// <response code="201">Customer created successfully.</response>
        /// <response code="400">Invalid request data or customer creation failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for customer creation.");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid customer data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                var companyId = GetCompanyId();
                var userId = GetUserId();
                _logger.LogInformation("Creating customer for company {CompanyId} by user {UserId}", companyId, userId);

                var result = await _customerService.CreateAsync(dto, companyId, userId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Customer creation failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Customer Creation Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during customer creation.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer for company {CompanyId}", GetCompanyId());
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while creating the customer."
                });
            }
        }

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="id">The ID of the customer to update.</param>
        /// <param name="dto">The updated customer data.</param>
        /// <returns>The updated <see cref="CustomerResponseDto"/>.</returns>
        /// <response code="200">Customer updated successfully.</response>
        /// <response code="400">Invalid request data or customer not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid || id != dto.Id)
                {
                    _logger.LogWarning("Invalid model state or ID mismatch for customer update. ID: {Id}, DTO ID: {DtoId}", id, dto.Id);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = id != dto.Id ? "ID in URL does not match DTO ID." : "Invalid customer data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                var companyId = GetCompanyId();
                var userId = GetUserId();
                _logger.LogInformation("Updating customer {CustomerId} for company {CompanyId} by user {UserId}", id, companyId, userId);

                var result = await _customerService.UpdateAsync(dto, companyId, userId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Customer update failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Customer Update Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during customer update.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating customer {CustomerId} for company {CompanyId}", id, GetCompanyId());
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while updating the customer in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId} for company {CompanyId}", id, GetCompanyId());
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while updating the customer."
                });
            }
        }

        /// <summary>
        /// Deletes a customer by ID (soft delete).
        /// </summary>
        /// <param name="id">The ID of the customer to delete.</param>
        /// <returns>No content if successful.</returns>
        /// <response code="204">Customer deleted successfully.</response>
        /// <response code="400">Customer not found or deletion failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                var userId = GetUserId();
                _logger.LogInformation("Deleting customer {CustomerId} for company {CompanyId} by user {UserId}", id, companyId, userId);

                var result = await _customerService.DeleteAsync(id, companyId, userId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Customer deletion failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Customer Deletion Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during customer deletion.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting customer {CustomerId} for company {CompanyId}", id, GetCompanyId());
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while deleting the customer from the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId} for company {CompanyId}", id, GetCompanyId());
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while deleting the customer."
                });
            }
        }

        /// <summary>
        /// Retrieves a customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to retrieve.</param>
        /// <returns>The <see cref="CustomerResponseDto"/> for the specified customer.</returns>
        /// <response code="200">Customer retrieved successfully.</response>
        /// <response code="404">Customer not found.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var companyId = GetCompanyId();
                _logger.LogInformation("Retrieving customer {CustomerId} for company {CompanyId}", id, companyId);

                var result = await _customerService.GetByIdAsync(id, companyId);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Customer retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Customer Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during customer retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer {CustomerId} for company {CompanyId}", id, GetCompanyId());
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving the customer."
                });
            }
        }

        /// <summary>
        /// Retrieves a paged list of customers for the authenticated user's company.
        /// </summary>
        /// <param name="pageNumber">The page number (default: 1).</param>
        /// <param name="pageSize">The number of customers per page (default: 10).</param>
        /// <param name="search">Optional search term to filter by name or email.</param>
        /// <returns>A <see cref="PaginatedResult{CustomerResponseDto}"/> containing the customers.</returns>
        /// <response code="200">Customers retrieved successfully.</response>
        /// <response code="400">Invalid pagination parameters or retrieval failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = null)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    _logger.LogWarning("Invalid pagination parameters: pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Pagination Parameters",
                        Detail = "Page number and page size must be greater than 0.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var companyId = GetCompanyId();
                _logger.LogInformation("Retrieving paged customers for company {CompanyId}, page {PageNumber}, size {PageSize}, search: {Search}", companyId, pageNumber, pageSize, search ?? "none");

                var result = await _customerService.GetPagedAsync(companyId, pageNumber, pageSize, search?.Trim());
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Paged customers retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Customer Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during paged customer retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged customers for company {CompanyId}", GetCompanyId());
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving customers."
                });
            }
        }
    }
}