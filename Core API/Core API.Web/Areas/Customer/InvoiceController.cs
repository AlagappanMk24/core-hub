using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core_API.Web.Areas.Customer
{
    [Route("api/customer")]
    [ApiController]
    [Authorize(Policy = "Customer")]
    public class InvoiceController(IInvoiceService invoiceService, ILogger<CustomerDashboardController> logger) : ControllerBase
    {
        private readonly IInvoiceService _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        private readonly ILogger<CustomerDashboardController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private OperationContext GetOperationContext()
        {
            var customerIdClaim = User.FindFirst("customerId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID claim is missing.");
                throw new UnauthorizedAccessException("User ID claim is missing.");
            }

            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out var customerId) || customerId <= 0)
            {
                _logger.LogError("Valid CustomerId claim is missing or invalid.");
                throw new UnauthorizedAccessException("Valid CustomerId claim is required.");
            }

            // Create context for customer-specific operations (no CompanyId)
            return new OperationContext(userId, customerId: customerId);
        }

        [HttpGet("invoices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerInvoices([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string status = null)
        {
            //var operationContext = GetOperationContext();
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

                var context = GetOperationContext();

                _logger.LogInformation("Retrieving invoices for customer {CustomerId}, page {PageNumber}, size {PageSize}, status: {Status}", context.CustomerId, pageNumber, pageSize, status ?? "none");
                var result = await _invoiceService.GetCustomerInvoicesAsync(context, pageNumber, pageSize, status);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Customer invoices retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Invoice Retrieval Failed",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during customer invoice retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices for customer.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving invoices."
                });
            }
        }

        [HttpGet("invoices/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerInvoiceById(int id)
        {
            try
            {
                var context = GetOperationContext();
                _logger.LogInformation("Retrieving invoice {InvoiceId} for customer {CustomerId}", id, context.CustomerId);

                var result = await _invoiceService.GetCustomerInvoiceByIdAsync(id, context);
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Invoice retrieval failed: {ErrorMessage}", result.ErrorMessage);
                    return NotFound(new ProblemDetails
                    {
                        Title = "Invoice Not Found",
                        Detail = result.ErrorMessage,
                        Status = StatusCodes.Status404NotFound
                    });
                }

                return Ok(result.Data);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access during invoice retrieval.");
                return Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices for company.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred while retrieving invoices."
                });
            }
        }
    }
}