using Core_API.Application.Contracts.Services.Customers;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.Features.Customers.Commands.CreateCustomer;
using Core_API.Application.Features.Customers.Commands.DeleteCustomer;
using Core_API.Application.Features.Customers.Commands.UpdateCustomer;
using Core_API.Application.Features.Customers.Queries.GetCustomerActivities;
using Core_API.Application.Features.Customers.Queries.GetCustomerById;
using Core_API.Application.Features.Customers.Queries.GetCustomerInvoices;
using Core_API.Application.Features.Customers.Queries.GetCustomerPayments;
using Core_API.Application.Features.Customers.Queries.GetCustomers;
using Core_API.Application.Features.Customers.Queries.GetCustomerSpendingTrend;
using Core_API.Application.Features.Customers.Queries.GetCustomerStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public class CustomerController(IMediator mediator, ICustomerService customerService, ILogger<CustomerController> logger) : BaseApiController
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly ICustomerService _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        private readonly ILogger<CustomerController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
        public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Invalid customer data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }
                var result = await _mediator.Send(command);

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
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, new ProblemDetails
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerCommand command)
        {
            try
            {
                if (!ModelState.IsValid || id != command.Id)
                {
                    _logger.LogWarning("Invalid model state or ID mismatch for customer update. ID: {Id}, DTO ID: {DtoId}", id, command.Id);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = id != command.Id ? "ID in URL does not match DTO ID." : "Invalid customer data provided.",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { { "errors", ModelState } }
                    });
                }

                var context = CurrentContext;
                _logger.LogInformation("Updating customer {CustomerId} for company {CompanyId} by user {UserId}",
                   id, context.CompanyId, context.UserId);

                command = command with { Context = context };

                var result = await _mediator.Send(command);

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
                _logger.LogError(ex, "Database error updating customer {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while updating the customer in the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
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
                var context = CurrentContext;

                _logger.LogInformation("Deleting customer {CustomerId} for company {CompanyId} by user {UserId}",
                    id, context.CompanyId, context.UserId);

                var command = new DeleteCustomerCommand { Id = id, Context = context };

                var result = await _mediator.Send(command);

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
                _logger.LogError(ex, "Database error deleting customer {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Database Error",
                    Detail = "An error occurred while deleting the customer from the database."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
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
                var context = CurrentContext;

                _logger.LogInformation("Retrieving customer {CustomerId} for company {CompanyId}", id, context.CompanyId);

                var query = new GetCustomerByIdQuery { Id = id, Context = context };

                var result = await _mediator.Send(query);

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
                _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving the customer."
                });
            }
        }

        /// <summary>
        /// Retrieves a paged list of customers for the authenticated user's company.
        /// </summary>
        /// <param name="filter">The filter parameters for pagination and search.</param>
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
        public async Task<IActionResult> GetPaged([FromQuery] CustomerFilterRequestDto filter)
        {
            try
            {
                if (!filter.IsValid())
                {
                    _logger.LogWarning("Invalid filter parameters: pageNumber={PageNumber}, pageSize={PageSize}, status={Status}",
                        filter.PageNumber, filter.PageSize, filter.Status ?? "none");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Filter Parameters",
                        Detail = "Page number and page size must be greater than 0, and status must be 'All', 'Active', or 'Inactive'.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var context = CurrentContext;
                // Validate company context
                if (!IsSuperAdmin && !context.CompanyId.HasValue)
                {
                    _logger.LogWarning("Cannot retrieve customers - missing company context");
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Detail = "Company context is missing.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                _logger.LogInformation("Retrieving paged customers for company {CompanyId}, page {PageNumber}, size {PageSize}, search: {Search}, status: {Status}",
                      context.CompanyId, filter.PageNumber, filter.PageSize, filter.Search ?? "none", filter.Status ?? "none");
                var query = new GetCustomersQuery
                {
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Search = filter.Search,
                    Status = filter.Status,
                    Context = context
                };

                var result = await _mediator.Send(query);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged customers");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving customers."
                });
            }
        }


        /// <summary>
        /// Retrieves customer statistics for the authenticated user's company.
        /// </summary>
        /// <returns>A <see cref="CustomerStatsDto"/> containing customer statistics.</returns>
        /// <response code="200">Statistics retrieved successfully.</response>
        /// <response code="400">Statistics retrieval failed.</response>
        /// <response code="401">Unauthorized access due to invalid token.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var context = CurrentContext;
                _logger.LogInformation("Retrieving customer stats for company {CompanyId}", context.CompanyId);

                var query = new GetCustomerStatsQuery { Context = context };

                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Customer stats retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Customer Stats Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during customer stats retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer stats");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving customer stats."
                });
            }
        }

        /// <summary>
        /// Gets all invoices for a specific customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>List of customer invoices</returns>
        [HttpGet("{customerId}/invoices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCustomerInvoices(int customerId)
        {
            try
            {
                var context = CurrentContext;
                _logger.LogInformation("Retrieving invoices for customer {CustomerId}", customerId);

                var query = new GetCustomerInvoicesQuery
                {
                    CustomerId = customerId,
                    Context = context
                };

                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Customer Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer invoices for customer {CustomerId}", customerId);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving customer invoices."
                });
            }
        }

        /// <summary>
        /// Gets all payments for a specific customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>List of customer payments</returns>
        [HttpGet("{customerId}/payments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCustomerPayments(int customerId)
        {
            try
            {
                var context = CurrentContext;
                _logger.LogInformation("Retrieving payments for customer {CustomerId}", customerId);

                var query = new GetCustomerPaymentsQuery
                {
                    CustomerId = customerId,
                    Context = context
                };

                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Customer Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer payments for customer {CustomerId}", customerId);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving customer payments."
                });
            }
        }

        /// <summary>
        /// Gets recent activities for a specific customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>List of customer activities</returns>
        [HttpGet("{customerId}/activities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCustomerActivities(int customerId)
        {
            try
            {
                var context = CurrentContext;
                _logger.LogInformation("Retrieving activities for customer {CustomerId}", customerId);

                var query = new GetCustomerActivitiesQuery
                {
                    CustomerId = customerId,
                    Context = context
                };

                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Customer Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer activities for customer {CustomerId}", customerId);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving customer activities."
                });
            }
        }

        /// <summary>
        /// Gets spending trend for a specific customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>List of spending trend data</returns>
        [HttpGet("{customerId}/spending-trend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCustomerSpendingTrend(int customerId)
        {
            try
            {
                var context = CurrentContext;
                _logger.LogInformation("Retrieving spending trend for customer {CustomerId}", customerId);

                var query = new GetCustomerSpendingTrendQuery
                {
                    CustomerId = customerId,
                    Context = context
                };

                var result = await _mediator.Send(query);

                if (!result.IsSuccess)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Customer Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving spending trend for customer {CustomerId}", customerId);
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving spending trend."
                });
            }
        }
    }
}