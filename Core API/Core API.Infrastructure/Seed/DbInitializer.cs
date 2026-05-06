using Core_API.Application.Common.Constants;
using Core_API.Application.Contracts.Services.Auth;
using Core_API.Domain.Entities.Companies;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Entities.RecurringInvoices;
using Core_API.Domain.Enums;
using Core_API.Domain.ValueObjects;
using Core_API.Infrastructure.Helpers;
using Core_API.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core_API.Infrastructure.Seed;
public class DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, CoreInvoiceDbContext dbContext, IPermissionService permissionService, ILogger<DbInitializer> logger) : IDbInitializer
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly CoreInvoiceDbContext _dbContext = dbContext;
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
            await SeedCompaniesAsync(cancellationToken); // Seed companies after admin creation
            await SeedDataAsync(cancellationToken); // Seed other data
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
            AppConstants.Role_Admin,
            AppConstants.Role_Admin_Super,
            AppConstants.Role_User
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
        const string adminEmail = "alagappanmuthukumar1998@gmail.com";

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
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "system",
                IsDeleted = false
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
    private async Task SeedCompaniesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Seeding companies...");
            var adminEmail = "alagappanmuthukumar1998@gmail.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                _logger.LogError("Admin user not found. Cannot seed companies.");
                throw new InvalidOperationException("Admin user not found.");
            }

            var companies = new[]
            {
                new Company
                {
                    Name = "KL Infotech",
                    TaxId = "GSTIN1234567890",
                    Address = new Address(
                        addressLine1: "123 Tech Park",
                        city: "Chennai",
                        zipCode: "600100",
                        countryCode: "IN",
                        addressLine2: null,
                        state: "Tamil Nadu"
                    ),
                    Email = new Email(value:"contact@klinfotech.com"),
                    PhoneNumber = new PhoneNumber(value:"+91-44-12345678"),
                    CreatedByUserId = adminUser.Id,
                    CreatedBy = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-6),
                    IsDeleted = false
                },
                new Company
                {
                    Name = "Tech Solutions Inc.",
                    TaxId = "EIN987654321",
                    Address = new Address(
                        addressLine1: "456 Innovation Drive",
                        city: "San Francisco",
                        zipCode: "94105",
                        countryCode: "US",
                        addressLine2: null,
                        state: "CA"
                    ),
                    Email = new Email(value:"info@techsolutions.com"),
                    PhoneNumber = new PhoneNumber(value:"+1-415-555-1234"),
                    CreatedByUserId = adminUser.Id,
                    CreatedBy = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-5),
                    IsDeleted = false
                },
                new Company
                {
                    Name = "Global Enterprises",
                    TaxId = "VAT456789123",
                    Address = new Address(
                        addressLine1: "789 Business Avenue",
                        city: "London",
                        zipCode: "EC1A 1BB",
                        countryCode: "UK",
                        addressLine2: null,
                        state: null
                    ),
                    Email = new Email(value:"support@globalenterprises.co.uk"),
                    PhoneNumber = new PhoneNumber(value:"+44-20-1234-5678"),
                    CreatedByUserId = adminUser.Id,
                    CreatedBy = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-4),
                    IsDeleted = false
                },
                new Company
                {
                    Name = "Innovate Tech Ltd",
                    TaxId = "GSTIN9876543210",
                    Address = new Address(
                        addressLine1: "101 Future Road",
                        city: "Bangalore",
                        zipCode: "560001",
                        countryCode: "IN",
                        addressLine2: "Suite 500",
                        state: "Karnataka"
                    ),
                    Email = new Email(value:"contact@innovatetech.com"),
                    PhoneNumber = new PhoneNumber(value:"+91-80-98765432"),
                    CreatedByUserId = adminUser.Id,
                    CreatedBy = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-3),
                    IsDeleted = false
                },
                new Company
                {
                    Name = "Bright Future Corp",
                    TaxId = "EIN123456789",
                    Address = new Address(
                        addressLine1: "202 Progress Street",
                        city: "New York",
                        zipCode: "10002",
                        countryCode: "US",
                        addressLine2: null,
                        state: "NY"
                    ),
                    Email = new Email(value:"info@brightfuturecorp.com"),
                    PhoneNumber = new PhoneNumber(value:"+1-212-555-6789"),
                    CreatedByUserId = adminUser.Id,
                    CreatedBy = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-2),
                    IsDeleted = false
                },
                new Company
                {
                    Name = "Euro Dynamics",
                    TaxId = "VAT789123456",
                    Address = new Address(
                        addressLine1: "303 Enterprise Lane",
                        city: "Berlin",
                        zipCode: "10115",
                        countryCode: "DE",
                        addressLine2: null,
                        state: null
                    ),
                    Email = new Email(value:"support@eurodynamics.de"),
                    PhoneNumber = new PhoneNumber(value:"+49-30-12345678"),
                    CreatedByUserId = adminUser.Id,
                    CreatedBy = adminUser.Id,
                    CreatedDate = DateTime.UtcNow.AddMonths(-1),
                    IsDeleted = false
                }
            };

            int addedCount = 0;
            foreach (var companyToSeed in companies)
            {
                var existingCompany = await _dbContext.Companies
                    .FirstOrDefaultAsync(c => c.Name == companyToSeed.Name && !c.IsDeleted, cancellationToken);

                if (existingCompany != null)
                {
                    _logger.LogInformation("Company {CompanyName} already exists, skipping.", companyToSeed.Name);
                    continue;
                }

                _dbContext.Companies.Add(companyToSeed);
                addedCount++;
            }

            if (addedCount > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully seeded {Count} companies.", addedCount);
            }
            else
            {
                _logger.LogInformation("No new companies to seed.");
            }

            var company = await _dbContext.Companies
                .FirstOrDefaultAsync(c => c.Name == "KL Infotech" && !c.IsDeleted, cancellationToken);

            if (adminUser != null && company != null && adminUser.CompanyId == null)
            {
                adminUser.CompanyId = company.Id;
                adminUser.UpdatedBy = "system";
                adminUser.UpdatedDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(adminUser);
                _logger.LogInformation("Assigned CompanyId {CompanyId} to admin user.", company.Id);
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Company seeding was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding companies: {Message}", ex.Message);
            throw;
        }
    }
    private async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Seeding customers and related data...");

            // Retrieve existing companies
            var companies = await _dbContext.Companies
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (companies.Count == 0)
            {
                _logger.LogError("No companies found in the database. Cannot seed customers or related data.");
                throw new InvalidOperationException("No companies found in the database.");
            }

            // Map company names to IDs for easier reference
            var companyMap = companies.ToDictionary(c => c.Name, c => c.Id);

            // Seed Customer Groups first
            await SeedCustomerGroupsAsync(companyMap, cancellationToken);

            // Get customer groups
            var customerGroups = await _dbContext.CustomerGroups
                .Where(g => !g.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var groupMap = customerGroups.ToDictionary(g => (g.CompanyId, g.Name), g => g.Id);

            // Helper function to get group ID
            int? GetGroupId(int companyId, string groupName)
            {
                return groupMap.TryGetValue((companyId, groupName), out var id) ? id : null;
            }

            // Seed Customers with all new fields
            var customersToSeed = new List<Customer>();

            var klInfotechId = companyMap["KL Infotech"];
            var techSolutionsId = companyMap["Tech Solutions Inc."];
            var globalEnterprisesId = companyMap["Global Enterprises"];

            // KL Infotech Customers
            customersToSeed.AddRange(
            [
                new Customer {
                Name = "Alagappan M",
                Email = new Email("alagappanmk984@gmail.com"),
                PhoneNumber = new PhoneNumber("+91-98765-43210"),
                CompanyId = klInfotechId,
                Address = new Address(
                    addressLine1: "123 Maple St",
                    city: "Chennai",
                    zipCode: "600001",
                    countryCode: "IN",
                    addressLine2: "Apt 4B",
                    state: "Tamil Nadu"
                ),
                TaxId = "GSTIN123456789",
                Website = "https://alagappan.me",
                CreditLimit = 50000.00m,
                DefaultPaymentTerms = "Net 30",
                DefaultCurrency = "INR",
                CustomerGroupId = GetGroupId(klInfotechId, "Premium"),
                Status = CustomerStatus.Active,
                ActiveSince = DateTime.UtcNow.AddYears(-1),
                LastPurchaseDate = DateTime.UtcNow.AddDays(-15),
                TotalPurchases = 15000.00m,
                AveragePaymentDays = 15,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-5),
                IsDeleted = false
            },
            new Customer {
                Name = "Acme Corp",
                Email = new Email("billing@acmecorp.com"),
                PhoneNumber = new PhoneNumber("+1-555-123-4567"),
                CompanyId = klInfotechId,
                Address = new Address(
                    addressLine1: "789 Pine Rd",
                    city: "New York",
                    zipCode: "10001",
                    countryCode: "US",
                    addressLine2: null,
                    state: "NY"
                ),
                TaxId = "US-EIN-9876543",
                Website = "https://acmecorp.com",
                CreditLimit = 250000.00m,
                DefaultPaymentTerms = "Net 60",
                DefaultCurrency = "USD",
                CustomerGroupId = GetGroupId(klInfotechId, "Corporate"),
                Status = CustomerStatus.Active,
                ActiveSince = DateTime.UtcNow.AddYears(-2),
                LastPurchaseDate = DateTime.UtcNow.AddDays(-30),
                TotalPurchases = 85000.50m,
                AveragePaymentDays = 45,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-4),
                IsDeleted = false
            },
            new Customer {
                Name = "David Lee",
                Email = new Email("david.lee@example.com"),
                PhoneNumber = new PhoneNumber("+65-9876-5432"),
                CompanyId = klInfotechId,
                Address = new Address(
                    addressLine1: "101 Blossom Blvd",
                    addressLine2: "#12-45",
                    city: "Singapore",
                    state: "Singapore",
                    countryCode: "SG",
                    zipCode: "018906"
                ),
                TaxId = "SG-GST-987654321",
                Website = "https://davidlee.com",
                CreditLimit = 10000.00m,
                DefaultPaymentTerms = "Net 15",
                DefaultCurrency = "SGD",
                CustomerGroupId = GetGroupId(klInfotechId, "Regular"),
                Status = CustomerStatus.Active,
                ActiveSince = DateTime.UtcNow.AddMonths(-8),
                LastPurchaseDate = DateTime.UtcNow.AddDays(-45),
                TotalPurchases = 5000.00m,
                AveragePaymentDays = 10,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-3),
                IsDeleted = false
            },
            new Customer {
                Name = "Priya Sharma",
                Email = new Email("priya.sharma@example.com"),
                PhoneNumber = new PhoneNumber("+91-99876-54321"),
                CompanyId = klInfotechId,
                Address = new Address(
                    addressLine1: "202 Rosewood St",
                    addressLine2: "Bungalow 5",
                    city: "Mumbai",
                    state: "Maharashtra",
                    countryCode: "IN",
                    zipCode: "400001"
                ),
                TaxId = "GSTIN9876543210",
                Website = "https://priyasharma.com",
                CreditLimit = 25000.00m,
                DefaultPaymentTerms = "Net 30",
                DefaultCurrency = "INR",
                CustomerGroupId = GetGroupId(klInfotechId, "Premium"),
                Status = CustomerStatus.Active,
                ActiveSince = DateTime.UtcNow.AddMonths(-6),
                LastPurchaseDate = DateTime.UtcNow.AddDays(-20),
                TotalPurchases = 12000.00m,
                AveragePaymentDays = 12,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-2),
                IsDeleted = false
            },
            new Customer {
                Name = "Rahul Gupta",
                Email = new Email("rahul.gupta@example.com"),
                PhoneNumber = new PhoneNumber("+91-98765-12345"),
                CompanyId = klInfotechId,
                Address = new Address(
                    addressLine1: "303 Jasmine Apt",
                    addressLine2: "Tower A, Flat 1204",
                    city: "Bangalore",
                    state: "Karnataka",
                    countryCode: "IN",
                    zipCode: "560001"
                ),
                TaxId = "GSTIN4567890123",
                Website = "https://rahulgupta.com",
                CreditLimit = 15000.00m,
                DefaultPaymentTerms = "Net 15",
                DefaultCurrency = "INR",
                CustomerGroupId = GetGroupId(klInfotechId, "Regular"),
                Status = CustomerStatus.Active,
                ActiveSince = DateTime.UtcNow.AddMonths(-4),
                LastPurchaseDate = DateTime.UtcNow.AddDays(-60),
                TotalPurchases = 8000.00m,
                AveragePaymentDays = 8,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-1),
                IsDeleted = false
            }
            ]);

            // Tech Solutions Inc. Customers
            customersToSeed.AddRange([
                new Customer {
                    Name = "Sarah Johnson",
                    Email = new Email("sarah.j@example.com"),
                    PhoneNumber = new PhoneNumber("+1-617-555-0104"),
                    CompanyId = techSolutionsId,
                    Address = new Address(
                        addressLine1: "101 Elm St",
                        addressLine2: "Apt 3B",
                        city: "Boston",
                        state: "MA",
                        countryCode: "US",
                        zipCode: "02108"
                    ),
                    TaxId = "US-EIN-1234567",
                    Website = "https://sarahjohnson.com",
                    CreditLimit = 50000.00m,
                    DefaultPaymentTerms = "Net 30",
                    DefaultCurrency = "USD",
                    CustomerGroupId = GetGroupId(techSolutionsId, "Premium"),
                    Status = CustomerStatus.Active,
                    ActiveSince = DateTime.UtcNow.AddMonths(-10),
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-25),
                    TotalPurchases = 25000.00m,
                    AveragePaymentDays = 20,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-4),
                    IsDeleted = false
                },
                new Customer {
                    Name = "TechTrend Ltd",
                    Email = new Email("accounts@techtrend.com"),
                    PhoneNumber = new PhoneNumber("+1-416-555-0105"),
                    CompanyId = techSolutionsId,
                    Address = new Address(
                        addressLine1: "202 Birch Ln",
                        addressLine2: "Floor 3",
                        city: "Toronto",
                        state: "ON",
                        countryCode: "CA",
                        zipCode: "M5V 2T7"
                    ),
                    TaxId = "CA-GST-987654321",
                    Website = "https://techtrend.com",
                    CreditLimit = 100000.00m,
                    DefaultPaymentTerms = "Net 45",
                    DefaultCurrency = "CAD",
                    CustomerGroupId = GetGroupId(techSolutionsId, "Corporate"),
                    Status = CustomerStatus.Active,
                    ActiveSince = DateTime.UtcNow.AddYears(-1),
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-40),
                    TotalPurchases = 75000.00m,
                    AveragePaymentDays = 35,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-3),
                    IsDeleted = false
                },
                new Customer {
                    Name = "Mark Taylor",
                    Email = new Email("mark.t@example.com"),
                    PhoneNumber = new PhoneNumber("+1-206-555-0119"),
                    CompanyId = techSolutionsId,
                    Address = new Address(
                        addressLine1: "606 Pinecrest Dr",
                        addressLine2: null,
                        city: "Seattle",
                        state: "WA",
                        countryCode: "US",
                        zipCode: "98101"
                    ),
                    TaxId = "US-EIN-7654321",
                    Website = "https://marktaylor.com",
                    CreditLimit = 30000.00m,
                    DefaultPaymentTerms = "Net 30",
                    DefaultCurrency = "USD",
                    CustomerGroupId = GetGroupId(techSolutionsId, "Regular"),
                    Status = CustomerStatus.Active,
                    ActiveSince = DateTime.UtcNow.AddMonths(-7),
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-50),
                    TotalPurchases = 15000.00m,
                    AveragePaymentDays = 25,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-2),
                    IsDeleted = false
                }
            ]);

            // Global Enterprises Customers
            customersToSeed.AddRange([
                new Customer {
                    Name = "Michael Brown",
                    Email = new Email("michael.brown@example.com"),
                    PhoneNumber = new PhoneNumber("+61-2-5550-0106"),
                    CompanyId = globalEnterprisesId,
                    Address = new Address(
                        addressLine1: "303 Cedar Ave",
                        addressLine2: "Level 8",
                        city: "Sydney",
                        state: "NSW",
                        countryCode: "AU",
                        zipCode: "2000"
                    ),
                    TaxId = "AU-GST-123456789",
                    Website = "https://michaelbrown.com",
                    CreditLimit = 40000.00m,
                    DefaultPaymentTerms = "Net 30",
                    DefaultCurrency = "AUD",
                    CustomerGroupId = GetGroupId(globalEnterprisesId, "Premium"),
                    Status = CustomerStatus.Active,
                    ActiveSince = DateTime.UtcNow.AddMonths(-9),
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-35),
                    TotalPurchases = 22000.00m,
                    AveragePaymentDays = 18,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-3),
                    IsDeleted = false
                },
                new Customer {
                    Name = "Global Traders",
                    Email = new Email("finance@globaltraders.com"),
                    PhoneNumber = new PhoneNumber("+65-6555-0107"),
                    CompanyId = globalEnterprisesId,
                    Address = new Address(
                        addressLine1: "404 Spruce St",
                        addressLine2: "Unit 5",
                        city: "Singapore",
                        state: "Singapore",
                        countryCode: "SG",
                        zipCode: "069609"
                    ),
                    TaxId = "SG-GST-456789012",
                    Website = "https://globaltraders.com",
                    CreditLimit = 150000.00m,
                    DefaultPaymentTerms = "Net 60",
                    DefaultCurrency = "SGD",
                    CustomerGroupId = GetGroupId(globalEnterprisesId, "Corporate"),
                    Status = CustomerStatus.Active,
                    ActiveSince = DateTime.UtcNow.AddYears(-1),
                    LastPurchaseDate = DateTime.UtcNow.AddDays(-55),
                    TotalPurchases = 95000.00m,
                    AveragePaymentDays = 50,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-2),
                    IsDeleted = false
                }
            ]);

            // Seed customers
            var existingCustomerEmails = await _dbContext.Customers
                .AsNoTracking()
                .Select(c => c.Email.Value) // Use .Value to get the string
                .ToListAsync(cancellationToken);

            var newCustomers = customersToSeed
               .Where(c => !existingCustomerEmails.Contains(c.Email.Value)) // Compare string values
               .ToList();

            if (newCustomers.Any())
            {
                _dbContext.Customers.AddRange(newCustomers);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Seeded {Count} new customers.", newCustomers.Count);
            }
            else
            {
                _logger.LogInformation("No new customers to seed.");
            }

            // Retrieve/Refresh customers for invoice seeding
            var customers = await _dbContext.Customers
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var customerMap = customers.ToDictionary(c => c.Email.Value, c => c.Id);

            // Seed Customer Contacts
            await SeedCustomerContactsAsync(customerMap, cancellationToken);

            // Seed Customer Notes
            await SeedCustomerNotesAsync(customerMap, cancellationToken);

            // Seed Customer Documents
            await SeedCustomerDocumentsAsync(customerMap, cancellationToken);

            // Seed Users for customers
            await SeedCustomerUsersAsync(customerMap, companyMap, cancellationToken);

            // Seed Tax Types with ALL fields
            await SeedTaxTypesAsync(companyMap, cancellationToken);

            // Seed Invoices and ALL related data
            await SeedInvoicesAsync(customerMap, companyMap, cancellationToken);

            // Seed Recurring Invoices with ALL fields
            await SeedRecurringInvoicesAsync(customerMap, companyMap, cancellationToken);

            _logger.LogInformation("Successfully seeded all data.");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Seeding customers and related data was canceled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding customers and related data: {Message}", ex.Message);
            throw;
        }
    }
    private async Task SeedCustomerGroupsAsync(Dictionary<string, int> companyMap, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Seeding customer groups...");

            var groupsToSeed = new List<CustomerGroup>();

            foreach (var company in companyMap)
            {
                groupsToSeed.AddRange([
                    new CustomerGroup
                {
                    Name = "Premium",
                    Description = "High-value customers with premium benefits",
                    CompanyId = company.Value,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                },
                new CustomerGroup
                {
                    Name = "Corporate",
                    Description = "Corporate and enterprise customers",
                    CompanyId = company.Value,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                },
                new CustomerGroup
                {
                    Name = "Regular",
                    Description = "Regular standard customers",
                    CompanyId = company.Value,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                },
                new CustomerGroup
                {
                    Name = "New",
                    Description = "Newly onboarded customers",
                    CompanyId = company.Value,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                }
                ]);
            }

            var existingGroups = await _dbContext.CustomerGroups
                .Select(g => new { g.CompanyId, g.Name })
                .ToListAsync(cancellationToken);

            var newGroups = groupsToSeed
                .Where(g => !existingGroups.Any(e => e.CompanyId == g.CompanyId && e.Name == g.Name))
                .ToList();

            if (newGroups.Any())
            {
                await _dbContext.CustomerGroups.AddRangeAsync(newGroups, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Seeded {Count} customer groups.", newGroups.Count);
            }
            else
            {
                _logger.LogInformation("No new customer groups to seed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding customer groups");
            throw;
        }
    }
    private async Task SeedCustomerContactsAsync(Dictionary<string, int> customerMap, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Seeding customer contacts...");

            var contactsToSeed = new List<CustomerContact>();

            // Alagappan M contacts
            if (customerMap.TryGetValue("alagappanmk984@gmail.com", out var alagappanId))
            {
                contactsToSeed.Add(new CustomerContact
                {
                    CustomerId = alagappanId,
                    Name = "Alagappan M",
                    Email = "alagappanmk984@gmail.com",
                    PhoneNumber = "+91-98765-43210",
                    Designation = "CEO",
                    IsPrimary = true,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                });
                contactsToSeed.Add(new CustomerContact
                {
                    CustomerId = alagappanId,
                    Name = "Rajesh Kumar",
                    Email = "rajesh@klinfotech.com",
                    PhoneNumber = "+91-98765-12345",
                    Designation = "Finance Manager",
                    IsPrimary = false,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            // Acme Corp contacts
            if (customerMap.TryGetValue("billing@acmecorp.com", out var acmeId))
            {
                contactsToSeed.Add(new CustomerContact
                {
                    CustomerId = acmeId,
                    Name = "John Smith",
                    Email = "john.smith@acmecorp.com",
                    PhoneNumber = "+1-555-123-4567",
                    Designation = "Procurement Director",
                    IsPrimary = true,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            // Sarah Johnson contacts
            if (customerMap.TryGetValue("sarah.j@example.com", out var sarahId))
            {
                contactsToSeed.Add(new CustomerContact
                {
                    CustomerId = sarahId,
                    Name = "Sarah Johnson",
                    Email = "sarah.j@example.com",
                    PhoneNumber = "+1-617-555-0104",
                    Designation = "Owner",
                    IsPrimary = true,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            if (contactsToSeed.Any())
            {
                await _dbContext.CustomerContacts.AddRangeAsync(contactsToSeed, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Seeded {Count} customer contacts.", contactsToSeed.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding customer contacts");
            throw;
        }
    }
    private async Task SeedCustomerNotesAsync(Dictionary<string, int> customerMap, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Seeding customer notes...");

            var notesToSeed = new List<CustomerNote>();

            // Alagappan M notes
            if (customerMap.TryGetValue("alagappanmk984@gmail.com", out var alagappanId))
            {
                notesToSeed.Add(new CustomerNote
                {
                    CustomerId = alagappanId,
                    Note = "Premium customer since 2024. Always pays on time.",
                    Type = NoteType.Internal,
                    IsPinned = true,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-5),
                    IsDeleted = false
                });
                notesToSeed.Add(new CustomerNote
                {
                    CustomerId = alagappanId,
                    Note = "Send quarterly review reports to this customer.",
                    Type = NoteType.Reminder,
                    IsPinned = false,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-2),
                    IsDeleted = false
                });
            }

            // Acme Corp notes
            if (customerMap.TryGetValue("billing@acmecorp.com", out var acmeId))
            {
                notesToSeed.Add(new CustomerNote
                {
                    CustomerId = acmeId,
                    Note = "Corporate customer with special payment terms (Net 60).",
                    Type = NoteType.Internal,
                    IsPinned = true,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-4),
                    IsDeleted = false
                });
            }

            if (notesToSeed.Any())
            {
                await _dbContext.CustomerNotes.AddRangeAsync(notesToSeed, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Seeded {Count} customer notes.", notesToSeed.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding customer notes");
            throw;
        }
    }
    private async Task SeedCustomerDocumentsAsync(Dictionary<string, int> customerMap, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Seeding customer documents...");

            var documentsToSeed = new List<CustomerDocument>();

            // Alagappan M documents
            if (customerMap.TryGetValue("alagappanmk984@gmail.com", out var alagappanId))
            {
                documentsToSeed.Add(new CustomerDocument
                {
                    CustomerId = alagappanId,
                    DocumentName = "GST Certificate.pdf",
                    FileUrl = "/uploads/documents/gst_certificate_alagappan.pdf",
                    FileType = "application/pdf",
                    FileSize = 102400,
                    Type = DocumentType.TaxCertificate,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-5),
                    IsDeleted = false
                });
                documentsToSeed.Add(new CustomerDocument
                {
                    CustomerId = alagappanId,
                    DocumentName = "Business Contract.pdf",
                    FileUrl = "/uploads/documents/contract_alagappan.pdf",
                    FileType = "application/pdf",
                    FileSize = 204800,
                    Type = DocumentType.Contract,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-4),
                    IsDeleted = false
                });
            }

            // Acme Corp documents
            if (customerMap.TryGetValue("billing@acmecorp.com", out var acmeId))
            {
                documentsToSeed.Add(new CustomerDocument
                {
                    CustomerId = acmeId,
                    DocumentName = "Master Service Agreement.pdf",
                    FileUrl = "/uploads/documents/msa_acmecorp.pdf",
                    FileType = "application/pdf",
                    FileSize = 512000,
                    Type = DocumentType.Contract,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-4),
                    IsDeleted = false
                });
            }

            if (documentsToSeed.Any())
            {
                await _dbContext.CustomerDocuments.AddRangeAsync(documentsToSeed, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Seeded {Count} customer documents.", documentsToSeed.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding customer documents");
            throw;
        }
    }
    private async Task SeedCustomerUsersAsync(Dictionary<string, int> customerMap, Dictionary<string, int> companyMap, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Seeding customer users...");

            var customerUsers = new List<(int CustomerId, ApplicationUser User, string Role)>();

            // Alagappan M user
            if (customerMap.TryGetValue("alagappanmk984@gmail.com", out var alagappanId) &&
                companyMap.TryGetValue("KL Infotech", out var klInfotechId))
            {
                customerUsers.Add((
                    alagappanId,
                    new ApplicationUser
                    {
                        UserName = "alagappan.m",
                        Email = "alagappanmk984@gmail.com",
                        FullName = "Alagappan M",
                        EmailConfirmed = true,
                        CompanyId = klInfotechId,
                        CustomerId = alagappanId,
                        CountryCode = "IN",
                        PhoneNumber = "+91-98765-43210",
                        StreetAddress = "123 Maple St, Apt 4B",
                        City = "Chennai",
                        State = "Tamil Nadu",
                        PostalCode = "600001",
                        CreatedDate = DateTime.UtcNow.AddMonths(-5),
                        CreatedBy = "system",
                        IsDeleted = false
                    },
                    AppConstants.Role_Customer
                ));
            }

            // Sarah Johnson user
            if (customerMap.TryGetValue("sarah.j@example.com", out var sarahId) &&
                companyMap.TryGetValue("Tech Solutions Inc.", out var techSolutionsId))
            {
                customerUsers.Add((
                    sarahId,
                    new ApplicationUser
                    {
                        UserName = "sarah.johnson",
                        Email = "sarah.j@example.com",
                        FullName = "Sarah Johnson",
                        EmailConfirmed = true,
                        CompanyId = techSolutionsId,
                        CustomerId = sarahId,
                        CountryCode = "US",
                        PhoneNumber = "+1-617-555-0104",
                        StreetAddress = "101 Elm St, Apt 3B",
                        City = "Boston",
                        State = "MA",
                        PostalCode = "02108",
                        CreatedDate = DateTime.UtcNow.AddMonths(-4),
                        CreatedBy = "system",
                        IsDeleted = false
                    },
                    AppConstants.Role_Customer
                ));
            }

            // Priya Sharma user
            if (customerMap.TryGetValue("priya.sharma@example.com", out var priyaId) &&
                companyMap.TryGetValue("KL Infotech", out klInfotechId))
            {
                customerUsers.Add((
                    priyaId,
                    new ApplicationUser
                    {
                        UserName = "priya.sharma",
                        Email = "priya.sharma@example.com",
                        FullName = "Priya Sharma",
                        EmailConfirmed = true,
                        CompanyId = klInfotechId,
                        CustomerId = priyaId,
                        CountryCode = "IN",
                        PhoneNumber = "+91-99876-54321",
                        StreetAddress = "202 Rosewood St, Bungalow 5",
                        City = "Mumbai",
                        State = "Maharashtra",
                        PostalCode = "400001",
                        CreatedDate = DateTime.UtcNow.AddMonths(-2),
                        CreatedBy = "system",
                        IsDeleted = false
                    },
                    AppConstants.Role_Customer
                ));
            }

            // Rahul Gupta user
            if (customerMap.TryGetValue("rahul.gupta@example.com", out var rahulId) &&
                companyMap.TryGetValue("KL Infotech", out klInfotechId))
            {
                customerUsers.Add((
                    rahulId,
                    new ApplicationUser
                    {
                        UserName = "rahul.gupta",
                        Email = "rahul.gupta@example.com",
                        FullName = "Rahul Gupta",
                        EmailConfirmed = true,
                        CompanyId = klInfotechId,
                        CustomerId = rahulId,
                        CountryCode = "IN",
                        PhoneNumber = "+91-98765-12345",
                        StreetAddress = "303 Jasmine Apt, Tower A, Flat 1204",
                        City = "Bangalore",
                        State = "Karnataka",
                        PostalCode = "560001",
                        CreatedDate = DateTime.UtcNow.AddMonths(-1),
                        CreatedBy = "system",
                        IsDeleted = false
                    },
                    AppConstants.Role_Customer
                ));
            }

            foreach (var item in customerUsers)
            {
                var existingUser = await _userManager.FindByEmailAsync(item.User.Email);
                if (existingUser != null)
                {
                    if (existingUser.CustomerId == null)
                    {
                        existingUser.CustomerId = item.CustomerId;
                        existingUser.UpdatedBy = "system";
                        existingUser.UpdatedDate = DateTime.UtcNow;
                        await _userManager.UpdateAsync(existingUser);
                        _logger.LogInformation("Updated existing user {Email} with CustomerId {CustomerId}", item.User.Email, item.CustomerId);
                    }
                    continue;
                }

                _logger.LogInformation("Creating customer user {Email}...", item.User.Email);
                var creationResult = await _userManager.CreateAsync(item.User, "Customer@123");
                if (!creationResult.Succeeded)
                {
                    _logger.LogError("Failed to create customer user {Email}: {Errors}",
                        item.User.Email,
                        string.Join(", ", creationResult.Errors.Select(e => e.Description)));
                    continue;
                }

                // Assign Customer role
                await _userManager.AddToRoleAsync(item.User, item.Role);
                _logger.LogInformation("Successfully created customer user {Email}", item.User.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding customer users");
            throw;
        }
    }
    private async Task SeedTaxTypesAsync(Dictionary<string, int> companyMap, CancellationToken cancellationToken)
    {
        var taxTypes = new[]
        {
            new TaxType {
                Name = "GST 5%",
                Rate = 5.00m,
                CompanyId = companyMap["KL Infotech"],
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                IsDeleted = false
            },
            new TaxType {
                Name = "GST 12%",
                Rate = 12.00m,
                CompanyId = companyMap["KL Infotech"],
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                IsDeleted = false
            },
            new TaxType {
                Name = "GST 18%",
                Rate = 18.00m,
                CompanyId = companyMap["KL Infotech"],
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                IsDeleted = false
            },
            new TaxType {
                Name = "Sales Tax 8%",
                Rate = 8.00m,
                CompanyId = companyMap["Tech Solutions Inc."],
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-5),
                IsDeleted = false
            },
            new TaxType {
                Name = "VAT 20%",
                Rate = 20.00m,
                CompanyId = companyMap["Global Enterprises"],
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-4),
                IsDeleted = false
            },
            new TaxType {
                Name = "VAT 19%",
                Rate = 19.00m,
                CompanyId = companyMap["Euro Dynamics"],
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddMonths(-3),
                IsDeleted = false
            }
        };

        var existingTaxTypes = await _dbContext.TaxTypes.ToListAsync(cancellationToken);
        var newTaxTypes = taxTypes
            .Where(tt => !existingTaxTypes.Any(et => et.Name == tt.Name && et.CompanyId == tt.CompanyId))
            .ToArray();

        if (newTaxTypes.Any())
        {
            _dbContext.TaxTypes.AddRange(newTaxTypes);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded {Count} new tax types.", newTaxTypes.Length);
        }
    }
    private async Task SeedInvoicesAsync(Dictionary<string, int> customerMap, Dictionary<string, int> companyMap, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding invoices...");

        // First, check if invoices already exist to avoid duplicates
        var existingInvoiceNumbers = await _dbContext.Invoices
            .Where(i => !i.IsDeleted)
            .Select(i => i.InvoiceNumber)
            .ToListAsync(cancellationToken);

        // If invoices already exist, skip seeding
        if (existingInvoiceNumbers.Any())
        {
            _logger.LogInformation("Invoices already exist in the database. Skipping invoice seeding.");
            return;
        }

        var invoices = new List<Invoice>();
        var invoiceItems = new List<InvoiceItem>();
        var invoiceTaxDetails = new List<InvoiceTaxDetail>();
        var invoiceDiscounts = new List<InvoiceDiscount>();
        var invoiceAttachments = new List<InvoiceAttachment>();
        var invoicePayments = new List<InvoicePayment>();
        var invoiceAuditLogs = new List<InvoiceAuditLog>();

        // Helper function to create invoice with ALL fields
        Invoice CreateInvoice(
            string invoiceNumber,
            string customerEmail,
            string companyName,
            DateTime issueDate,
            DateTime dueDate,
            DateTime? sentDate,
            DateTime? paidDate,
            InvoiceStatus status,
            PaymentStatus paymentStatus,
            InvoiceType type,
            decimal subtotal,
            decimal taxTotal,
            decimal discountTotal,
            decimal shippingAmount,
            decimal adjustmentAmount,
            string adjustmentDescription,
            decimal amountPaid,
            decimal amountDue,
            decimal amountRefunded,
            string paymentGateway,
            string paymentTransactionId,
            bool isAutomated,
            int? recurringInvoiceId,
            string sourceSystem,
            string customerNotes,
            string internalNotes,
            string termsAndConditions,
            string footerNote,
            string projectDetail,
            string paymentMethod,
            string paymentTerms,
            int? billingAddressId = null,
            int? shippingAddressId = null,
            string currency = "USD",
            decimal currencyRate = 1.0m)
        {
            var totalAmount = subtotal + taxTotal + shippingAmount + adjustmentAmount - discountTotal;

            return new Invoice
            {
                InvoiceNumber = invoiceNumber,
                IssueDate = issueDate,
                DueDate = dueDate,
                SentDate = sentDate,
                PaidDate = paidDate,
                InvoiceStatus = status,
                PaymentStatus = paymentStatus,
                InvoiceType = type,
                CustomerId = customerMap[customerEmail],
                CompanyId = companyMap[companyName],
                BillingAddressId = billingAddressId,
                ShippingAddressId = shippingAddressId,
                Currency = currency,
                CurrencyRate = currencyRate,
                PONumber = $"PO-{invoiceNumber.Substring(8)}",
                Subtotal = subtotal,
                DiscountTotal = discountTotal,
                TaxTotal = taxTotal,
                ShippingAmount = shippingAmount,
                AdjustmentAmount = adjustmentAmount,
                AdjustmentDescription = adjustmentDescription,
                TotalAmount = totalAmount,
                AmountPaid = amountPaid,
                AmountDue = amountDue,
                AmountRefunded = amountRefunded,
                PaymentGateway = paymentGateway,
                PaymentTransactionId = paymentTransactionId,
                IsAutomated = isAutomated,
                RecurringInvoiceId = recurringInvoiceId,
                SourceSystem = sourceSystem,
                CustomerNotes = customerNotes,
                InternalNotes = internalNotes,
                TermsAndConditions = termsAndConditions,
                FooterNote = footerNote,
                ProjectDetail = projectDetail,
                PaymentMethod = paymentMethod,
                PaymentTerms = paymentTerms,
                CreatedBy = "system",
                CreatedDate = issueDate,
                IsDeleted = false
            };
        }

        // ============================================================
        // INVOICE 1 - Paid Invoice (KL Infotech)
        // ============================================================
        var invoice1 = CreateInvoice(
            invoiceNumber: "INV-2024-0001",
            customerEmail: "alagappanmk984@gmail.com",
            companyName: "KL Infotech",
            issueDate: DateTime.UtcNow.AddDays(-30),
            dueDate: DateTime.UtcNow.AddDays(-15),
            sentDate: DateTime.UtcNow.AddDays(-29),
            paidDate: DateTime.UtcNow.AddDays(-20),
            status: InvoiceStatus.Paid,
            paymentStatus: PaymentStatus.Paid,
            type: InvoiceType.Standard,
            subtotal: 2950.00m,
            taxTotal: 531.00m,
            discountTotal: 150.00m,
            shippingAmount: 0,
            adjustmentAmount: 0,
            adjustmentDescription: string.Empty,
            amountPaid: 3331.00m,
            amountDue: 0,
            amountRefunded: 0,
            paymentGateway: "Stripe",
            paymentTransactionId: "pi_3Qwerty123456",
            isAutomated: false,
            recurringInvoiceId: null,
            sourceSystem: "Manual",
            customerNotes: "Thank you for your business!",
            internalNotes: "Q3 Marketing Campaign - Digital Ad Spend",
            termsAndConditions: "Payment due within 30 days. Late payments subject to 1.5% monthly interest.",
            footerNote: "Please include invoice number with payment.",
            projectDetail: "Q3 Marketing Campaign - Digital Ad Spend",
            paymentMethod: "Bank Transfer",
            paymentTerms: "Net 30",
            currency: "USD"
        );

        // ============================================================
        // INVOICE 2 - Overdue Invoice (Tech Solutions Inc.)
        // ============================================================
        var invoice2 = CreateInvoice(
            invoiceNumber: "INV-2024-0002",
            customerEmail: "sarah.j@example.com",
            companyName: "Tech Solutions Inc.",
            issueDate: DateTime.UtcNow.AddDays(-20),
            dueDate: DateTime.UtcNow.AddDays(-5),
            sentDate: DateTime.UtcNow.AddDays(-19),
            paidDate: null,
            status: InvoiceStatus.Sent,
            paymentStatus: PaymentStatus.Overdue,
            type: InvoiceType.Standard,
            subtotal: 2850.00m,
            taxTotal: 228.00m,
            discountTotal: 313.50m,
            shippingAmount: 0,
            adjustmentAmount: 0,
            adjustmentDescription: string.Empty,
            amountPaid: 0,
            amountDue: 2764.50m,
            amountRefunded: 0,
            paymentGateway: "Stripe",
            paymentTransactionId: string.Empty,
            isAutomated: false,
            recurringInvoiceId: null,
            sourceSystem: "Manual",
            customerNotes: "Thank you for your business!",
            internalNotes: "Mobile App Revamp - UI/UX Design Phase",
            termsAndConditions: "Payment due within 30 days. Late payments subject to 1.5% monthly interest.",
            footerNote: "Please include invoice number with payment.",
            projectDetail: "Mobile App Revamp - UI/UX Design Phase",
            paymentMethod: "Credit Card",
            paymentTerms: "Net 30",
            currency: "USD"
        );

        // ============================================================
        // INVOICE 3 - Pending Invoice (KL Infotech - EUR)
        // ============================================================
        var invoice3 = CreateInvoice(
            invoiceNumber: "INV-2024-0003",
            customerEmail: "billing@acmecorp.com",
            companyName: "KL Infotech",
            issueDate: DateTime.UtcNow.AddDays(-10),
            dueDate: DateTime.UtcNow.AddDays(5),
            sentDate: DateTime.UtcNow.AddDays(-9),
            paidDate: null,
            status: InvoiceStatus.Sent,
            paymentStatus: PaymentStatus.Pending,
            type: InvoiceType.Standard,
            subtotal: 2710.00m,
            taxTotal: 542.00m,
            discountTotal: 298.10m,
            shippingAmount: 0,
            adjustmentAmount: 0,
            adjustmentDescription: string.Empty,
            amountPaid: 0,
            amountDue: 2953.90m,
            amountRefunded: 0,
            paymentGateway: "PayPal",
            paymentTransactionId: string.Empty,
            isAutomated: false,
            recurringInvoiceId: null,
            sourceSystem: "Manual",
            customerNotes: "Thank you for your business!",
            internalNotes: "ERP System Integration - Consultancy Hours",
            termsAndConditions: "Payment due within 30 days. Late payments subject to 1.5% monthly interest.",
            footerNote: "Please include invoice number with payment.",
            projectDetail: "ERP System Integration - Consultancy Hours",
            paymentMethod: "PayPal",
            paymentTerms: "Net 30",
            currency: "EUR",
            currencyRate: 0.92m
        );

        // ============================================================
        // INVOICE 4 - Partially Paid Invoice (Tech Solutions Inc.)
        // ============================================================
        var invoice4 = CreateInvoice(
            invoiceNumber: "INV-2024-0004",
            customerEmail: "accounts@techtrend.com",
            companyName: "Tech Solutions Inc.",
            issueDate: DateTime.UtcNow.AddDays(-25),
            dueDate: DateTime.UtcNow.AddDays(-10),
            sentDate: DateTime.UtcNow.AddDays(-24),
            paidDate: null,
            status: InvoiceStatus.PartiallyPaid,
            paymentStatus: PaymentStatus.PartiallyPaid,
            type: InvoiceType.Standard,
            subtotal: 2750.00m,
            taxTotal: 220.00m,
            discountTotal: 0,
            shippingAmount: 0,
            adjustmentAmount: 0,
            adjustmentDescription: null,
            amountPaid: 1500.00m,
            amountDue: 1470.00m,
            amountRefunded: 0,
            paymentGateway: "Stripe",
            paymentTransactionId: "pi_3Abcdef789012",
            isAutomated: false,
            recurringInvoiceId: null,
            sourceSystem: "Manual",
            customerNotes: "Thank you for your business!",
            internalNotes: "Infrastructure Upgrade - Server & Network",
            termsAndConditions: "Payment due within 30 days. Late payments subject to 1.5% monthly interest.",
            footerNote: "Please include invoice number with payment.",
            projectDetail: "Infrastructure Upgrade - Server & Network",
            paymentMethod: "Bank Transfer",
            paymentTerms: "Net 30",
            currency: "USD"
        );

        var invoice5 = CreateInvoice(
            invoiceNumber: "INV-2024-0005",
            customerEmail: "michael.brown@example.com",
            companyName: "Global Enterprises",
            issueDate: DateTime.UtcNow.AddDays(-15),
            dueDate: DateTime.UtcNow,
            sentDate: DateTime.UtcNow.AddDays(-14),
            paidDate: null,
            status: InvoiceStatus.Sent,
            paymentStatus: PaymentStatus.Overdue,
            type: InvoiceType.Standard,
            subtotal: 4050.00m,
            taxTotal: 810.00m,
            discountTotal: 0,
            shippingAmount: 0,
            adjustmentAmount: 0,
            adjustmentDescription: string.Empty,
            amountPaid: 0,
            amountDue: 4860.00m,
            amountRefunded: 0,
            paymentGateway: "Stripe",
            paymentTransactionId: string.Empty,
            isAutomated: false,
            recurringInvoiceId: null,
            sourceSystem: "Recurring",
            customerNotes: "Thank you for your continued business!",
            internalNotes: "Monthly Cloud Hosting Services - October",
            termsAndConditions: "Payment due upon receipt. Services may be suspended after 15 days overdue.",
            footerNote: "This is a recurring monthly invoice.",
            projectDetail: "Monthly Cloud Hosting Services",
            paymentMethod: "Credit Card",
            paymentTerms: "Due on Receipt",
            currency: "GBP",
            currencyRate: 0.79m
        );

        // ============================================================
        // INVOICE 6 - Pending Invoice (KL Infotech)
        // ============================================================
        var invoice6 = CreateInvoice(
            invoiceNumber: "INV-2024-0006",
            customerEmail: "david.lee@example.com",
            companyName: "KL Infotech",
            issueDate: DateTime.UtcNow.AddDays(-4),
            dueDate: DateTime.UtcNow.AddDays(10),
            sentDate: DateTime.UtcNow.AddDays(-3),
            paidDate: null,
            status: InvoiceStatus.Sent,
            paymentStatus: PaymentStatus.Pending,
            type: InvoiceType.Standard,
            subtotal: 7100.00m,
            taxTotal: 1278.00m,
            discountTotal: 0,
            shippingAmount: 0,
            adjustmentAmount: 0,
            adjustmentDescription: string.Empty,
            amountPaid: 0,
            amountDue: 8378.00m,
            amountRefunded: 0,
            paymentGateway: "Razorpay",
            paymentTransactionId: string.Empty,
            isAutomated: false,
            recurringInvoiceId: null,
            sourceSystem: "Manual",
            customerNotes: "Thank you for your business!",
            internalNotes: "Database Optimization & Performance Tuning",
            termsAndConditions: "Payment due within 30 days. Late payments subject to 1.5% monthly interest.",
            footerNote: "Please include invoice number with payment.",
            projectDetail: "Database Optimization & Performance Tuning",
            paymentMethod: "Bank Transfer",
            paymentTerms: "Net 30",
            currency: "USD"
        );

        invoices.AddRange(new[] { invoice1, invoice2, invoice3, invoice4, invoice5, invoice6 });
        await _dbContext.Invoices.AddRangeAsync(invoices, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // ============================================================
        // INVOICE ITEMS - All invoices have line items
        // ============================================================

        // INVOICE 1 ITEMS (5 items)
        invoiceItems.AddRange(new[]
        {
            new InvoiceItem {
                InvoiceId = invoice1.Id,
                Description = "Web Development Services",
                Quantity = 10,
                UnitPrice = 100.00m,
                Amount = 1000.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 180.00m,
                TotalAmount = 1180.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice1.Id,
                Description = "Frontend Design Package",
                Quantity = 1,
                UnitPrice = 500.00m,
                Amount = 500.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 90.00m,
                TotalAmount = 590.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice1.Id,
                Description = "Database Optimization",
                Quantity = 5,
                UnitPrice = 150.00m,
                Amount = 750.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 135.00m,
                TotalAmount = 885.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice1.Id,
                Description = "API Integration",
                Quantity = 1,
                UnitPrice = 400.00m,
                Amount = 400.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 72.00m,
                TotalAmount = 472.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice1.Id,
                Description = "Consultation Hours",
                Quantity = 3,
                UnitPrice = 100.00m,
                Amount = 300.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 54.00m,
                TotalAmount = 354.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            }
        });

        // INVOICE 2 ITEMS (6 items)
        invoiceItems.AddRange(new[]
        {
            new InvoiceItem {
                InvoiceId = invoice2.Id,
                Description = "Software License (Pro Version)",
                Quantity = 5,
                UnitPrice = 300.00m,
                Amount = 1500.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 120.00m,
                TotalAmount = 1620.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice2.Id,
                Description = "User Training Session",
                Quantity = 1,
                UnitPrice = 250.00m,
                Amount = 250.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 20.00m,
                TotalAmount = 270.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice2.Id,
                Description = "Custom Feature Development",
                Quantity = 1,
                UnitPrice = 350.00m,
                Amount = 350.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 28.00m,
                TotalAmount = 378.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice2.Id,
                Description = "Monthly Support Package",
                Quantity = 1,
                UnitPrice = 200.00m,
                Amount = 200.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 16.00m,
                TotalAmount = 216.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice2.Id,
                Description = "Data Migration Assistance",
                Quantity = 1,
                UnitPrice = 150.00m,
                Amount = 150.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 12.00m,
                TotalAmount = 162.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice2.Id,
                Description = "Configuration Services",
                Quantity = 1,
                UnitPrice = 100.00m,
                Amount = 100.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 8.00m,
                TotalAmount = 108.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            }
        });

        // INVOICE 3 ITEMS (4 items)
        invoiceItems.AddRange(new[]
        {
            new InvoiceItem {
                InvoiceId = invoice3.Id,
                Description = "ERP Consultation",
                Quantity = 10,
                UnitPrice = 150.00m,
                Amount = 1500.00m,
                TaxType = "VAT",
                TaxPercentage = 20.00m,
                TaxAmount = 300.00m,
                TotalAmount = 1800.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice3.Id,
                Description = "System Integration",
                Quantity = 5,
                UnitPrice = 200.00m,
                Amount = 1000.00m,
                TaxType = "VAT",
                TaxPercentage = 20.00m,
                TaxAmount = 200.00m,
                TotalAmount = 1200.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice3.Id,
                Description = "Data Migration",
                Quantity = 1,
                UnitPrice = 120.00m,
                Amount = 120.00m,
                TaxType = "VAT",
                TaxPercentage = 20.00m,
                TaxAmount = 24.00m,
                TotalAmount = 144.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice3.Id,
                Description = "Training Materials",
                Quantity = 1,
                UnitPrice = 90.00m,
                Amount = 90.00m,
                TaxType = "VAT",
                TaxPercentage = 20.00m,
                TaxAmount = 18.00m,
                TotalAmount = 108.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            }
        });

        // INVOICE 4 ITEMS (4 items)
        invoiceItems.AddRange(new[]
        {
            new InvoiceItem {
                InvoiceId = invoice4.Id,
                Description = "Server Hardware",
                Quantity = 2,
                UnitPrice = 800.00m,
                Amount = 1600.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 128.00m,
                TotalAmount = 1728.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice4.Id,
                Description = "Network Equipment",
                Quantity = 3,
                UnitPrice = 250.00m,
                Amount = 750.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 60.00m,
                TotalAmount = 810.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice4.Id,
                Description = "Installation Services",
                Quantity = 1,
                UnitPrice = 200.00m,
                Amount = 200.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 16.00m,
                TotalAmount = 216.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice4.Id,
                Description = "Configuration & Testing",
                Quantity = 1,
                UnitPrice = 200.00m,
                Amount = 200.00m,
                TaxType = "Sales Tax",
                TaxPercentage = 8.00m,
                TaxAmount = 16.00m,
                TotalAmount = 216.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                IsDeleted = false
            }
        });

        // INVOICE 5 ITEMS (4 items)
        invoiceItems.AddRange(new[]
        {
            new InvoiceItem {
                InvoiceId = invoice5.Id,
                Description = "Cloud Hosting - Premium Plan",
                Quantity = 1,
                UnitPrice = 2000.00m,
                Amount = 2000.00m,
                TaxType = "VAT",
                TaxPercentage = 20.00m,
                TaxAmount = 400.00m,
                TotalAmount = 2400.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice5.Id,
                Description = "Data Storage (500GB)",
                Quantity = 1,
                UnitPrice = 800.00m,
                Amount = 800.00m,
                TaxType = "VAT",
                TaxPercentage = 20.00m,
                TaxAmount = 160.00m,
                TotalAmount = 960.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice5.Id,
                Description = "Bandwidth Usage",
                Quantity = 1,
                UnitPrice = 750.00m,
                Amount = 750.00m,
                TaxType = "VAT",
                TaxPercentage = 20.00m,
                TaxAmount = 150.00m,
                TotalAmount = 900.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice5.Id,
                Description = "Technical Support",
                Quantity = 1,
                UnitPrice = 500.00m,
                Amount = 500.00m,
                TaxType = "VAT",
                TaxPercentage = 20.00m,
                TaxAmount = 100.00m,
                TotalAmount = 600.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                IsDeleted = false
            }
        });

        // INVOICE 6 ITEMS (4 items)
        invoiceItems.AddRange(new[]
        {
            new InvoiceItem {
                InvoiceId = invoice6.Id,
                Description = "Database Performance Audit",
                Quantity = 1,
                UnitPrice = 2000.00m,
                Amount = 2000.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 360.00m,
                TotalAmount = 2360.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-4),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice6.Id,
                Description = "Query Optimization",
                Quantity = 20,
                UnitPrice = 150.00m,
                Amount = 3000.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 540.00m,
                TotalAmount = 3540.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-4),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice6.Id,
                Description = "Index Tuning",
                Quantity = 1,
                UnitPrice = 1200.00m,
                Amount = 1200.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 216.00m,
                TotalAmount = 1416.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-4),
                IsDeleted = false
            },
            new InvoiceItem {
                InvoiceId = invoice6.Id,
                Description = "Backup Strategy Review",
                Quantity = 1,
                UnitPrice = 900.00m,
                Amount = 900.00m,
                TaxType = "GST",
                TaxPercentage = 18.00m,
                TaxAmount = 162.00m,
                TotalAmount = 1062.00m,
                IsTaxable = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-4),
                IsDeleted = false
            }
        });

        await _dbContext.InvoiceItems.AddRangeAsync(invoiceItems, cancellationToken);

        // ============================================================
        // TAX DETAILS
        // ============================================================
        invoiceTaxDetails.AddRange(new[]
        {
            new InvoiceTaxDetail {
                InvoiceId = invoice1.Id,
                TaxName = "GST",
                Rate = 18.00m,
                TaxAmount = 531.00m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            },
            new InvoiceTaxDetail {
                InvoiceId = invoice2.Id,
                TaxName = "Sales Tax",
                Rate = 8.00m,
                TaxAmount = 204.00m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceTaxDetail {
                InvoiceId = invoice3.Id,
                TaxName = "VAT",
                Rate = 20.00m,
                TaxAmount = 542.00m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            },
            new InvoiceTaxDetail {
                InvoiceId = invoice4.Id,
                TaxName = "Sales Tax",
                Rate = 8.00m,
                TaxAmount = 220.00m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                IsDeleted = false
            },
            new InvoiceTaxDetail {
                InvoiceId = invoice5.Id,
                TaxName = "VAT",
                Rate = 20.00m,
                TaxAmount = 810.00m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                IsDeleted = false
            },
            new InvoiceTaxDetail {
                InvoiceId = invoice6.Id,
                TaxName = "GST",
                Rate = 18.00m,
                TaxAmount = 1278.00m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-4),
                IsDeleted = false
            }
        });
        await _dbContext.InvoiceTaxDetails.AddRangeAsync(invoiceTaxDetails, cancellationToken);

        // ============================================================
        // DISCOUNTS
        // ============================================================
        invoiceDiscounts.AddRange(new[]
        {
            new InvoiceDiscount {
                InvoiceId = invoice1.Id,
                Description = "Early Payment Discount",
                DiscountType = DiscountType.Percentage,
                Amount = 150.00m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            },
            new InvoiceDiscount {
                InvoiceId = invoice2.Id,
                Description = "Volume Discount",
                DiscountType = DiscountType.Percentage,
                Amount = 313.50m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceDiscount {
                InvoiceId = invoice3.Id,
                Description = "Loyalty Discount",
                DiscountType = DiscountType.Percentage,
                Amount = 298.10m,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false
            }
        });

        await _dbContext.InvoiceDiscounts.AddRangeAsync(invoiceDiscounts, cancellationToken);

        // ============================================================
        // PAYMENTS
        // ============================================================
        invoicePayments.AddRange(new[]
        {
            new InvoicePayment {
                // Identity & References
                PaymentNumber = "PAY-2026-001",
                InvoiceId = invoice1.Id,
                InvoiceNumber = invoice1.InvoiceNumber,
                CustomerId = invoice1.CustomerId,
                CompanyId = invoice1.CompanyId,
                // Payment Details
                Amount = 3331.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-20),
                PaymentMethod = "Bank Transfer",
                PaymentStatus = PaymentStatus.Paid,
                PaymentType = PaymentType.Full,

                // Transaction Details
                TransactionId = "TXN-789012",
                ReferenceNumber = "TRX-001-ABC-123",
                CheckNumber = "",
                BankName = "Standard Chartered",
                CardLast4 = "",
                CardBrand = "",
         
                // Notes & Amounts
                CustomerNotes = "Invoice 1 Full Payment",
                InternalNotes = "System Generated Seed",
                AppliedAmount = 3331.00m, // Usually matches Amount for Full payments
                UnappliedAmount = 0.00m,
                IsRefund = false,

                // Audit & Tracking
                ProcessedBy = "system_admin",
                GatewayResponse = "{}",
                BankAccountId = 1,

                // BaseEntity Fields
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoicePayment {
                // Identity & References
                PaymentNumber = "PAY-2026-002",
                InvoiceId = invoice4.Id,
                InvoiceNumber = invoice2.InvoiceNumber,
                CustomerId = invoice4.CustomerId,
                CompanyId = invoice4.CompanyId,
        
                // Payment Details
                Amount = 1500.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-18),
                PaymentMethod = "Bank Transfer",
                PaymentStatus = PaymentStatus.Paid,
                PaymentType = PaymentType.Partial, // Marked as Partial
        
                // Transaction Details
                TransactionId = "TXN-789013",
                ReferenceNumber = "TRX-002-DEF-456",
                CheckNumber = "",
                BankName = "Standard Chartered",
                CardLast4 = "",
                CardBrand = "",
        
                // Notes & Amounts
                CustomerNotes = "Partial payment for project milestone",
                InternalNotes = "System Generated Seed",
                AppliedAmount = 1500.00m,
                UnappliedAmount = 0.00m,
                IsRefund = false,
        
                // Audit & Tracking
                ProcessedBy = "system_admin",
                GatewayResponse = "{}",
                BankAccountId = 1,
        
                // BaseEntity Fields
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-18),
                IsDeleted = false
            }
        });
        await _dbContext.InvoicePayments.AddRangeAsync(invoicePayments, cancellationToken);

        // ============================================================
        // ATTACHMENTS
        // ============================================================
        invoiceAttachments.AddRange(new[]
        {
            new InvoiceAttachment {
                InvoiceId = invoice1.Id,
                FileName = "INV-001_Contract.pdf",
                FilePath = "/uploads/invoices/INV-001/contract.pdf",
                FileUrl = "https://storage.example.com/invoices/INV-001/contract.pdf",
                ContentType = "application/pdf",
                   FileSize = FileSizeHelper.MB(1.5),
                Description = "Signed Service Contract",
                IsPublic = false,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            },
            new InvoiceAttachment {
                    InvoiceId = invoice1.Id,
                    FileName = "INV-001_Receipt.pdf",
                    FilePath = "/uploads/invoices/INV-001/receipt.pdf",
                    FileUrl = "https://storage.example.com/invoices/INV-001/receipt.pdf",
                    ContentType = "application/pdf",
                    FileSize = FileSizeHelper.MB(1.2),
                    Description = "Payment Receipt",
                    IsPublic = true,
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddDays(-25),
                    IsDeleted = false
                },
            new InvoiceAttachment {
                InvoiceId = invoice2.Id,
                FileName = "INV-002_Design_Specs.pdf",
                FilePath = "/uploads/invoices/INV-002/specs.pdf",
                FileUrl = "https://storage.example.com/invoices/INV-002/specs.pdf",
                ContentType = "application/pdf",
                FileSize = FileSizeHelper.MB(1.5),
                Description = "UI/UX Design Specifications",
                IsPublic = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceAttachment {
                InvoiceId = invoice3.Id,
                FileName = "INV-003_Consultancy_Report.pdf",
                FilePath = "/uploads/invoices/INV-003/report.pdf",
                FileUrl = "https://storage.example.com/invoices/INV-003/report.pdf",
                ContentType = "application/pdf",
             FileSize = FileSizeHelper.MB(1.8),
                Description = "ERP Consultancy Report",
                IsPublic = true,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-9),
                IsDeleted = false
            },
            new InvoiceAttachment {
                InvoiceId = invoice5.Id,
                FileName = "INV-005_Cloud_Agreement.pdf",
                FilePath = "/uploads/invoices/INV-005/agreement.pdf",
                FileUrl = "https://storage.example.com/invoices/INV-005/agreement.pdf",
                ContentType = "application/pdf",
                FileSize = FileSizeHelper.GB(1.5),
                Description = "Cloud Hosting Service Agreement",
                IsPublic = false,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-14),
                IsDeleted = false
            }
        });
        await _dbContext.InvoiceAttachments.AddRangeAsync(invoiceAttachments, cancellationToken);

        // ============================================================
        // AUDIT LOGS
        // ============================================================
        invoiceAuditLogs.AddRange(new[]
        {
            new InvoiceAuditLog {
                InvoiceId = invoice1.Id,
                Action = "Created",
                Description = "Invoice created manually",
                Changes = JsonSerializer.Serialize(new {
                    createdBy = "system",
                    createdDate = DateTime.UtcNow.AddDays(-30)
                }),
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            },
            new InvoiceAuditLog {
                InvoiceId = invoice1.Id,
                Action = "Sent",
                Description = "Invoice sent to customer",
                Changes = JsonSerializer.Serialize(new {
                    sentDate = DateTime.UtcNow.AddDays(-29),
                    method = "Email"
                }),
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-29),
                IsDeleted = false
            },
            new InvoiceAuditLog {
                InvoiceId = invoice1.Id,
                Action = "Paid",
                Description = "Payment received in full",
                Changes = JsonSerializer.Serialize(new {
                    amount = 3095.00m,
                    paymentMethod = "Bank Transfer",
                    transactionId = "TRX-001-ABC-123"
                }),
                IpAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-25),
                IsDeleted = false
            },
            new InvoiceAuditLog {
                InvoiceId = invoice2.Id,
                Action = "Created",
                Description = "Invoice created manually",
                Changes = JsonSerializer.Serialize(new {
                    createdBy = "system",
                    createdDate = DateTime.UtcNow.AddDays(-20)
                }),
                IpAddress = "192.168.1.101",
                UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36",
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            },
            new InvoiceAuditLog {
                InvoiceId = invoice2.Id,
                Action = "Overdue",
                Description = "Invoice became overdue",
                Changes = JsonSerializer.Serialize(new {
                    dueDate = DateTime.UtcNow.AddDays(-5),
                    amountDue = 2821.50m
                }),
                IpAddress = "192.168.1.101",
                UserAgent = "system",
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false
            }
        });
        await _dbContext.InvoiceAuditLogs.AddRangeAsync(invoiceAuditLogs, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} invoices with ALL related data.", invoices.Count);
    }
    private async Task SeedRecurringInvoicesAsync(Dictionary<string, int> customerMap, Dictionary<string, int> companyMap, CancellationToken cancellationToken)
    {
        // Get source invoices
        var sourceInvoices = await _dbContext.Invoices
            .Where(i => i.InvoiceNumber == "INV-2024-0005")
            .ToListAsync(cancellationToken);

        var recurringInvoices = new List<RecurringInvoice>();
        var recurringInstances = new List<RecurringInvoiceInstance>();
        var recurringAuditLogs = new List<RecurringInvoiceAuditLog>();
        var recurringInvoiceMap = new Dictionary<string, RecurringInvoice>(); // To track by name

        // Create recurring invoices with ALL fields from InvoiceHeaderBase + RecurringInvoice specific fields
        var recurringInvoice1 = new RecurringInvoice
        {
            Name = "Monthly Cloud Hosting - Global Enterprises",
            Description = "Monthly cloud hosting and infrastructure services including 24/7 monitoring and support",
            CustomerId = customerMap["michael.brown@example.com"],
            CompanyId = companyMap["Global Enterprises"],
            BillingAddressId = null,
            ShippingAddressId = null,
            Currency = "GBP",
            CurrencyRate = 0.79m,
            PONumber = "PO-REC-001",
            Subtotal = 4050.00m,
            DiscountTotal = 0,
            TaxTotal = 405.00m,
            ShippingAmount = 0,
            TotalAmount = 4455.00m,
            CustomerNotes = "Monthly hosting services - thank you for your business!",
            InternalNotes = "Auto-generated recurring invoice for hosting services",
            TermsAndConditions = "Services billed monthly in advance. Cancellation requires 30 days notice.",
            FooterNote = "This is a recurring monthly invoice - please set up automatic payments",
            ProjectDetail = "Cloud Infrastructure Hosting",
            PaymentMethod = "Credit Card",
            PaymentTerms = "Due on Receipt",

            // Recurring-specific fields
            Frequency = RecurringFrequency.Monthly,
            FrequencyInterval = 1,
            DayOfMonth = 1,
            DayOfWeek = null,
            WeekOfMonth = null,
            MonthOfYear = null,
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow.AddYears(1),
            PausedDate = null,
            CancelledDate = null,
            MaxOccurrences = 12,
            OccurrencesGenerated = 1,
            NextInvoiceDate = DateTime.UtcNow.AddMonths(1),
            LastInvoiceDate = DateTime.UtcNow.AddMonths(-1),
            Status = RecurringInvoiceStatus.Active,
            GenerateInAdvanceDays = 3,
            AutoSend = true,
            AutoEmail = true,
            AutoCharge = false,
            ReminderBeforeDue = true,
            ReminderDaysBefore = 5,
            SourceInvoiceId = sourceInvoices.FirstOrDefault()?.Id,

            // Override fields
            OverridePONumber = null,
            OverrideCustomerNotes = "This is your monthly hosting invoice",
            OverrideTermsAndConditions = null,
            OverrideFooterNote = "Thank you for being a valued customer",
            OverrideProjectDetail = null,
            OverridePaymentMethod = null,
            OverridePaymentTerms = 0,
            OverrideShippingAmount = null,
            OverrideAdjustmentAmount = null,
            OverrideAdjustmentDescription = null,

            CreatedBy = "system",
            CreatedDate = DateTime.UtcNow.AddMonths(-1),
            IsDeleted = false
        };

        var recurringInvoice2 = new RecurringInvoice
        {
            Name = "Monthly Retainer - Tech Solutions",
            Description = "Monthly consulting retainer for ongoing technical support and advisory services",
            CustomerId = customerMap["sarah.j@example.com"],
            CompanyId = companyMap["Tech Solutions Inc."],
            BillingAddressId = null,
            ShippingAddressId = null,
            Currency = "USD",
            CurrencyRate = 1.0m,
            PONumber = "PO-RET-001",
            Subtotal = 5000.00m,
            DiscountTotal = 0,
            TaxTotal = 400.00m,
            ShippingAmount = 0,
            TotalAmount = 5400.00m,
            CustomerNotes = "Monthly retainer for consulting services",
            InternalNotes = "20 hours/month retainer package",
            TermsAndConditions = "Services billed monthly. Unused hours expire at month end.",
            FooterNote = "Please track your hours in the portal",
            ProjectDetail = "Technical Advisory Services",
            PaymentMethod = "Bank Transfer",
            PaymentTerms = "Net 15",

            // Recurring-specific fields
            Frequency = RecurringFrequency.Monthly,
            FrequencyInterval = 1,
            DayOfMonth = 15,
            DayOfWeek = null,
            WeekOfMonth = null,
            MonthOfYear = null,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            PausedDate = null,
            CancelledDate = null,
            MaxOccurrences = 6,
            OccurrencesGenerated = 0,
            NextInvoiceDate = DateTime.UtcNow.AddDays(15),
            LastInvoiceDate = null,
            Status = RecurringInvoiceStatus.Active,
            GenerateInAdvanceDays = 2,
            AutoSend = true,
            AutoEmail = true,
            AutoCharge = false,
            ReminderBeforeDue = false,
            ReminderDaysBefore = 3,
            SourceInvoiceId = null,

            // Override fields
            OverridePONumber = "PO-RET-2024",
            OverrideCustomerNotes = "Monthly retainer invoice - please remit payment within 15 days",
            OverrideTermsAndConditions = "Retainer hours must be used within the month",
            OverrideFooterNote = "Thank you for your continued partnership",
            OverrideProjectDetail = "Technical Advisory Retainer",
            OverridePaymentMethod = "Wire Transfer",
            OverridePaymentTerms = 15,
            OverrideShippingAmount = null,
            OverrideAdjustmentAmount = null,
            OverrideAdjustmentDescription = null,

            CreatedBy = "system",
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        var recurringInvoice3 = new RecurringInvoice
        {
            Name = "Quarterly SEO Services - KL Infotech",
            Description = "Quarterly search engine optimization services including keyword research, content optimization, and reporting",
            CustomerId = customerMap["alagappanmk984@gmail.com"],
            CompanyId = companyMap["KL Infotech"],
            BillingAddressId = null,
            ShippingAddressId = null,
            Currency = "USD",
            CurrencyRate = 1.0m,
            PONumber = "PO-SEO-001",
            Subtotal = 3000.00m,
            DiscountTotal = 0,
            TaxTotal = 540.00m,
            ShippingAmount = 0,
            TotalAmount = 3540.00m,
            CustomerNotes = "Quarterly SEO package - includes monthly reporting",
            InternalNotes = "Q1-Q4 SEO services",
            TermsAndConditions = "Services billed quarterly in advance. 30-day cancellation notice required.",
            FooterNote = "SEO reports available in client portal",
            ProjectDetail = "SEO Optimization Services",
            PaymentMethod = "Bank Transfer",
            PaymentTerms = "Net 30",

            // Recurring-specific fields
            Frequency = RecurringFrequency.Quarterly,
            FrequencyInterval = 1,
            DayOfMonth = 1,
            DayOfWeek = null,
            WeekOfMonth = null,
            MonthOfYear = 1,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(2),
            PausedDate = null,
            CancelledDate = null,
            MaxOccurrences = 8,
            OccurrencesGenerated = 0,
            NextInvoiceDate = DateTime.UtcNow.AddMonths(3),
            LastInvoiceDate = null,
            Status = RecurringInvoiceStatus.Draft,
            GenerateInAdvanceDays = 7,
            AutoSend = false,
            AutoEmail = false,
            AutoCharge = false,
            ReminderBeforeDue = false,
            ReminderDaysBefore = 5,
            SourceInvoiceId = null,

            // Override fields
            OverridePONumber = null,
            OverrideCustomerNotes = "Your quarterly SEO invoice is attached",
            OverrideTermsAndConditions = null,
            OverrideFooterNote = "Thank you for choosing us for your SEO needs",
            OverrideProjectDetail = null,
            OverridePaymentMethod = null,
            OverridePaymentTerms = null,
            OverrideShippingAmount = null,
            OverrideAdjustmentAmount = null,
            OverrideAdjustmentDescription = null,

            CreatedBy = "system",
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        recurringInvoices.AddRange(new[] { recurringInvoice1, recurringInvoice2, recurringInvoice3 });

        // Track by name for later reference
        foreach (var inv in recurringInvoices)
        {
            recurringInvoiceMap[inv.Name] = inv;
        }

        // Check for existing recurring invoices and add only new ones
        foreach (var recurring in recurringInvoices)
        {
            var exists = await _dbContext.RecurringInvoices
                .AnyAsync(r => r.Name == recurring.Name && r.CompanyId == recurring.CompanyId, cancellationToken);

            if (!exists)
            {
                _dbContext.RecurringInvoices.Add(recurring);
            }
        }
        // Save to generate IDs
        await _dbContext.SaveChangesAsync(cancellationToken);


        // NOW create audit logs with the actual IDs
        var auditLogs = new List<RecurringInvoiceAuditLog>();

        // Now create audit logs for invoices that were actually added
        foreach (var recurring in recurringInvoices.Where(r => r.Id > 0))
        {
            if (recurring.Name == "Monthly Cloud Hosting - Global Enterprises")
            {
                recurringAuditLogs.AddRange(new[]
                {
                new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurring.Id,
                    Action = "Created",
                    Description = "Recurring invoice template created",
                    Changes = JsonSerializer.Serialize(new { createdBy = "system", frequency = "Monthly", amount = 4455.00m }),
                    IpAddress = "192.168.1.100",
                    UserAgent = "system",
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-1),
                    IsDeleted = false
                },
                new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurring.Id,
                    Action = "Activated",
                    Description = "Recurring invoice activated",
                    Changes = JsonSerializer.Serialize(new { activatedDate = DateTime.UtcNow.AddMonths(-1), nextInvoiceDate = DateTime.UtcNow.AddMonths(1) }),
                    IpAddress = "192.168.1.100",
                    UserAgent = "system",
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddMonths(-1),
                    IsDeleted = false
                },
                new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurring.Id,
                    Action = "Generated",
                    Description = "Invoice generated successfully",
                    Changes = JsonSerializer.Serialize(new { invoiceNumber = "INV-2024-0005", amount = 4455.00m, sequence = 1 }),
                    IpAddress = "192.168.1.100",
                    UserAgent = "system",
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow.AddDays(-15),
                    IsDeleted = false
                }
            });
            }
            else if (recurring.Name == "Monthly Retainer - Tech Solutions")
            {
                recurringAuditLogs.Add(new RecurringInvoiceAuditLog
                {
                    RecurringInvoiceId = recurring.Id,
                    Action = "Created",
                    Description = "Recurring invoice template created",
                    Changes = JsonSerializer.Serialize(new { createdBy = "system", frequency = "Monthly", amount = 5400.00m }),
                    IpAddress = "192.168.1.101",
                    UserAgent = "system",
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
        }

        // Create recurring instances for existing generated invoices
        var generatedInvoice = await _dbContext.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceNumber == "INV-2024-0005", cancellationToken);

        if (generatedInvoice != null)
        {
            var recurringTemplate = recurringInvoiceMap.GetValueOrDefault("Monthly Cloud Hosting - Global Enterprises");

            if (recurringTemplate != null && recurringTemplate.Id > 0)
            {
                // Update the invoice with the recurring ID
                generatedInvoice.RecurringInvoiceId = recurringTemplate.Id;
                generatedInvoice.UpdatedBy = "system";
                generatedInvoice.UpdatedDate = DateTime.UtcNow;
                _dbContext.Invoices.Update(generatedInvoice);

                var instance = new RecurringInvoiceInstance
                {
                    RecurringInvoiceId = recurringTemplate.Id,
                    InvoiceId = generatedInvoice.Id,
                    GeneratedDate = generatedInvoice.IssueDate,
                    ScheduledGenerationDate = generatedInvoice.IssueDate.AddDays(-3),
                    SequenceNumber = 1,
                    GeneratedInvoiceNumber = generatedInvoice.InvoiceNumber,
                    Amount = generatedInvoice.TotalAmount,
                    Notes = "First recurring generation - successful",
                    GenerationStatus = GenerationStatus.Success,
                    ErrorMessage = null,
                    RetryCount = 0,
                    CreatedBy = "system",
                    CreatedDate = generatedInvoice.IssueDate,
                    IsDeleted = false
                };

                var exists = await _dbContext.RecurringInvoiceInstances
                    .AnyAsync(i => i.RecurringInvoiceId == instance.RecurringInvoiceId &&
                                   i.InvoiceId == instance.InvoiceId, cancellationToken);

                if (!exists)
                {
                    recurringInstances.Add(instance);
                }
            }
        }

        // Create additional instances for testing
        if (recurringInstances.Any())
        {
            await _dbContext.RecurringInvoiceInstances.AddRangeAsync(recurringInstances, cancellationToken);
        }

        // Add audit logs (they now have valid RecurringInvoiceId values)
        if (auditLogs.Any())
        {
            await _dbContext.RecurringInvoiceAuditLogs.AddRangeAsync(auditLogs, cancellationToken);
        }

        // Final save
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} recurring invoices with related data.",
            recurringInvoices.Count(r => r.Id > 0));
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
