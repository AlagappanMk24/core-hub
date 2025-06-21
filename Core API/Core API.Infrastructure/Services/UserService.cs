using AutoMapper;
using Core_API.Application.Common.Results;
using Core_API.Application.Contracts.Persistence;
using Core_API.Application.Contracts.Service;
using Core_API.Application.Contracts.Services;
using Core_API.Application.Features.Users.DTOs;
using Core_API.Application.Features.Users.ViewModels;
using Core_API.Application.Features.Users;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Models.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Core_API.Infrastructure.Shared;
using Microsoft.Extensions.Configuration;

namespace Core_API.Infrastructure.Service
{
    public class UserService(
       IUnitOfWork unitOfWork,
       UserManager<ApplicationUser> userManager,
       ILogger<UserService> logger, IMapper mapper,
       RoleManager<IdentityRole> roleManager,
       //ICompanyService companyService,
       CoreAPIDbContext dbContext,
       IEmailService emailService,
       IOptions<AdminSettings> adminSettings,
       IConfiguration configuration) : IUserService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<UserService> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        //private readonly ICompanyService _companyService = companyService;
        //private readonly IOptions<UserCleanupOptions> _cleanupOptions = cleanupOptions;
        private readonly CoreAPIDbContext _dbContext = dbContext;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;
        private readonly string _fallbackEmail = adminSettings.Value.FallbackEmail;
        public async Task<PaginatedResult<UserDto>> GetUsersPaginatedAsync(UserQueryParameters queryParams)
        {
            try
            {
                // Base query - includes users from UserManager
                var usersQuery = _userManager.Users
                    .Include(u => u.Company)
                    .Where(u => !u.IsDeleted) // ✅ Soft delete check
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(queryParams.SearchTerm))
                {
                    string searchTerm = queryParams.SearchTerm.ToLower().Trim();
                    usersQuery = usersQuery.Where(u =>
                        u.FullName.ToLower().Contains(searchTerm) ||
                        u.Email.ToLower().Contains(searchTerm) ||
                        u.PhoneNumber.Contains(searchTerm)
                    );
                }

                // Apply company filter
                if (queryParams.CompanyId.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.CompanyId == queryParams.CompanyId.Value);
                }

                // Apply role filter
                if (!string.IsNullOrEmpty(queryParams.Role))
                {
                    // This requires handling differently since roles aren't directly in the User table
                    // We'll get all users in the role first
                    var usersInRole = await _userManager.GetUsersInRoleAsync(queryParams.Role);
                    var userIdsInRole = usersInRole.Select(u => u.Id);
                    usersQuery = usersQuery.Where(u => userIdsInRole.Contains(u.Id));
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(queryParams.SortColumn))
                {
                    usersQuery = ApplySorting(usersQuery, queryParams.SortColumn, queryParams.SortDirection);
                }
                else
                {
                    usersQuery = usersQuery.OrderBy(u => u.FullName);
                }

                // Get total count before pagination
                var totalCount = await usersQuery.CountAsync();

                // Apply pagination
                var users = await usersQuery
                    .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToListAsync();

                // Define country code to prefix mapping
                var countryCodeToPrefixMap = new Dictionary<string, string>
                {
                    ["US"] = "+1",
                    ["CA"] = "+1",
                    ["IN"] = "+91",
                    ["AU"] = "+61",
                    ["UK"] = "+44",
                    ["RU"] = "+7",
                    ["CN"] = "+86"
                };

