using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Company.Request;
using Core_API.Application.DTOs.User.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System.Security.Claims;

namespace Core_API.Web.Controllers;

[Route("api/companies")]
[ApiController]
[Authorize(Roles = "Admin, Customer, User")]
public class CompanyController(ICompanyService companyService, ILogger<CompanyController> logger) : BaseApiController
{
    private readonly ICompanyService _companyService = companyService;
    private readonly ILogger<CompanyController> _logger = logger;

    /// <summary>
    /// Retrieves all companies (only for Super Admin)
    /// </summary>
    [HttpGet("admin-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<CompanyDto>>> GetAllCompaniesForAdmin()
    {
        var operationContext = CurrentContext;
        try
        {
            // Only Super Admin can access all companies
            if (!operationContext.IsSuperAdmin)
            {
                _logger.LogWarning("Non-Super Admin attempted to access all companies");
                return Forbid();
            }

            _logger.LogInformation("Super Admin retrieving all companies");
            var result = await _companyService.GetAllCompaniesAsync(operationContext);

            if (!result.IsSuccess)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Failed to retrieve companies",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while retrieving companies."
            });
        }
    }

    /// <summary>
    /// Path: GET api/companies
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCompanies()
    {
        try
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies");
            return StatusCode(500, new { message = "An error occurred while retrieving companies." });
        }
    }

    /// <summary>
    /// Path: GET api/companies/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCompanyById(int id)
    {
        try
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            return Ok(company);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Company with ID {CompanyId} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company {CompanyId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the company." });
        }
    }

    /// <summary>
    /// Path: POST api/companies
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("CompanyCreationPolicy")]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyCreateDto companyDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            var company = await _companyService.CreateCompanyAsync(companyDto, userId);
            return CreatedAtAction(nameof(GetCompanyById), new { id = company.Id }, company);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid company data");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Company creation failed");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return StatusCode(500, new { message = "An error occurred while creating the company." });
        }
    }

    /// <summary>
    /// Path: DELETE api/companies/{id}
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        try
        {
            var deleted = await _companyService.DeleteCompanyAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Company with ID {CompanyId} not found", id);
                return NotFound(new { message = $"Company with ID {id} not found." });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete company {CompanyId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company {CompanyId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the company." });
        }
    }
}