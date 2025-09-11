using Core_API.Application.Contracts.Services;
using Core_API.Application.DTOs.Company.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Core_API.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Customer, User")]
    public class CompanyController(ICompanyService companyService, ILogger<CompanyController> logger) : ControllerBase
    {
        private readonly ICompanyService _companyService = companyService;
        private readonly ILogger<CompanyController> _logger = logger;

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

        [HttpGet("{id}")]
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

        [HttpDelete("{id}")]
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
}