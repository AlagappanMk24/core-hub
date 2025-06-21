using Core_API.Application.Contracts.Services;
using Core_API.Application.Features.Users;
using Core_API.Application.Features.Users.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Web.Areas.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompanyService _companyService;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ICompanyService companyService)
        {
            _userService = userService;
            _roleManager = roleManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _companyService = companyService;
        }

        // GET: Fetch paginated users
        [HttpPost("get-users")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUsers([FromBody] UserQueryParameters queryParams)
        {
            try
            {
                var result = await _userService.GetUsersPaginatedAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, new { error = "Error fetching users" });
            }
        }

        // GET: Fetch user by ID for editing
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var userDto = await _userService.GetUserByIdAsync(id);
                if (userDto == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound(new { error = "User not found" });
                }
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
                return StatusCode(500, new { error = "Error fetching user" });
            }
        }

        // GET: Fetch data for user upsert form (roles and companies)
        [HttpGet("upsert-data")]
        public async Task<IActionResult> GetUpsertData()
        {
            try
            {
                var companies = await _companyService.GetAllCompaniesAsync();
                var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

                var upsertData = new
                {
                    Companies = companies.Select(c => new { c.Id, c.Name }).ToList(),
                    Roles = roles
                };

                return Ok(upsertData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upsert data");
                return StatusCode(500, new { error = "Error fetching upsert data" });
            }
        }

        // POST: Create a new user
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserDto userDto, IFormFile? file)
        {
            string newImagePath = null;
            try
            {
                string webRootPath = _webHostEnvironment.WebRootPath;

                // Handle profile image upload
                if (file != null && file.Length > 0)
                {
                    string originalExtension = Path.GetExtension(file.FileName).ToLower();
                    string normalizedExtension = originalExtension switch
                    {
                        ".jpeg" or ".jpe" or ".jfif" or ".jif" or ".jfi" or ".avif" => ".jpg",
                        _ => originalExtension
                    };

                    string fileName = Guid.NewGuid().ToString() + normalizedExtension;
                    string userPath = Path.Combine(webRootPath, "images", "users");

                    if (!Directory.Exists(userPath))
                    {
                        Directory.CreateDirectory(userPath);
                    }

                    newImagePath = Path.Combine(userPath, fileName);
                    using (var fileStream = new FileStream(newImagePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    userDto.ProfileImageUrl = $"/images/users/{fileName}";
                }

                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("Could not determine the current user ID in CreateUser action.");
                    if (newImagePath != null && System.IO.File.Exists(newImagePath))
                    {
                        System.IO.File.Delete(newImagePath);
                    }
                    return BadRequest(new { error = "Unable to determine the current user" });
                }

                var result = await _userService.CreateUserAsync(userDto, currentUserId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Created user with ID: {Id}", result.Data);
                    return Ok(new { success = true, message = "User created successfully" });
                }

                if (newImagePath != null && System.IO.File.Exists(newImagePath))
                {
                    System.IO.File.Delete(newImagePath);
                }

                return BadRequest(new { error = result.ErrorMessage ?? "Failed to create user", errors = result.Errors });
            }
            catch (Exception ex)
            {
                if (newImagePath != null && System.IO.File.Exists(newImagePath))
                {
                    System.IO.File.Delete(newImagePath);
                }
                _logger.LogError(ex, "Error occurred during user creation");
                return StatusCode(500, new { error = "An error occurred while creating the user" });
            }
        }

        // PUT: Update an existing user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] UserDto userDto, IFormFile? file)
        {
            string newImagePath = null;
            try
            {
                userDto.Id = id;
                string webRootPath = _webHostEnvironment.WebRootPath;

                // Handle profile image upload
                if (file != null && file.Length > 0)
                {
                    string originalExtension = Path.GetExtension(file.FileName).ToLower();
                    string normalizedExtension = originalExtension switch
                    {
                        ".jpeg" or ".jpe" or ".jfif" or ".jif" or ".jfi" or ".avif" => ".jpg",
                        _ => originalExtension
                    };

                    string fileName = Guid.NewGuid().ToString() + normalizedExtension;
                    string userPath = Path.Combine(webRootPath, "images", "users");

                    if (!Directory.Exists(userPath))
                    {
                        Directory.CreateDirectory(userPath);
                    }

                    newImagePath = Path.Combine(userPath, fileName);
                    using (var fileStream = new FileStream(newImagePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    userDto.ProfileImageUrl = $"/images/users/{fileName}";
                }

                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("Could not determine the current user ID in UpdateUser action.");
                    if (newImagePath != null && System.IO.File.Exists(newImagePath))
                    {
                        System.IO.File.Delete(newImagePath);
                    }
                    return BadRequest(new { error = "Unable to determine the current user" });
                }

                var oldUser = await _userService.GetUserByIdAsync(id);
                string oldImagePath = string.IsNullOrEmpty(oldUser?.ProfileImageUrl)
                    ? null
                    : Path.Combine(webRootPath, oldUser.ProfileImageUrl.TrimStart('/'));

                var result = await _userService.UpdateUserAsync(userDto, currentUserId);
                if (result.IsSuccess)
                {
                    if (oldImagePath != null && System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    _logger.LogInformation("Updated user with ID: {Id}", id);
                    return Ok(new { success = true, message = "User updated successfully" });
                }

                if (newImagePath != null && System.IO.File.Exists(newImagePath))
                {
                    System.IO.File.Delete(newImagePath);
                }

                return BadRequest(new { error = result.ErrorMessage ?? "Failed to update user", errors = result.Errors });
            }
            catch (Exception ex)
            {
                if (newImagePath != null && System.IO.File.Exists(newImagePath))
                {
                    System.IO.File.Delete(newImagePath);
                }
                _logger.LogError(ex, "Error occurred during user update");
                return StatusCode(500, new { error = "An error occurred while updating the user" });
            }
        }

        // DELETE: Soft delete a user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for deletion.", id);
                    return NotFound(new { error = "User not found" });
                }

                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var webRootPath = _webHostEnvironment.WebRootPath;
                    var imagePath = Path.Combine(webRootPath, user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("Could not determine the current user ID in DeleteUser action.");
                    return BadRequest(new { error = "Unable to determine the current user" });
                }

                var result = await _userService.DeleteUserAsync(id, currentUserId);
                if (result.IsSuccess)
                {
                    _logger.LogInformation("User with ID {UserId} soft-deleted successfully.", id);
                    return Ok(new { success = true, message = "User deleted successfully" });
                }

                _logger.LogWarning("Failed to soft-delete user with ID {UserId}.", id);
                return BadRequest(new { error = "Failed to delete user" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft-deleting user with ID: {UserId}", id);
                return StatusCode(500, new { error = "An error occurred while soft-deleting the user" });
            }
        }

        // POST: Release email for a soft-deleted user
        [HttpPost("release-email/{userId}")]
        public async Task<IActionResult> ReleaseEmail(string userId)
        {
            try
            {
                var result = await _userService.ReleaseEmailAsync(userId);
                if (result.IsSuccess)
                {
                    return Ok(new { success = true, message = "Email released successfully" });
                }
                return BadRequest(new { error = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing email for user with ID: {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while releasing the email" });
            }
        }
    }
}
