using Core_API.Application.Common.Models;
using Core_API.Application.Contracts.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core_API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger) : BaseApiController
    {
        private readonly IDashboardService _dashboardService = dashboardService;
        private readonly ILogger<DashboardController> _logger = logger;

        [HttpGet("admin/summary")]
        [Authorize(Roles = "Admin,Super Admin")]
        public async Task<IActionResult> GetAdminDashboardSummary()
        {
            var context = CurrentContext;
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
            var context = CurrentContext;
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
            var context = CurrentContext;
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
            var context = CurrentContext;
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
    }
}