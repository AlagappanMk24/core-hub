using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core_API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger) : ControllerBase
    {
        private readonly IDashboardService _dashboardService = dashboardService;
        private readonly ILogger<DashboardController> _logger = logger;

        [HttpGet("admin/summary")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAdminDashboardSummary()
        {
            var context = GetOperationContext();
            var result = await _dashboardService.GetAdminDashboardAsync(context);

            if (!result.IsSuccess)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dashboard Retrieval Failed",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(result.Data);
        }

        [HttpGet("customer/summary")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCustomerDashboardSummary()
        {
            var context = GetOperationContext();
            var result = await _dashboardService.GetCustomerDashboardAsync(context);

            if (!result.IsSuccess)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dashboard Retrieval Failed",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(result.Data);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var context = GetOperationContext();
            var result = await _dashboardService.GetStatsAsync(context);

            if (!result.IsSuccess)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Stats Retrieval Failed",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(result.Data);
        }

        [HttpGet("recent-invoices")]
        public async Task<IActionResult> GetRecentInvoices([FromQuery] int count = 5)
        {
            var context = GetOperationContext();
            var result = await _dashboardService.GetRecentInvoicesAsync(context, count);

            if (!result.IsSuccess)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Recent Invoices Retrieval Failed",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(result.Data);
        }
        private OperationContext GetOperationContext()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            var customerIdClaim = User.FindFirst("customerId")?.Value;

            int? companyId = int.TryParse(companyIdClaim, out var cId) ? cId : null;
            int? customerId = int.TryParse(customerIdClaim, out var custId) ? custId : null;

            return new OperationContext(userId, companyId, customerId);
        }
    }
}