                // Create DTOs with roles
                var userDtos = new List<UserDto>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // Combine CountryCode and PhoneNumber for display in PhoneNumber field
                    string combinedPhoneNumber = string.Empty;
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        if (!string.IsNullOrEmpty(user.CountryCode) && countryCodeToPrefixMap.TryGetValue(user.CountryCode, out var prefix))
                        {
                            combinedPhoneNumber = $"{prefix}{user.PhoneNumber}";
                        }
                        else
                        {
                            combinedPhoneNumber = user.PhoneNumber; // Fallback if country code not found
                        }
                    }
                    else
                    {
                        combinedPhoneNumber = "Not specified";
                    }

                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email ?? user.UserName ?? "Not specified",
                        CountryCode = user?.CountryCode ?? string.Empty,
                        PhoneNumber = combinedPhoneNumber,
                        CompanyId = user.CompanyId,
                        CompanyName = user?.Company?.Name ?? "N/A",
                        Roles = roles?.ToList() ?? new List<string>(),
                        ProfileImageUrl = user?.ProfileImageUrl ?? "/images/default-avatar.png",
                        //Address = string.IsNullOrEmpty(user.Address1) ? string.Empty :
                        //    $"{user.Address1}, {user.City}, {user.State} {user.PostalCode}"
                    });
                }

                return new PaginatedResult<UserDto>
                {
                    Items = userDtos,
                    TotalCount = totalCount,
                    PageNumber = queryParams.PageNumber,
                    PageSize = queryParams.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated users");
                throw;
            }
        }
        private IQueryable<ApplicationUser> ApplySorting(IQueryable<ApplicationUser> query, string sortColumn, string sortDirection)
        {
            bool isAscending = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);

            return sortColumn.ToLower() switch
            {
                "name" => isAscending ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName),
                "email" => isAscending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "company" => isAscending ?
                    query.OrderBy(u => u.Company != null ? u.Company.Name : "") :
                    query.OrderByDescending(u => u.Company != null ? u.Company.Name : ""),
                _ => isAscending ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName),
            };
        }
        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return null;
                }

                var roles = await _userManager.GetRolesAsync(user);

                return new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    StreetAddress = user.StreetAddress,
                    State = user.State,
                    City = user.City,
                    PostalCode = user.PostalCode,
                    CompanyId = user.CompanyId,
                    Roles = roles.ToList(),
                    CountryCode = user.CountryCode,
                    ProfileImageUrl = user.ProfileImageUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user by ID: {UserId}", id);
                throw;
            }
        }
        public async Task<OperationResult<string>> CreateUserAsync(UserDto userDto, string currentUserId)
        {
            try
            {
                // Check if email is already in use
                var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Email {Email} is already in use by user ID {UserId} (IsDeleted: {IsDeleted})",
                        userDto.Email, existingUser.Id, existingUser.IsDeleted);
                    return OperationResult<string>.FailureResult("Email is already in use.");
                }

                if (existingUser != null)
                {
                    return OperationResult<string>.FailureResult("Email is already in use.");
                }
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("Could not determine the current user ID for CreatedBy field.");
                    return OperationResult<string>.FailureResult("Unable to determine the current user.");
                }

                // Validate country code
                if (string.IsNullOrEmpty(userDto.CountryCode) || string.IsNullOrEmpty(userDto.PhoneNumber))
                {
                    return OperationResult<string>.FailureResult("Country code and phone number are required.");
                }

                var countryConfigs = new Dictionary<string, string>
                {
                    ["US"] = "+1",
                    ["CA"] = "+1",
                    ["IN"] = "+91",
                    ["AU"] = "+61",
                    ["UK"] = "+44",
                    ["RU"] = "+7",
                    ["CN"] = "+86"
                };
                if (!countryConfigs.ContainsKey(userDto.CountryCode))
                {
                    return OperationResult<string>.FailureResult("Unsupported country code.");
                }

                // Map UserDto to ApplicationUser
                ApplicationUser newUser;
                try
                {
                    newUser = _mapper.Map<ApplicationUser>(userDto);
                }
                catch (ValidationException ex)
                {
                    return OperationResult<string>.FailureResult(ex.Message);
                }

                // Set audit and confirmation fields
                newUser.CreatedBy = currentUserId;
                newUser.CreatedDate = DateTime.UtcNow;
                newUser.EmailConfirmed = true;
                newUser.PhoneNumberConfirmed = true;
                newUser.IsFirstLogin = true; // Set flag for first login

                // Log the user object before creation
                _logger.LogInformation("Creating user with Email: {Email}, PhoneNumber: {PhoneNumber}, CreatedBy: {CreatedBy}, CreatedDate: {CreatedDate}",
                    newUser.Email, newUser.PhoneNumber, newUser.CreatedBy, newUser.CreatedDate);

                var createResult = await _userManager.CreateAsync(newUser, userDto.Password);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.ToList();
                    _logger.LogWarning("User creation failed with errors: {Errors}", string.Join(", ", errors.Select(e => e.Description)));
                    // Map DuplicateUserName to Email error
                    if (errors.Any(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail"))
                    {
                        return OperationResult<string>.FailureResult("Email is already in use.");
                    }
                    return OperationResult<string>.FailureResult(errors, string.Join(", ", errors.Select(e => e.Description)));
                }

                // Assign multiple roles
                if (userDto.Roles != null && userDto.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(newUser, userDto.Roles);
                    if (!roleResult.Succeeded)
                    {
                        await _userManager.DeleteAsync(newUser);
                        return OperationResult<string>.FailureResult(
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
                // Create a login URL (fixed to use absolute URL instead of Url.Page)
                string loginUrl = $"{_configuration["AppSettings:BaseUrl"]}/Identity/Account/Login?email={Uri.EscapeDataString(newUser.Email)}";

                var welcomeEmailRequest = new WelcomeEmailRequest
                {
                    Email = newUser.Email,
                    TemporaryPassword = userDto.Password,
                    Name = newUser.FullName,
                    HtmlMessage = $"Please log in to set your password: <a href='{loginUrl}'>Log In</a>"
                };

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(welcomeEmailRequest);

                return OperationResult<string>.SuccessResult("User created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return OperationResult<string>.FailureResult(ex.Message);
            }
        }
        public async Task<OperationResult<string>> UpdateUserAsync(UserDto userDto, string currentUserId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userDto.Id);
                if (user == null)
                {
                    return OperationResult<string>.FailureResult("User not found.");
                }

                // Check if email is already in use by another user (include soft-deleted)
                if (user.Email != userDto.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
                    if (existingUser != null && existingUser.Id != userDto.Id)
                    {
                        _logger.LogWarning("Email {Email} is already in use by user ID {UserId} (IsDeleted: {IsDeleted})",
                            userDto.Email, existingUser.Id, existingUser.IsDeleted);
                        return OperationResult<string>.FailureResult("Email is already in use.");
                    }
                }

                // Map UserDto to existing ApplicationUser
                try
                {
                    _mapper.Map(userDto, user); // Updates user with userDto values
                }
                catch (ValidationException ex)
                {
                    return OperationResult<string>.FailureResult(ex.Message);
                }

                // Set audit fields
                user.UpdatedBy = currentUserId;
                user.UpdatedDate = DateTime.UtcNow;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.ToList();
                    _logger.LogWarning("User update failed with errors: {Errors}", string.Join(", ", errors.Select(e => e.Description)));
                    if (errors.Any(e => e.Code == "DuplicateUserName" || e.Code == "DuplicateEmail"))
                    {
                        return OperationResult<string>.FailureResult("Email is already in use.");
                    }
                    return OperationResult<string>.FailureResult(errors, string.Join(", ", errors.Select(e => e.Description)));
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(userDto.Roles).ToList();
                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        return OperationResult<string>.FailureResult(string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    }
                }
                var rolesToAdd = userDto.Roles.Except(currentRoles).ToList();
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        return OperationResult<string>.FailureResult(string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    }
                }

                return OperationResult<string>.SuccessResult("User updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", userDto.Id);
                return OperationResult<string>.FailureResult(ex.Message);
            }
        }
        public async Task<OperationResult<string>> DeleteUserAsync(string userId, string currentUserId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return OperationResult<string>.FailureResult("User not found.");
                }

                // Soft delete: set IsDeleted = true instead of deleting
                user.IsDeleted = true;
                user.UpdatedBy = currentUserId;
                user.UpdatedDate = DateTime.UtcNow;
                user.DeletedDate = DateTime.UtcNow;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to soft delete user with ID: {UserId}. Errors: {Errors}", userId, errors);
                    return OperationResult<string>.FailureResult(errors);
                }
                _logger.LogInformation("User with ID: {UserId} successfully marked as deleted.", userId);
                return OperationResult<string>.SuccessResult("User deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting user with ID: {UserId}", userId);
                return OperationResult<string>.FailureResult("An error occurred while marking the user as deleted.");
            }
        }
        public string GetUserId(ClaimsPrincipal user)
        {
            var claimsIdentity = (ClaimsIdentity)user.Identity;
            return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        //public async Task<ApplicationUser> GetApplicationUser(string userId)
        //{
        //    return await _unitOfWork.ApplicationUsers.GetAsync(u => u.Id == userId);
        //}
        //public async Task<UserUpsertVM> CreateUserUpsertVMAsync(UserDto userDto)
        //{
        //    try
        //    {
        //        // Map UserDto to UserUpsertVM
        //        var userUpsertVM = _mapper.Map<UserUpsertVM>(userDto);

        //        // Ensure SelectedRoles is initialized
        //        userUpsertVM.SelectedRoles ??= new List<string>();

        //        // Populate RoleList
        //        var roles = await _roleManager.Roles.ToListAsync();
        //        userUpsertVM.RoleList = roles.Select(r => new SelectListItem
        //        {
        //            Value = r.Name,
        //            Text = r.Name
        //        }).ToList();

        //        // Populate CompanyList
        //        var companies = await _companyService.GetAllCompaniesAsync();
        //        userUpsertVM.CompanyList = companies.Select(c => new SelectListItem
        //        {
        //            Value = c.Id.ToString(),
        //            Text = c.Name
        //        }).ToList();

        //        return userUpsertVM;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating UserUpsertVM for user: {Email}", userDto.Email);
        //        throw;
        //    }
        //}
        public async Task<OperationResult<string>> HardDeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsDeleted)
            {
                return OperationResult<string>.FailureResult("User not found or not soft-deleted.");
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return OperationResult<string>.FailureResult(
                    result.Errors.ToList(),
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            _logger.LogInformation("Hard-deleted user {Email}", user.Email);
            return OperationResult<string>.SuccessResult("User permanently deleted.");
        }
        public async Task<OperationResult<string>> ReleaseEmailAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsDeleted)
            {
                return OperationResult<string>.FailureResult("User not found or not soft-deleted.");
            }
            var oldEmail = user.Email;
            user.Email = $"deleted_{user.Id}@example.com";
            user.UserName = user.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(user.Email);
            user.NormalizedUserName = _userManager.NormalizeName(user.UserName);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return OperationResult<string>.FailureResult(
                    result.Errors.ToList(),
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            _logger.LogInformation("Released email {OldEmail} for user {UserId}", oldEmail, userId);
            return OperationResult<string>.SuccessResult("Email released successfully.");
        }

        public Task CleanupSoftDeletedUsersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> GetApplicationUser(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
