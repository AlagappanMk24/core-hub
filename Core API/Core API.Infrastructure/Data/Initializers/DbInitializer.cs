using Core_API.Application.Common.Constants;
using Core_API.Application.Contracts.Services;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core_API.Infrastructure.Data.Initializers
{
    public class DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, CoreAPIDbContext dbContext, IPermissionService permissionService, ILogger<DbInitializer> logger) : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly CoreAPIDbContext _dbContext = dbContext;
        private readonly IPermissionService _permissionService = permissionService;
        private readonly ILogger<DbInitializer> _logger = logger;
        private readonly TimeSpan _operationTimeout = TimeSpan.FromMinutes(5); // Configurable timeout
        public async Task Initialize(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting database initialization...");

                await ApplyMigrationsAsync(cancellationToken);
                await CreateRolesAsync(cancellationToken);
                await CreateAdminUserAsync(cancellationToken);
                await SeedPermissionsAsync(cancellationToken);

                _logger.LogInformation("Database initialization completed successfully.");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Database initialization was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database initialization");
                throw;
            }
        }
        private async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Checking for pending migrations...");
                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    using var cts = new CancellationTokenSource(_operationTimeout);
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

                    await _dbContext.Database.MigrateAsync(linkedCts.Token);
                    _logger.LogInformation("Successfully applied {Count} migrations", pendingMigrations.Count());
                }
                else
                {
                    _logger.LogInformation("No pending migrations to apply.");
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Migration operation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying migrations: {Message}", ex.Message);
                throw;
            }
        }
        private async Task CreateRolesAsync(CancellationToken cancellationToken)
        {
            var roles = new[]
            {
                AppConstants.Role_Customer,
                AppConstants.Role_Employee,
                AppConstants.Role_Admin,
                AppConstants.Role_Admin_Super,
                AppConstants.Role_Supplier,
                AppConstants.Role_CustomerSupport,
                AppConstants.Role_DeliveryAgent,
                AppConstants.Role_Manager,
                AppConstants.Role_Vendor,
                AppConstants.Role_Company
            };

            foreach (var role in roles)
            {
                try
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        _logger.LogInformation("Creating role: {Role}", role);
                        using var cts = new CancellationTokenSource(_operationTimeout);
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

                        var result = await _roleManager.CreateAsync(new IdentityRole(role));
                        if (!result.Succeeded)
                        {
                            _logger.LogError("Failed to create role {Role}", role);
                            foreach (var error in result.Errors)
                            {
                                _logger.LogError("Role creation error: {Error}", error.Description);
                            }
                            throw new InvalidOperationException($"Failed to create role {role}");
                        }
                        _logger.LogInformation("Successfully created role: {Role}", role);
                    }
                    else
                    {
                        _logger.LogInformation("Role {Role} already exists", role);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogError(ex, "Role creation for {Role} was canceled.", role);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating role {Role}: {Message}", role, ex.Message);
                    throw;
                }
            }
        }
        private async Task CreateAdminUserAsync(CancellationToken cancellationToken)
        {
            const string adminEmail = "alagappanmk98@gmail.com";

            try
            {
                var adminUser = await _userManager.FindByEmailAsync(adminEmail);
                if (adminUser != null)
                {
                    _logger.LogInformation("Admin user already exists");
                    return;
                }

                _logger.LogInformation("Creating admin user...");
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Alagappan",
                    CountryCode = "IN",
                    PhoneNumber = "8668453402",
                    StreetAddress = "123 St, Besant Nagar, Bt Town",
                    State = "TamilNadu",
                    PostalCode = "600001",
                    City = "Chennai",
                    EmailConfirmed = true
                };

                using var cts = new CancellationTokenSource(_operationTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

                var creationResult = await _userManager.CreateAsync(newAdmin, "Alagappan@123");
                if (!creationResult.Succeeded)
                {
                    _logger.LogError("Failed to create admin user");
                    foreach (var error in creationResult.Errors)
                    {
                        _logger.LogError("User creation error: {Error}", error.Description);
                    }
                    throw new InvalidOperationException("Failed to create admin user");
                }

                _logger.LogInformation("Admin user created successfully");

                var roleResult = await _userManager.AddToRoleAsync(newAdmin, AppConstants.Role_Admin);
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to add admin role to user");
                    foreach (var error in roleResult.Errors)
                    {
                        _logger.LogError("Role assignment error: {Error}", error.Description);
                    }
                    throw new InvalidOperationException("Failed to assign admin role");
                }
                _logger.LogInformation("Successfully assigned admin role to user");

                var superAdminRoleResult = await _userManager.AddToRoleAsync(newAdmin, AppConstants.Role_Admin_Super);
                if (superAdminRoleResult.Succeeded)
                {
                    _logger.LogInformation("Successfully assigned super admin role to user");
                }
                else
                {
                    _logger.LogWarning("Failed to assign super admin role to user");
                    foreach (var error in superAdminRoleResult.Errors)
                    {
                        _logger.LogWarning("Super admin role assignment error: {Error}", error.Description);
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Admin user creation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin user: {Message}", ex.Message);
                throw;
            }
        }
        private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Seeding default permissions...");
                using var cts = new CancellationTokenSource(_operationTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

                await _permissionService.SeedDefaultPermissionsAsync();
                _logger.LogInformation("Successfully seeded default permissions.");
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Permission seeding was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding permissions: {Message}", ex.Message);
                throw;
            }
        }
    }
}
