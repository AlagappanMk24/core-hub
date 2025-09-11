using Core_API.Application.Common.Constants;
using Core_API.Application.Contracts.Services;
using Core_API.Domain.Entities;
using Core_API.Domain.Entities.Identity;
using Core_API.Domain.Enums;
using Core_API.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        private async Task SeedCompaniesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Seeding companies...");
                var adminEmail = "alagappanmk98@gmail.com";
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
                        Address = new Address
                        {
                            Address1 = "123 Tech Park",
                            City = "Chennai",
                            State = "Tamil Nadu",
                            Country = "India",
                            ZipCode = "600100"
                        },
                        Email = "contact@klinfotech.com",
                        PhoneNumber = "+91-44-12345678",
                        CreatedByUserId = adminUser.Id,
                        IsDeleted = false
                    },
                    new Company
                    {
                        Name = "Tech Solutions Inc.",
                        TaxId = "EIN987654321",
                        Address = new Address
                        {
                            Address1 = "456 Innovation Drive",
                            City = "San Francisco",
                            State = "California",
                            Country = "USA",
                            ZipCode = "94105"
                        },
                        Email = "info@techsolutions.com",
                        PhoneNumber = "+1-415-555-1234",
                        CreatedByUserId = adminUser.Id,
                        IsDeleted = false
                    },
                    new Company
                    {
                        Name = "Global Enterprises",
                        TaxId = "VAT456789123",
                        Address = new Address
                        {
                            Address1 = "789 Business Avenue",
                            City = "London",
                            Country = "UK",
                            ZipCode = "EC1A 1BB"
                        },
                        Email = "support@globalenterprises.co.uk",
                        PhoneNumber = "+44-20-1234-5678",
                        CreatedByUserId = adminUser.Id,
                        IsDeleted = false
                    },
                    new Company
                    {
                        Name = "Innovate Tech Ltd",
                        TaxId = "GSTIN9876543210",
                        Address = new Address
                        {
                            Address1 = "101 Future Road",
                            City = "Bangalore",
                            State = "Karnataka",
                            Country = "India",
                            ZipCode = "560001"
                        },
                        Email = "contact@innovatetech.com",
                        PhoneNumber = "+91-80-98765432",
                        CreatedByUserId = adminUser.Id,
                        IsDeleted = false
                    },
                    new Company
                    {
                        Name = "Bright Future Corp",
                        TaxId = "EIN123456789",
                        Address = new Address
                        {
                            Address1 = "202 Progress Street",
                            City = "New York",
                            State = "NY",
                            Country = "USA",
                            ZipCode = "10002"
                        },
                        Email = "info@brightfuturecorp.com",
                        PhoneNumber = "+1-212-555-6789",
                        CreatedByUserId = adminUser.Id,
                        IsDeleted = false
                    },
                    new Company
                    {
                        Name = "Euro Dynamics",
                        TaxId = "VAT789123456",
                        Address = new Address
                        {
                            Address1 = "303 Enterprise Lane",
                            City = "Berlin",
                            Country = "Germany",
                            ZipCode = "10115"
                        },
                        Email = "support@eurodynamics.de",
                        PhoneNumber = "+49-30-12345678",
                        CreatedByUserId = adminUser.Id,
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
                    .ToListAsync(cancellationToken);

                if (companies.Count == 0)
                {
                    _logger.LogError("No companies found in the database. Cannot seed customers or related data.");
                    throw new InvalidOperationException("No companies found in the database.");
                }

                // Map company names to IDs for easier reference
                var companyMap = companies.ToDictionary(c => c.Name, c => c.Id);

                // Seed Customers
                var customersToSeed = new List<Customer>
                {
                    // KL Infotech - 8 Customers
                    new() {
                        Name = "Alaga Muthu",
                        Email = "alagappanmuthukumar1998@gmail.com",
                        PhoneNumber = "555-0101",
                        CompanyId = companyMap["KL Infotech"],
                        Address = new Address { Address1 = "123 Maple St", Address2 = "Apt 4B", City = "Springfield", State = "IL", Country = "USA", ZipCode = "62701" }
                    },
                    new() {
                        Name = "Jane Smith",
                        Email = "jane.smith@example.com",
                        PhoneNumber = "555-0102",
                        CompanyId = companyMap["KL Infotech"],
                        Address = new Address { Address1 = "456 Oak Ave", Address2 = "Suite 201", City = "London", State = "Greater London", Country = "UK", ZipCode = "SW1A 1AA" }
                    },
                    new() {
                        Name = "Acme Corp",
                        Email = "billing@acmecorp.com",
                        PhoneNumber = "555-0103",
                        CompanyId = companyMap["KL Infotech"],
                        Address = new Address { Address1 = "789 Pine Rd", Address2 = "", City = "New York", State = "NY", Country = "USA", ZipCode = "10001" }
                    },
                    new() {
                        Name = "David Lee",
                        Email = "david.lee@example.com",
                        PhoneNumber = "555-0114",
                        CompanyId = companyMap["KL Infotech"],
                        Address = new Address { Address1 = "101 Blossom Blvd", City = "Chennai", State = "Tamil Nadu", Country = "India", ZipCode = "600001" }
                    },
                    new() {
                        Name = "Priya Sharma",
                        Email = "priya.sharma@example.com",
                        PhoneNumber = "555-0115",
                        CompanyId = companyMap["KL Infotech"],
                        Address = new Address { Address1 = "202 Rosewood St", City = "Mumbai", State = "Maharashtra", Country = "India", ZipCode = "400001" }
                    },
                    new() {
                        Name = "Rahul Gupta",
                        Email = "rahul.gupta@example.com",
                        PhoneNumber = "555-0116",
                        CompanyId = companyMap["KL Infotech"],
                        Address = new Address { Address1 = "303 Jasmine Apt", City = "Bangalore", State = "Karnataka", Country = "India", ZipCode = "560001" }
                    },
                    new() {
                        Name = "Sophie Dubois",
                        Email = "sophie.d@example.com",
                        PhoneNumber = "555-0117",
                        CompanyId = companyMap["KL Infotech"],
                        Address = new Address { Address1 = "404 Lavender Ln", City = "Paris", Country = "France", ZipCode = "75001" }
                    },
                    new() {
                        Name = "Kenji Tanaka",
                        Email = "kenji.t@example.com",
                        PhoneNumber = "555-0118",
                        CompanyId = companyMap["KL Infotech"],
                        Address = new Address { Address1 = "505 Cherry Blossum St", City = "Tokyo", Country = "Japan", ZipCode = "100-0001" }
                    },

                    // Tech Solutions Inc. - 8 Customers
                    new() {
                        Name = "Sarah Johnson",
                        Email = "sarah.j@example.com",
                        PhoneNumber = "555-0104",
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Address = new Address { Address1 = "101 Elm St", Address2 = "", City = "Boston", State = "MA", Country = "USA", ZipCode = "02108" }
                    },
                    new() {
                        Name = "TechTrend Ltd",
                        Email = "accounts@techtrend.com",
                        PhoneNumber = "555-0105",
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Address = new Address { Address1 = "202 Birch Ln", Address2 = "Floor 3", City = "Toronto", State = "ON", Country = "Canada", ZipCode = "M5V 2T7" }
                    },
                    new() {
                        Name = "Mark Taylor",
                        Email = "mark.t@example.com",
                        PhoneNumber = "555-0119",
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Address = new Address { Address1 = "606 Pinecrest Dr", City = "Seattle", State = "WA", Country = "USA", ZipCode = "98101" }
                    },
                    new() {
                        Name = "Anna Garcia",
                        Email = "anna.g@example.com",
                        PhoneNumber = "555-0120",
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Address = new Address { Address1 = "707 Ocean Blvd", City = "Los Angeles", State = "CA", Country = "USA", ZipCode = "90001" }
                    },
                    new() {
                        Name = "Cybernetics Corp",
                        Email = "contact@cybernetics.com",
                        PhoneNumber = "555-0121",
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Address = new Address { Address1 = "808 Silicon Valley", City = "San Jose", State = "CA", Country = "USA", ZipCode = "95113" }
                    },
                    new() {
                        Name = "Lucas White",
                        Email = "lucas.w@example.com",
                        PhoneNumber = "555-0122",
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Address = new Address { Address1 = "909 Innovation Way", City = "Austin", State = "TX", Country = "USA", ZipCode = "78701" }
                    },
                    new() {
                        Name = "Global Software Inc.",
                        Email = "sales@globalsoftware.com",
                        PhoneNumber = "555-0123",
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Address = new Address { Address1 = "111 Tech Plaza", City = "Dublin", Country = "Ireland", ZipCode = "D02 XY01" }
                    },
                    new() {
                        Name = "Lena Schmidt",
                        Email = "lena.s@example.com",
                        PhoneNumber = "555-0124",
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Address = new Address { Address1 = "222 Innovation Allee", City = "Munich", Country = "Germany", ZipCode = "80331" }
                    },

                    // Global Enterprises - 8 Customers
                    new() {
                        Name = "Michael Brown",
                        Email = "michael.brown@example.com",
                        PhoneNumber = "555-0106",
                        CompanyId = companyMap["Global Enterprises"],
                        Address = new Address { Address1 = "303 Cedar Ave", Address2 = "", City = "Sydney", State = "NSW", Country = "Australia", ZipCode = "2000" }
                    },
                    new() {
                        Name = "Global Traders",
                        Email = "finance@globaltraders.com",
                        PhoneNumber = "555-0107",
                        CompanyId = companyMap["Global Enterprises"],
                        Address = new Address { Address1 = "404 Spruce St", Address2 = "Unit 5", City = "Singapore", State = "", Country = "Singapore", ZipCode = "069609" }
                    },
                    new() {
                        Name = "Liam Miller",
                        Email = "liam.m@example.com",
                        PhoneNumber = "555-0125",
                        CompanyId = companyMap["Global Enterprises"],
                        Address = new Address { Address1 = "333 World Trade Ctr", City = "Dubai", Country = "UAE", ZipCode = "00000" }
                    },
                    new() {
                        Name = "Olivia Wilson",
                        Email = "olivia.w@example.com",
                        PhoneNumber = "555-0126",
                        CompanyId = companyMap["Global Enterprises"],
                        Address = new Address { Address1 = "444 International Dr", City = "Hong Kong", Country = "Hong Kong", ZipCode = "00000" }
                    },
                    new() {
                        Name = "TransWorld Holdings",
                        Email = "invest@transworld.com",
                        PhoneNumber = "555-0127",
                        CompanyId = companyMap["Global Enterprises"],
                        Address = new Address { Address1 = "555 Global Plaza", City = "Shanghai", Country = "China", ZipCode = "200000" }
                    },
                    new() {
                        Name = "Noah Jones",
                        Email = "noah.j@example.com",
                        PhoneNumber = "555-0128",
                        CompanyId = companyMap["Global Enterprises"],
                        Address = new Address { Address1 = "666 Liberty Blvd", City = "Rio de Janeiro", Country = "Brazil", ZipCode = "20000-000" }
                    },
                    new() {
                        Name = "Isabella Davis",
                        Email = "isabella.d@example.com",
                        PhoneNumber = "555-0129",
                        CompanyId = companyMap["Global Enterprises"],
                        Address = new Address { Address1 = "777 Market St", City = "Cape Town", Country = "South Africa", ZipCode = "8001" }
                    },
                    new() {
                        Name = "Atlas Ventures",
                        Email = "partners@atlasventures.com",
                        PhoneNumber = "555-0130",
                        CompanyId = companyMap["Global Enterprises"],
                        Address = new Address { Address1 = "888 Capital Square", City = "Zurich", Country = "Switzerland", ZipCode = "8001" }
                    },

                    // Innovate Tech Ltd - 8 Customers
                    new() {
                        Name = "Emily Davis",
                        Email = "emily.davis@example.com",
                        PhoneNumber = "555-0108",
                        CompanyId = companyMap["Innovate Tech Ltd"], // Corrected CompanyId
                        Address = new Address { Address1 = "505 Walnut Rd", Address2 = "", City = "San Francisco", State = "CA", Country = "USA", ZipCode = "94103" }
                    },
                    new() {
                        Name = "Innovate Designs",
                        Email = "billing@innovatedesigns.com",
                        PhoneNumber = "555-0109",
                        CompanyId = companyMap["Innovate Tech Ltd"], // Corrected CompanyId
                        Address = new Address { Address1 = "606 Chestnut Dr", Address2 = "Suite 101", City = "Berlin", State = "", Country = "Germany", ZipCode = "10115" }
                    },
                    new() {
                        Name = "Daniel King",
                        Email = "daniel.k@example.com",
                        PhoneNumber = "555-0131",
                        CompanyId = companyMap["Innovate Tech Ltd"],
                        Address = new Address { Address1 = "999 Tech Blvd", City = "Hyderabad", State = "Telangana", Country = "India", ZipCode = "500081" }
                    },
                    new() {
                        Name = "Mia Clark",
                        Email = "mia.c@example.com",
                        PhoneNumber = "555-0132",
                        CompanyId = companyMap["Innovate Tech Ltd"],
                        Address = new Address { Address1 = "111 Data Drive", City = "Pune", State = "Maharashtra", Country = "India", ZipCode = "411001" }
                    },
                    new() {
                        Name = "Futuristic Systems",
                        Email = "contact@futuristicsystems.com",
                        PhoneNumber = "555-0133",
                        CompanyId = companyMap["Innovate Tech Ltd"],
                        Address = new Address { Address1 = "222 Innovation Hub", City = "Gurgaon", State = "Haryana", Country = "India", ZipCode = "122001" }
                    },
                    new() {
                        Name = "Chloe Green",
                        Email = "chloe.g@example.com",
                        PhoneNumber = "555-0134",
                        CompanyId = companyMap["Innovate Tech Ltd"],
                        Address = new Address { Address1 = "333 Digital Rd", City = "Singapore", Country = "Singapore", ZipCode = "069180" }
                    },
                    new() {
                        Name = "Advanced Solutions Co.",
                        Email = "info@advancedsolutions.com",
                        PhoneNumber = "555-0135",
                        CompanyId = companyMap["Innovate Tech Ltd"],
                        Address = new Address { Address1 = "444 Logic Lane", City = "Sydney", Country = "Australia", ZipCode = "2000" }
                    },
                    new() {
                        Name = "Jackson Hall",
                        Email = "jackson.h@example.com",
                        PhoneNumber = "555-0136",
                        CompanyId = companyMap["Innovate Tech Ltd"],
                        Address = new Address { Address1 = "555 Creative Circle", City = "Melbourne", Country = "Australia", ZipCode = "3000" }
                    },

                    // Bright Future Corp - 8 Customers
                    new() {
                        Name = "Robert Wilson",
                        Email = "robert.w@example.com",
                        PhoneNumber = "555-0110",
                        CompanyId = companyMap["Bright Future Corp"],
                        Address = new Address { Address1 = "707 Sycamore Ave", Address2 = "", City = "Chicago", State = "IL", Country = "USA", ZipCode = "60614" }
                    },
                    new() {
                        Name = "Bright Solutions",
                        Email = "accounts@brightsolutions.com",
                        PhoneNumber = "555-0111",
                        CompanyId = companyMap["Bright Future Corp"],
                        Address = new Address { Address1 = "808 Magnolia St", Address2 = "", City = "Paris", State = "", Country = "France", ZipCode = "75001" }
                    },
                    new() {
                        Name = "Sophia Lee",
                        Email = "sophia.l@example.com",
                        PhoneNumber = "555-0137",
                        CompanyId = companyMap["Bright Future Corp"],
                        Address = new Address { Address1 = "666 Innovation Way", City = "Washington DC", State = "DC", Country = "USA", ZipCode = "20001" }
                    },
                    new() {
                        Name = "Ethan King",
                        Email = "ethan.k@example.com",
                        PhoneNumber = "555-0138",
                        CompanyId = companyMap["Bright Future Corp"],
                        Address = new Address { Address1 = "777 Progress Rd", City = "Miami", State = "FL", Country = "USA", ZipCode = "33101" }
                    },
                    new() {
                        Name = "Future Innovations LLC",
                        Email = "info@futureinnovations.com",
                        PhoneNumber = "555-0139",
                        CompanyId = companyMap["Bright Future Corp"],
                        Address = new Address { Address1 = "888 Visionary Ave", City = "Dallas", State = "TX", Country = "USA", ZipCode = "75201" }
                    },
                    new() {
                        Name = "Ava Scott",
                        Email = "ava.s@example.com",
                        PhoneNumber = "555-0140",
                        CompanyId = companyMap["Bright Future Corp"],
                        Address = new Address { Address1 = "999 Dreamland Dr", City = "Denver", State = "CO", Country = "USA", ZipCode = "80202" }
                    },
                    new() {
                        Name = "NextGen Systems",
                        Email = "support@nextgensystems.com",
                        PhoneNumber = "555-0141",
                        CompanyId = companyMap["Bright Future Corp"],
                        Address = new Address { Address1 = "101 Future Blvd", City = "Seoul", Country = "South Korea", ZipCode = "03149" }
                    },
                    new() {
                        Name = "Oliver Chen",
                        Email = "oliver.c@example.com",
                        PhoneNumber = "555-0142",
                        CompanyId = companyMap["Bright Future Corp"],
                        Address = new Address { Address1 = "202 Horizon Rd", City = "Shanghai", Country = "China", ZipCode = "200000" }
                    },

                    // Euro Dynamics - 8 Customers
                    new() {
                        Name = "Laura Martinez",
                        Email = "laura.m@example.com",
                        PhoneNumber = "555-0112",
                        CompanyId = companyMap["Euro Dynamics"],
                        Address = new Address { Address1 = "909 Laurel Rd", Address2 = "Apt 12", City = "Mumbai", State = "MH", Country = "India", ZipCode = "400001" }
                    },
                    new() {
                        Name = "NexGen Innovations",
                        Email = "finance@nexgen.com",
                        PhoneNumber = "555-0113",
                        CompanyId = companyMap["Euro Dynamics"],
                        Address = new Address { Address1 = "1010 Willow Ln", Address2 = "", City = "Tokyo", State = "", Country = "Japan", ZipCode = "100-0001" }
                    },
                    new() {
                        Name = "Charlotte Bell",
                        Email = "charlotte.b@example.com",
                        PhoneNumber = "555-0143",
                        CompanyId = companyMap["Euro Dynamics"],
                        Address = new Address { Address1 = "333 Europe St", City = "Frankfurt", Country = "Germany", ZipCode = "60311" }
                    },
                    new() {
                        Name = "Amelia Lewis",
                        Email = "amelia.l@example.com",
                        PhoneNumber = "555-0144",
                        CompanyId = companyMap["Euro Dynamics"],
                        Address = new Address { Address1 = "444 Alpine View", City = "Zurich", Country = "Switzerland", ZipCode = "8001" }
                    },
                    new() {
                        Name = "Continental Solutions",
                        Email = "contact@continentalsolutions.com",
                        PhoneNumber = "555-0145",
                        CompanyId = companyMap["Euro Dynamics"],
                        Address = new Address { Address1 = "555 Grand Plaza", City = "Rome", Country = "Italy", ZipCode = "00187" }
                    },
                    new() {
                        Name = "James Wright",
                        Email = "james.w@example.com",
                        PhoneNumber = "555-0146",
                        CompanyId = companyMap["Euro Dynamics"],
                        Address = new Address { Address1 = "666 Mediterranean Ave", City = "Madrid", Country = "Spain", ZipCode = "28001" }
                    },
                    new() {
                        Name = "European Tech Hub",
                        Email = "info@eurotechhub.com",
                        PhoneNumber = "555-0147",
                        CompanyId = companyMap["Euro Dynamics"],
                        Address = new Address { Address1 = "777 Nordic Way", City = "Stockholm", Country = "Sweden", ZipCode = "111 20" }
                    },
                    new() {
                        Name = "Grace Kelly",
                        Email = "grace.k@example.com",
                        PhoneNumber = "555-0148",
                        CompanyId = companyMap["Euro Dynamics"],
                        Address = new Address { Address1 = "888 Atlantic Ave", City = "Lisbon", Country = "Portugal", ZipCode = "1000-001" }
                    }
                };

                // Seed customers in a transaction
                using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var existingCustomers = await _dbContext.Customers
                              .Select(c => new { c.Email, c.Id })
                              .ToListAsync(cancellationToken);
                        var existingCustomerEmails = existingCustomers.Select(c => c.Email).ToHashSet();
                        var newCustomers = customersToSeed
                            .Where(c => !existingCustomerEmails.Contains(c.Email))
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

                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogError(ex, "Error seeding customers: {Message}", ex.Message);
                        throw;
                    }
                }

                // Retrieve all customers for invoice seeding
                var customers = await _dbContext.Customers
                    .ToListAsync(cancellationToken);

                var customerMap = customers.ToDictionary(c => c.Email, c => c.Id);

                // Seed Users
                var customerUsersToSeed = new List<(Customer Customer, ApplicationUser User, string Role)>
                {
                    // Customer Role
                    (customers.First(c => c.Email == "alagappanmuthukumar1998@gmail.com"), new ApplicationUser
                    {
                        UserName = "alaga.muthu@example.com",
                        Email = "alagappanmuthukumar1998@gmail.com",
                        FullName = "Alaga Muthu",
                        CustomerId = customerMap["alagappanmuthukumar1998@gmail.com"],
                        EmailConfirmed = true,
                        CompanyId = companyMap["KL Infotech"]
                    }, AppConstants.Role_Customer),

                    // User Role
                    (customers.First(c => c.Email == "jane.smith@example.com"), new ApplicationUser
                    {
                        UserName = "jane.smith@example.com",
                        Email = "jane.smith@example.com",
                        FullName = "Jane Smith",
                        CustomerId = customerMap["jane.smith@example.com"],
                        EmailConfirmed = true,
                        CompanyId = companyMap["KL Infotech"]
                    }, AppConstants.Role_User)
                };

                foreach (var (customer, user, role) in customerUsersToSeed)
                {
                    var existingUser = await _userManager.FindByEmailAsync(user.Email);
                    if (existingUser != null)
                    {
                        _logger.LogInformation("User {Email} already exists, skipping.", user.Email);
                        continue;
                    }

                    _logger.LogInformation("Creating user {Email} with role {Role}...", user.Email, role);
                    var creationResult = await _userManager.CreateAsync(user, "SecurePassword@123");
                    if (!creationResult.Succeeded)
                    {
                        _logger.LogError("Failed to create user {Email}", user.Email);
                        foreach (var error in creationResult.Errors)
                        {
                            _logger.LogError("User creation error: {Error}", error.Description);
                        }
                        throw new InvalidOperationException($"Failed to create user {user.Email}");
                    }

                    var roleResult = await _userManager.AddToRoleAsync(user, role);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Failed to assign {Role} role to user {Email}", role, user.Email);
                        foreach (var error in roleResult.Errors)
                        {
                            _logger.LogError("Role assignment error: {Error}", error.Description);
                        }
                        throw new InvalidOperationException($"Failed to assign {role} role to user {user.Email}");
                    }
                    _logger.LogInformation("Successfully created user {Email} with {Role} role.", user.Email, role);
                }

                // Seed Invoices
                var invoicesToSeed = new[]
                {
                    new Invoice
                    {
                        InvoiceNumber = "INV-001", PONumber = "PO-001", IssueDate = DateTime.UtcNow.AddDays(-30), PaymentDue = DateTime.UtcNow.AddDays(-15),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.Completed,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["alagappanmuthukumar1998@gmail.com"],
                        CompanyId = companyMap["KL Infotech"],
                        Subtotal = 2950.00m, Tax =  295.00m, TotalAmount = 3095.00m, // Subtotal(2950) + Tax(295) - Discount (150) = Total Amount (3095)
                        Notes = "Payment received on time.", PaymentMethod = "Bank Transfer",
                        Currency = "INR",
                        ProjectDetail = "Q3 Marketing Campaign - Digital Ad Spend", // Marketing/Advertising
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-30)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-002", PONumber = "PO-002", IssueDate = DateTime.UtcNow.AddDays(-20), PaymentDue = DateTime.UtcNow.AddDays(-5),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.Overdue,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["jane.smith@example.com"],
                        CompanyId = companyMap["KL Infotech"],
                        Subtotal = 2850.00m, Tax = 285.00m, TotalAmount = 2821.5m, // Subtotal(2850) + Tax(285) - Discount (10 % of 3135 = 313.5) = Total Amount (2821.5)
                        Notes = "Pending payment reminder sent.", PaymentMethod = "Credit Card",
                        Currency = "USD",
                        ProjectDetail = "Mobile App Revamp - UI/UX Design Phase", // Software Development
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-20)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-003", PONumber = "PO-003", IssueDate = DateTime.UtcNow.AddDays(-10), PaymentDue = DateTime.UtcNow.AddDays(5),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["billing@acmecorp.com"],
                        CompanyId = companyMap["KL Infotech"],
                        Subtotal = 2710.00m, Tax = 271.00m, TotalAmount = 2892.9m, // Subtotal(2710) + Tax(271) - Discount (10 % of 2981 = 298.1) = Total Amount (2892.9)
                        Notes = "Awaiting payment.", PaymentMethod = "PayPal",
                        Currency = "EUR",
                        ProjectDetail = "ERP System Integration - Consultancy Hours", // IT/Consulting
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-004", PONumber = "PO-004", IssueDate = DateTime.UtcNow.AddDays(-5), PaymentDue = DateTime.UtcNow.AddDays(10),
                        InvoiceStatus = InvoiceStatus.Draft, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Proforma,
                        CustomerId = customerMap["sarah.j@example.com"],
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Subtotal = 3400.00m, Tax = 340.00m, TotalAmount = 3740.00m,
                        Notes = "Draft for review.", PaymentMethod = null,
                        Currency = "USD",
                        ProjectDetail = "New Product Launch - Market Research", // Product Development/Research
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-005", PONumber = "PO-005", IssueDate = DateTime.UtcNow.AddDays(-25), PaymentDue = DateTime.UtcNow.AddDays(-10),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.PartiallyPaid,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["accounts@techtrend.com"],
                        CompanyId = companyMap["Tech Solutions Inc."],
                        Subtotal = 2750.00m, Tax = 275.00m, TotalAmount = 3025.00m,
                        Notes = "Partial payment of 1500 received.", PaymentMethod = "Bank Transfer",
                        Currency = "GBP",
                        ProjectDetail = "Infrastructure Upgrade - Server & Network", // IT Infrastructure
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-25)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-006", PONumber = "PO-006", IssueDate = DateTime.UtcNow.AddDays(-15), PaymentDue = DateTime.UtcNow,
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.Overdue,
                        InvoiceType = InvoiceType.Recurring,
                        CustomerId = customerMap["michael.brown@example.com"], CompanyId = companyMap["Global Enterprises"],
                        Subtotal = 4050.00m, Tax = 405.00m, TotalAmount = 4455.00m,
                        Notes = "Monthly subscription invoice.", PaymentMethod = "Credit Card",
                        Currency = "USD",
                        ProjectDetail = "Monthly Cloud Hosting Services", // Recurring IT Services
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-15)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-007", PONumber = "PO-007", IssueDate = DateTime.UtcNow.AddDays(-8), PaymentDue = DateTime.UtcNow.AddDays(7),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["finance@globaltraders.com"], CompanyId = companyMap["Global Enterprises"],
                        Subtotal = 3150.00m, Tax = 315.00m, TotalAmount = 3465.00m,
                        Notes = "Awaiting payment confirmation.", PaymentMethod = "PayPal",
                        Currency = "USD",
                        ProjectDetail = "Annual Security Penetration Test", // Cybersecurity
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-008", PONumber = "PO-008", IssueDate = DateTime.UtcNow.AddDays(-3), PaymentDue = DateTime.UtcNow.AddDays(12),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.Refunded,
                        InvoiceType = InvoiceType.CreditNote,
                        CustomerId = customerMap["emily.davis@example.com"], CompanyId = companyMap["Innovate Tech Ltd"],
                        Subtotal = -4150.00m, Tax = -415.00m, TotalAmount = -4565.00m,
                        Notes = "Credit note issued for returned items.", PaymentMethod = null,
                        Currency = "CAD",
                        ProjectDetail = "Refund Processing - Customer Return Policy",
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-009", PONumber = "PO-009", IssueDate = DateTime.UtcNow.AddDays(-12), PaymentDue = DateTime.UtcNow.AddDays(3),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["billing@innovatedesigns.com"], CompanyId = companyMap["Innovate Tech Ltd"],
                        Subtotal = 4800.00m, Tax = 480.00m, TotalAmount = 5280.00m,
                        Notes = "Awaiting payment for design services.", PaymentMethod = "Bank Transfer",
                        Currency = "USD",
                        ProjectDetail = "Corporate Brand Identity & Guidelines",
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-010", PONumber = "PO-010", IssueDate = DateTime.UtcNow.AddDays(-18), PaymentDue = DateTime.UtcNow.AddDays(-3),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.Overdue,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["robert.w@example.com"], CompanyId = companyMap["Bright Future Corp"],
                        Subtotal = 3950.00m, Tax = 395.00m, TotalAmount = 4345.00m,
                        Notes = "Payment overdue, reminder sent.", PaymentMethod = "Credit Card",
                        Currency = "USD",
                        ProjectDetail = "Q2 Content Marketing & SEO Strategy", // Marketing
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-18)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-011", PONumber = "PO-011", IssueDate = DateTime.UtcNow.AddDays(-7), PaymentDue = DateTime.UtcNow.AddDays(8),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["accounts@brightsolutions.com"], CompanyId = companyMap["Bright Future Corp"],
                        Subtotal = 3900.00m, Tax = 390.00m, TotalAmount = 4290.00m,
                        Notes = "Awaiting payment for consulting services.", PaymentMethod = "PayPal",
                        Currency = "AUD",
                        ProjectDetail = "Strategic Business Process Re-engineering", // Business
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-012", PONumber = "PO-012", IssueDate = DateTime.UtcNow.AddDays(-2), PaymentDue = DateTime.UtcNow.AddDays(13),
                        InvoiceStatus = InvoiceStatus.Draft, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Proforma,
                        CustomerId = customerMap["laura.m@example.com"], CompanyId = companyMap["Euro Dynamics"],
                        Subtotal = 7200.00m, Tax = 720.00m, TotalAmount = 7920.00m,
                        Notes = "Draft for client review, no payment initiated.", PaymentMethod = null,
                        Currency = "USD",
                        ProjectDetail = "Custom Software Development - Backend Module", // Software Development
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-013", PONumber = "PO-013", IssueDate = DateTime.UtcNow.AddDays(-40), PaymentDue = DateTime.UtcNow.AddDays(-25),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.Completed,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["alagappanmuthukumar1998@gmail.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 3450.00m, Tax = 345.00m, TotalAmount = 3795.00m,
                        Notes = "Payment received in full early.", PaymentMethod = "Bank Transfer",
                        Currency = "USD",
                        ProjectDetail = "Server Migration to AWS Cloud", // Cloud Computing
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-40)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-014", PONumber = "PO-014", IssueDate = DateTime.UtcNow.AddDays(-35), PaymentDue = DateTime.UtcNow.AddDays(-20),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.Overdue,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["jane.smith@example.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 3450.00m, Tax = 345.00m, TotalAmount = 3795.00m,
                        Notes = "Payment overdue, follow-up required.", PaymentMethod = "Credit Card",
                        Currency = "USD",
                        ProjectDetail = "IT Support & Maintenance Contract - July", // IT Support
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-35)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-015", PONumber = "PO-015", IssueDate = DateTime.UtcNow.AddDays(-28), PaymentDue = DateTime.UtcNow.AddDays(-10),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.PartiallyPaid,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["billing@acmecorp.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 3100.00m, Tax = 310.00m, TotalAmount = 3410.00m,
                        Notes = "Partial payment received.", PaymentMethod = "UPI",
                        Currency = "INR",
                        ProjectDetail = "Custom Web Application Development - Frontend", // Web Development
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-28)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-016", PONumber = "PO-016", IssueDate = DateTime.UtcNow.AddDays(-22), PaymentDue = DateTime.UtcNow.AddDays(-5),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["alagappanmuthukumar1998@gmail.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 4000.00m, Tax = 400.00m, TotalAmount = 4400.00m,
                        Notes = "Awaiting client payment.", PaymentMethod = "PayPal",
                        Currency = "USD",
                        ProjectDetail = "Quarterly SEO Performance Report", // SEO
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-22)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-017", PONumber = "PO-017", IssueDate = DateTime.UtcNow.AddDays(-18), PaymentDue = DateTime.UtcNow.AddDays(2),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["jane.smith@example.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 7400.00m, Tax = 740.00m, TotalAmount = 8140.00m,
                        Notes = "Awaiting payment for design work.", PaymentMethod = "Bank Transfer",
                        Currency = "USD",
                        ProjectDetail = "Packaging Design & Prototyping", // Product Designing
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-18)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-018", PONumber = "PO-018", IssueDate = DateTime.UtcNow.AddDays(-12), PaymentDue = DateTime.UtcNow.AddDays(7),
                        InvoiceStatus = InvoiceStatus.Draft, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Proforma,
                        CustomerId = customerMap["billing@acmecorp.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 5800.00m, Tax = 580.00m, TotalAmount = 6380.00m,
                        Notes = "Draft under discussion, no payment initiated.", PaymentMethod = null,
                        Currency = "USD",
                        ProjectDetail = "Feasibility Study - New Market Entry", // Consultation/strategy
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-019", PONumber = "PO-019", IssueDate = DateTime.UtcNow.AddDays(-10), PaymentDue = DateTime.UtcNow.AddDays(10),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["alagappanmuthukumar1998@gmail.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 4500.00m, Tax = 450.00m, TotalAmount = 4950.00m,
                        Notes = "Awaiting final payment.", PaymentMethod = "Stripe",
                        Currency = "USD",
                        ProjectDetail = "API Development & Integration for Partners", // Software Integration
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-020", PONumber = "PO-020", IssueDate = DateTime.UtcNow.AddDays(-7), PaymentDue = DateTime.UtcNow.AddDays(14),
                        InvoiceStatus = InvoiceStatus.Draft, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Proforma,
                        CustomerId = customerMap["jane.smith@example.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 6500.00m, Tax = 650.00m, TotalAmount = 7150.00m,
                        Notes = "Proforma sent for approval, no payment initiated.", PaymentMethod = null,
                        Currency = "USD",
                        ProjectDetail = "UX Research & Persona Development", // UX Research
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-021", PONumber = "PO-021", IssueDate = DateTime.UtcNow.AddDays(-6), PaymentDue = DateTime.UtcNow.AddDays(5),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.PartiallyPaid,
                        InvoiceType = InvoiceType.Recurring,
                        CustomerId = customerMap["billing@acmecorp.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 4200.00m, Tax = 420.00m, TotalAmount = 4620.00m,
                        Notes = "Partial payment received for monthly subscription.", PaymentMethod = "Credit Card",
                        Currency = "USD",
                        ProjectDetail = "Software License Renewal - SaaS Subscription", // Licensing/Recurring
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-022", PONumber = "PO-022", IssueDate = DateTime.UtcNow.AddDays(-4), PaymentDue = DateTime.UtcNow.AddDays(10),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["david.lee@example.com"], CompanyId = companyMap["KL Infotech"],
                        Subtotal = 7100.00m, Tax = 710.00m, TotalAmount = 7810.00m,
                        Notes = "Awaiting payment for development services.", PaymentMethod = "UPI",
                        Currency = "INR",
                        ProjectDetail = "Database Optimization & Performance Tuning", // Database Management
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-023", PONumber = "PO-023", IssueDate = DateTime.UtcNow.AddDays(-3), PaymentDue = DateTime.UtcNow.AddDays(12),
                        InvoiceStatus = InvoiceStatus.Sent, PaymentStatus = PaymentStatus.Pending,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["mark.t@example.com"], CompanyId = companyMap["Tech Solutions Inc."],
                        Subtotal = 4600.00m, Tax = 460.00m, TotalAmount = 5060.00m,
                        Notes = "Awaiting payment for testing services.", PaymentMethod = "Bank Transfer",
                        Currency = "USD",
                        ProjectDetail = "Automated Testing Suite Development", // QA/Testing
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "INV-024", PONumber = "PO-024", IssueDate = DateTime.UtcNow.AddDays(-1), PaymentDue = DateTime.UtcNow.AddDays(15),
                        InvoiceStatus = InvoiceStatus.Approved, PaymentStatus = PaymentStatus.PartiallyPaid,
                        InvoiceType = InvoiceType.Standard,
                        CustomerId = customerMap["anna.g@example.com"], CompanyId = companyMap["Tech Solutions Inc."],
                        Subtotal = 5600.00m, Tax = 560.00m, TotalAmount = 6160.00m,
                        Notes = "Partial payment received for final milestone.", PaymentMethod = "PayPal",
                        Currency = "USD",
                        ProjectDetail = "Compliance Audit & Reporting Services", // Auditing/Reporting
                        IsAutomated = false,
                        CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-1)
                    },
                };

                var existingInvoices = await _dbContext.Invoices
                         .Where(i => invoicesToSeed.Select(x => x.InvoiceNumber).Contains(i.InvoiceNumber))
                         .ToListAsync(cancellationToken);
                var newInvoices = invoicesToSeed
                    .Where(i => !existingInvoices.Any(ei => ei.InvoiceNumber == i.InvoiceNumber))
                    .ToArray();
                if (newInvoices.Any())
                {
                    _dbContext.Invoices.AddRange(newInvoices);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Seeded {Count} new invoices.", newInvoices.Length);
                }
                else
                {
                    _logger.LogInformation("No new invoices to seed.");
                }

                // Seed Invoice Items
                var invoiceItems = new[]
                {
                    // --- INV-001 ---
                    new InvoiceItem { Invoice = invoicesToSeed[0], Description = "Web Development Services", Quantity = 10, UnitPrice = 100.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-30) },
                    new InvoiceItem { Invoice = invoicesToSeed[0], Description = "Frontend Design Package", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-29) },
                    new InvoiceItem { Invoice = invoicesToSeed[0], Description = "Database Optimization", Quantity = 5, UnitPrice = 150.00m, Amount = 750.00m, TaxType = "GST", TaxAmount = 75.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-28) },
                    new InvoiceItem { Invoice = invoicesToSeed[0], Description = "API Integration", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-27) },
                    new InvoiceItem { Invoice = invoicesToSeed[0], Description = "Consultation Hours", Quantity = 3, UnitPrice = 100.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-26) },

                    // --- INV-002  ---
                    new InvoiceItem { Invoice = invoicesToSeed[1], Description = "Software License (Pro Version)", Quantity = 5, UnitPrice = 300.00m, Amount = 1500.00m, TaxType = "GST", TaxAmount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-20) },
                    new InvoiceItem { Invoice = invoicesToSeed[1], Description = "User Training Session", Quantity = 1, UnitPrice = 250.00m, Amount = 250.00m, TaxType = "GST", TaxAmount = 25.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-19) },
                    new InvoiceItem { Invoice = invoicesToSeed[1], Description = "Custom Feature Development", Quantity = 1, UnitPrice = 350.00m, Amount = 350.00m, TaxType = "GST", TaxAmount = 35.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-18) },
                    new InvoiceItem { Invoice = invoicesToSeed[1], Description = "Monthly Support Package", Quantity = 1, UnitPrice = 200.00m, Amount = 200.00m, TaxType = "GST", TaxAmount = 20.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-17) },
                    new InvoiceItem { Invoice = invoicesToSeed[1], Description = "Data Migration Assistance", Quantity = 1, UnitPrice = 150.00m, Amount = 150.00m, TaxType = "GST", TaxAmount = 15.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-16) },
                    new InvoiceItem { Invoice = invoicesToSeed[1], Description = "Configuration Services", Quantity = 1, UnitPrice = 100.00m, Amount = 100.00m, TaxType = "GST", TaxAmount = 10.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-15) },

                    // --- INV-003  ---
                    new InvoiceItem { Invoice = invoicesToSeed[2], Description = "Cloud Hosting Charges", Quantity = 12, UnitPrice = 80.00m, Amount = 960.00m, TaxType = "GST", TaxAmount = 96.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-14) },
                    new InvoiceItem { Invoice = invoicesToSeed[2], Description = "Security Patch Update", Quantity = 1, UnitPrice = 250.00m, Amount = 250.00m, TaxType = "GST", TaxAmount = 25.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-13) },
                    new InvoiceItem { Invoice = invoicesToSeed[2], Description = "System Monitoring", Quantity = 6, UnitPrice = 90.00m, Amount = 540.00m, TaxType = "GST", TaxAmount = 54.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                    new InvoiceItem { Invoice = invoicesToSeed[2], Description = "Backup Configuration", Quantity = 2, UnitPrice = 180.00m, Amount = 360.00m, TaxType = "GST", TaxAmount = 36.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-11) },
                    new InvoiceItem { Invoice = invoicesToSeed[2], Description = "Admin Dashboard Setup", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },

                    // --- INV-004  ---
                    new InvoiceItem { Invoice = invoicesToSeed[3], Description = "UX Audit", Quantity = 1, UnitPrice = 450.00m, Amount = 450.00m, TaxType = "GST", TaxAmount = 45.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[3], Description = "Performance Optimization", Quantity = 4, UnitPrice = 200.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[3], Description = "Custom Reports Module", Quantity = 1, UnitPrice = 750.00m, Amount = 750.00m, TaxType = "GST", TaxAmount = 75.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[3], Description = "Third-party Tool Integration", Quantity = 2, UnitPrice = 300.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[3], Description = "Analytics Setup", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                    new InvoiceItem { Invoice = invoicesToSeed[3], Description = "On-site Deployment Support", Quantity = 1, UnitPrice = 300.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },

                    // --- INV-005 ---
                    new InvoiceItem { Invoice = invoicesToSeed[4], Description = "E-Commerce Integration", Quantity = 1, UnitPrice = 900.00m, Amount = 900.00m, TaxType = "GST", TaxAmount = 90.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                    new InvoiceItem { Invoice = invoicesToSeed[4], Description = "Payment Gateway Setup", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new InvoiceItem { Invoice = invoicesToSeed[4], Description = "Product Catalogue Design", Quantity = 1, UnitPrice = 350.00m, Amount = 350.00m, TaxType = "GST", TaxAmount = 35.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new InvoiceItem { Invoice = invoicesToSeed[4], Description = "Responsive Theme Setup", Quantity = 2, UnitPrice = 300.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new InvoiceItem { Invoice = invoicesToSeed[4], Description = "SEO Optimization", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-1) },

                    // --- INV-006  ---
                    new InvoiceItem { Invoice = invoicesToSeed[5], Description = "Mobile App UI Design", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                    new InvoiceItem { Invoice = invoicesToSeed[5], Description = "React Native Development", Quantity = 3, UnitPrice = 600.00m, Amount = 1800.00m, TaxType = "GST", TaxAmount = 180.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-11) },
                    new InvoiceItem { Invoice = invoicesToSeed[5], Description = "API Authentication Setup", Quantity = 1, UnitPrice = 450.00m, Amount = 450.00m, TaxType = "GST", TaxAmount = 45.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[5], Description = "Push Notification Integration", Quantity = 1, UnitPrice = 300.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[5], Description = "App Deployment (Android & iOS)", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },

                    // --- INV-007  ---
                    new InvoiceItem { Invoice = invoicesToSeed[6], Description = "Product Strategy Consulting", Quantity = 2, UnitPrice = 800.00m, Amount = 1600.00m, TaxType = "GST", TaxAmount = 160.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-14) },
                    new InvoiceItem { Invoice = invoicesToSeed[6], Description = "User Persona Research", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-13) },
                    new InvoiceItem { Invoice = invoicesToSeed[6], Description = "Journey Mapping Session", Quantity = 1, UnitPrice = 300.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                    new InvoiceItem { Invoice = invoicesToSeed[6], Description = "Competitive Analysis Report", Quantity = 1, UnitPrice = 350.00m, Amount = 350.00m, TaxType = "GST", TaxAmount = 35.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-11) },
                    new InvoiceItem { Invoice = invoicesToSeed[6], Description = "Brand Style Guide", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[6], Description = "Pitch Deck Design", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },

                    // --- INV-008  ---
                    new InvoiceItem { Invoice = invoicesToSeed[7], Description = "Legacy System Migration", Quantity = 1, UnitPrice = 1200.00m, Amount = 1200.00m, TaxType = "GST", TaxAmount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[7], Description = "Schema Refactoring", Quantity = 1, UnitPrice = 900.00m, Amount = 900.00m, TaxType = "GST", TaxAmount = 90.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[7], Description = "Code Cleanup & Testing", Quantity = 2, UnitPrice = 400.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[7], Description = "CI/CD Pipeline Setup", Quantity = 1, UnitPrice = 550.00m, Amount = 550.00m, TaxType = "GST", TaxAmount = 55.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                    new InvoiceItem { Invoice = invoicesToSeed[7], Description = "Production Rollout Support", Quantity = 1, UnitPrice = 700.00m, Amount = 700.00m, TaxType = "GST", TaxAmount = 70.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },

                    // --- INV-009  ---
                    new InvoiceItem { Invoice = invoicesToSeed[8], Description = "AI Chatbot Integration", Quantity = 1, UnitPrice = 1500.00m, Amount = 1500.00m, TaxType = "GST", TaxAmount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[8], Description = "Natural Language Training", Quantity = 2, UnitPrice = 700.00m, Amount = 1400.00m, TaxType = "GST", TaxAmount = 140.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[8], Description = "Dialog Flow Setup", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[8], Description = "Support Bot Deployment", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[8], Description = "Multi-language Support", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[8], Description = "Knowledge Base Integration", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },

                    // --- INV-010  ---
                    new InvoiceItem { Invoice = invoicesToSeed[9], Description = "Website Revamp", Quantity = 1, UnitPrice = 2000.00m, Amount = 2000.00m, TaxType = "GST", TaxAmount = 200.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },
                    new InvoiceItem { Invoice = invoicesToSeed[9], Description = "Content Writing Services", Quantity = 10, UnitPrice = 100.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                    new InvoiceItem { Invoice = invoicesToSeed[9], Description = "Blog Setup", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new InvoiceItem { Invoice = invoicesToSeed[9], Description = "Newsletter Integration", Quantity = 1, UnitPrice = 300.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-1) },
                    new InvoiceItem { Invoice = invoicesToSeed[9], Description = "Social Media Sync", Quantity = 1, UnitPrice = 250.00m, Amount = 250.00m, TaxType = "GST", TaxAmount = 25.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow },

                    // --- INV-011  ---
                    new InvoiceItem { Invoice = invoicesToSeed[10], Description = "Onboarding Documentation", Quantity = 1, UnitPrice = 300.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[10], Description = "DevOps Consultation", Quantity = 2, UnitPrice = 700.00m, Amount = 1400.00m, TaxType = "GST", TaxAmount = 140.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[10], Description = "Server Setup and Optimization", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[10], Description = "Uptime Monitoring Integration", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[10], Description = "Load Balancer Configuration", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },

                    // --- INV-012  ---
                    new InvoiceItem { Invoice = invoicesToSeed[11], Description = "Multi-Vendor Marketplace Setup", Quantity = 1, UnitPrice = 2500.00m, Amount = 2500.00m, TaxType = "GST", TaxAmount = 250.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                    new InvoiceItem { Invoice = invoicesToSeed[11], Description = "Vendor Registration Module", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-11) },
                    new InvoiceItem { Invoice = invoicesToSeed[11], Description = "Commission Calculation Logic", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[11], Description = "Custom Payment Gateway", Quantity = 1, UnitPrice = 1500.00m, Amount = 1500.00m, TaxType = "GST", TaxAmount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[11], Description = "Admin Dashboard Panel", Quantity = 1, UnitPrice = 900.00m, Amount = 900.00m, TaxType = "GST", TaxAmount = 90.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[11], Description = "Basic SEO Optimization", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },

                    // --- INV-013  ---
                    new InvoiceItem { Invoice = invoicesToSeed[12], Description = "Logo Design Package", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[12], Description = "Business Card Design", Quantity = 1, UnitPrice = 300.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                    new InvoiceItem { Invoice = invoicesToSeed[12], Description = "Social Media Kit", Quantity = 1, UnitPrice = 700.00m, Amount = 700.00m, TaxType = "GST", TaxAmount = 70.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },
                    new InvoiceItem { Invoice = invoicesToSeed[12], Description = "Brand Identity Guidelines", Quantity = 1, UnitPrice = 900.00m, Amount = 900.00m, TaxType = "GST", TaxAmount = 90.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                    new InvoiceItem { Invoice = invoicesToSeed[12], Description = "Merchandise Mockups", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },

                    // --- INV-014  ---
                    new InvoiceItem { Invoice = invoicesToSeed[13], Description = "Annual Hosting Plan", Quantity = 1, UnitPrice = 1200.00m, Amount = 1200.00m, TaxType = "GST", TaxAmount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[13], Description = "Cloud Storage (1TB)", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[13], Description = "SSL Certificate", Quantity = 1, UnitPrice = 300.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[13], Description = "Email Hosting (10 users)", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[13], Description = "Technical Support (1 Year)", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[13], Description = "DNS Configuration", Quantity = 1, UnitPrice = 250.00m, Amount = 250.00m, TaxType = "GST", TaxAmount = 25.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },

                    // --- INV-015  ---
                    new InvoiceItem { Invoice = invoicesToSeed[14], Description = "Video Editing (Promo)", Quantity = 1, UnitPrice = 700.00m, Amount = 700.00m, TaxType = "GST", TaxAmount = 70.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },
                    new InvoiceItem { Invoice = invoicesToSeed[14], Description = "Voice Over Services", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                    new InvoiceItem { Invoice = invoicesToSeed[14], Description = "Animation Services", Quantity = 1, UnitPrice = 1200.00m, Amount = 1200.00m, TaxType = "GST", TaxAmount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new InvoiceItem { Invoice = invoicesToSeed[14], Description = "Color Grading & Effects", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-1) },
                    new InvoiceItem { Invoice = invoicesToSeed[14], Description = "Subtitle Integration", Quantity = 1, UnitPrice = 300.00m, Amount = 300.00m, TaxType = "GST", TaxAmount = 30.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow },

                    // --- INV-016 ---
                    new InvoiceItem { Invoice = invoicesToSeed[15], Description = "Mobile App UI Design", Quantity = 1, UnitPrice = 1500.00m, Amount = 1500.00m, TaxType = "GST", TaxAmount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[15], Description = "Wireframe Prototypes", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[15], Description = "UX Research Report", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[15], Description = "User Persona Development", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[15], Description = "App Icon Set", Quantity = 1, UnitPrice = 200.00m, Amount = 200.00m, TaxType = "GST", TaxAmount = 20.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[15], Description = "Design Handoff (Figma)", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },

                    // --- INV-017 ---
                    new InvoiceItem { Invoice = invoicesToSeed[16], Description = "E-commerce Backend API", Quantity = 1, UnitPrice = 2500.00m, Amount = 2500.00m, TaxType = "GST", TaxAmount = 250.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                    new InvoiceItem { Invoice = invoicesToSeed[16], Description = "Product Catalog Module", Quantity = 1, UnitPrice = 1500.00m, Amount = 1500.00m, TaxType = "GST", TaxAmount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-11) },
                    new InvoiceItem { Invoice = invoicesToSeed[16], Description = "Cart & Checkout Integration", Quantity = 1, UnitPrice = 1800.00m, Amount = 1800.00m, TaxType = "GST", TaxAmount = 180.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[16], Description = "Payment Gateway Setup", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[16], Description = "Email Notification System", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },

                    // --- INV-018 ---
                    new InvoiceItem { Invoice = invoicesToSeed[17], Description = "Cloud Infra Setup (AWS)", Quantity = 1, UnitPrice = 3000.00m, Amount = 3000.00m, TaxType = "GST", TaxAmount = 300.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[17], Description = "EC2 Configuration", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[17], Description = "RDS Setup", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                    new InvoiceItem { Invoice = invoicesToSeed[17], Description = "CloudWatch Alerts Setup", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },
                    new InvoiceItem { Invoice = invoicesToSeed[17], Description = "IAM Role Configuration", Quantity = 1, UnitPrice = 400.00m, Amount = 400.00m, TaxType = "GST", TaxAmount = 40.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },

                    // --- INV-019 ---
                    new InvoiceItem { Invoice = invoicesToSeed[18], Description = "Performance Audit Report", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[18], Description = "Bug Fixing Sprint", Quantity = 2, UnitPrice = 750.00m, Amount = 1500.00m, TaxType = "GST", TaxAmount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[18], Description = "Browser Compatibility Fixes", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[18], Description = "Mobile Responsiveness Fix", Quantity = 1, UnitPrice = 700.00m, Amount = 700.00m, TaxType = "GST", TaxAmount = 70.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[18], Description = "Security Patch Update", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                    new InvoiceItem { Invoice = invoicesToSeed[18], Description = "Page Load Optimization", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },

                    // --- INV-020 ---
                    new InvoiceItem { Invoice = invoicesToSeed[19], Description = "Full Stack Developer Support", Quantity = 1, UnitPrice = 3000.00m, Amount = 3000.00m, TaxType = "GST", TaxAmount = 300.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                    new InvoiceItem { Invoice = invoicesToSeed[19], Description = "Dedicated QA Support", Quantity = 1, UnitPrice = 1200.00m, Amount = 1200.00m, TaxType = "GST", TaxAmount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new InvoiceItem { Invoice = invoicesToSeed[19], Description = "Project Coordination", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-1) },
                    new InvoiceItem { Invoice = invoicesToSeed[19], Description = "Sprint Review Meetings", Quantity = 2, UnitPrice = 250.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow },
                    new InvoiceItem { Invoice = invoicesToSeed[19], Description = "Documentation & Reports", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow },

                    // --- INV-021 ---
                    new InvoiceItem { Invoice = invoicesToSeed[20], Description = "SEO Optimization Services", Quantity = 1, UnitPrice = 1200.00m, Amount = 1200.00m, TaxType = "GST", TaxAmount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[20], Description = "Backlink Campaign", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[20], Description = "On-Page Optimization", Quantity = 1, UnitPrice = 700.00m, Amount = 700.00m, TaxType = "GST", TaxAmount = 70.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[20], Description = "Content Marketing", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[20], Description = "Keyword Research", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },

                    // --- INV-022 (6 items) ---
                    new InvoiceItem { Invoice = invoicesToSeed[21], Description = "Data Analytics Dashboard", Quantity = 1, UnitPrice = 2200.00m, Amount = 2200.00m, TaxType = "GST", TaxAmount = 220.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                    new InvoiceItem { Invoice = invoicesToSeed[21], Description = "ETL Pipeline Setup", Quantity = 1, UnitPrice = 1500.00m, Amount = 1500.00m, TaxType = "GST", TaxAmount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-11) },
                    new InvoiceItem { Invoice = invoicesToSeed[21], Description = "Power BI Integration", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new InvoiceItem { Invoice = invoicesToSeed[21], Description = "Data Cleansing", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-9) },
                    new InvoiceItem { Invoice = invoicesToSeed[21], Description = "Report Automation", Quantity = 1, UnitPrice = 700.00m, Amount = 700.00m, TaxType = "GST", TaxAmount = 70.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[21], Description = "Business Insights Setup", Quantity = 1, UnitPrice = 900.00m, Amount = 900.00m, TaxType = "GST", TaxAmount = 90.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },

                    // --- INV-023 (5 items) ---
                    new InvoiceItem { Invoice = invoicesToSeed[22], Description = "SaaS Platform License", Quantity = 10, UnitPrice = 100.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                    new InvoiceItem { Invoice = invoicesToSeed[22], Description = "Setup & Configuration", Quantity = 1, UnitPrice = 800.00m, Amount = 800.00m, TaxType = "GST", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },
                    new InvoiceItem { Invoice = invoicesToSeed[22], Description = "Custom Roles & Permissions", Quantity = 1, UnitPrice = 500.00m, Amount = 500.00m, TaxType = "GST", TaxAmount = 50.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                    new InvoiceItem { Invoice = invoicesToSeed[22], Description = "Admin Dashboard Setup", Quantity = 1, UnitPrice = 600.00m, Amount = 600.00m, TaxType = "GST", TaxAmount = 60.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new InvoiceItem { Invoice = invoicesToSeed[22], Description = "Audit Logging Module", Quantity = 1, UnitPrice = 700.00m, Amount = 700.00m, TaxType = "GST", TaxAmount = 70.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-1) },

                    // --- INV-024 (5 items) ---
                    new InvoiceItem { Invoice = invoicesToSeed[23], Description = "DevOps Pipeline Setup", Quantity = 1, UnitPrice = 1800.00m, Amount = 1800.00m, TaxType = "GST", TaxAmount = 180.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new InvoiceItem { Invoice = invoicesToSeed[23], Description = "CI/CD for Production", Quantity = 1, UnitPrice = 1200.00m, Amount = 1200.00m, TaxType = "GST", TaxAmount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new InvoiceItem { Invoice = invoicesToSeed[23], Description = "GitHub Actions Setup", Quantity = 1, UnitPrice = 1000.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new InvoiceItem { Invoice = invoicesToSeed[23], Description = "Dockerization", Quantity = 1, UnitPrice = 900.00m, Amount = 900.00m, TaxType = "GST", TaxAmount = 90.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                    new InvoiceItem { Invoice = invoicesToSeed[23], Description = "Monitoring Setup", Quantity = 1, UnitPrice = 700.00m, Amount = 700.00m, TaxType = "GST", TaxAmount = 70.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },
                };

                if (newInvoices.Any())
                {
                    _dbContext.InvoiceItems.AddRange(invoiceItems);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Seeded {Count} invoice items.", invoiceItems.Length);
                }
                else
                {
                    _logger.LogInformation("No new invoice items to seed.");
                }

                // Seed Discounts
                var discounts = new[]
                {
                    new Discount
                    {
                        Invoice = invoicesToSeed[0], // INV-001
                        Description = "Promotional Discount",
                        Amount = 150.00m,
                        IsPercentage = false,
                        CreatedBy = "system",
                        CreatedDate = DateTime.UtcNow.AddDays(-30)
                    },
                    new Discount
                    {
                        Invoice = invoicesToSeed[1], // INV-002
                        Description = "10% Early Payment Discount",
                        Amount = 10.00m,
                        IsPercentage = true,
                        CreatedBy = "system",
                        CreatedDate = DateTime.UtcNow.AddDays(-20)
                    },
                    new Discount
                    {
                        Invoice = invoicesToSeed[2], // INV-003
                        Description = "10% Client Loyalty Discount",
                        Amount = 10.00m,
                        IsPercentage = true,
                        CreatedBy = "system",
                        CreatedDate = DateTime.UtcNow.AddDays(-10)
                    }
                };

                if (newInvoices.Any())
                {
                    _dbContext.Discounts.AddRange(discounts);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Seeded {Count} discounts.", discounts.Length);
                }
                else
                {
                    _logger.LogInformation("No new discounts to seed.");
                }

                // Seed Tax Details
                var taxDetails = new[]
                {
                    new TaxDetail { Invoice = invoicesToSeed[0], TaxType = "GST", Rate = 10.00m, Amount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-30) },
                    new TaxDetail { Invoice = invoicesToSeed[1], TaxType = "GST", Rate = 10.00m, Amount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-20) },
                    new TaxDetail { Invoice = invoicesToSeed[2], TaxType = "VAT", Rate = 10.00m, Amount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new TaxDetail { Invoice = invoicesToSeed[3], TaxType = "GST", Rate = 10.00m, Amount = 200.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                    new TaxDetail { Invoice = invoicesToSeed[4], TaxType = "GST", Rate = 10.00m, Amount = 300.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-25) },
                    new TaxDetail { Invoice = invoicesToSeed[5], TaxType = "GST", Rate = 10.00m, Amount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-15) },
                    new TaxDetail { Invoice = invoicesToSeed[6], TaxType = "VAT", Rate = 10.00m, Amount = 180.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
                    new TaxDetail { Invoice = invoicesToSeed[8], TaxType = "GST", Rate = 10.00m, Amount = 250.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                    new TaxDetail { Invoice = invoicesToSeed[9], TaxType = "GST", Rate = 10.00m, Amount = 160.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-18) },
                    new TaxDetail { Invoice = invoicesToSeed[10], TaxType = "VAT", Rate = 10.00m, Amount = 220.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new TaxDetail { Invoice = invoicesToSeed[11], TaxType = "GST", Rate = 10.00m, Amount = 190.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) },
                    new TaxDetail { Invoice = invoicesToSeed[12], TaxType = "GST", Rate = 10m, Amount = 120m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-40) },
                    new TaxDetail { Invoice = invoicesToSeed[13], TaxType = "GST", Rate = 10m, Amount = 95m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-35) },
                    new TaxDetail { Invoice = invoicesToSeed[14], TaxType = "GST", Rate = 10m, Amount = 180m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-28) },
                    new TaxDetail { Invoice = invoicesToSeed[15], TaxType = "GST", Rate = 10m, Amount = 70m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-22) },
                    new TaxDetail { Invoice = invoicesToSeed[16], TaxType = "GST", Rate = 10m, Amount = 150m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-18) },
                    new TaxDetail { Invoice = invoicesToSeed[17], TaxType = "GST", Rate = 10m, Amount = 220m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
                    new TaxDetail { Invoice = invoicesToSeed[18], TaxType = "GST", Rate = 10m, Amount = 65m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                    new TaxDetail { Invoice = invoicesToSeed[19], TaxType = "GST", Rate = 10m, Amount = 125m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
                    new TaxDetail { Invoice = invoicesToSeed[20], TaxType = "GST", Rate = 10m, Amount = 40m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-6) },
                    new TaxDetail { Invoice = invoicesToSeed[21], TaxType = "GST", Rate = 10m, Amount = 100m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-4) },
                    new TaxDetail { Invoice = invoicesToSeed[22], TaxType = "GST", Rate = 10m, Amount = 175m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },
                    new TaxDetail { Invoice = invoicesToSeed[23], TaxType = "GST", Rate = 10m, Amount = 210m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-1) }

                };

                if (newInvoices.Any())
                {
                    _dbContext.TaxDetails.AddRange(taxDetails);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Seeded {Count} tax details.", taxDetails.Length);
                }
                else
                {
                    _logger.LogInformation("No new tax details to seed.");
                }

                // Seed Tax Types
                var existingTaxTypes = await _dbContext.TaxTypes.ToListAsync(cancellationToken);
                var newTaxTypes = new[]
                {
                    new TaxType { Name = "Sales 5%", Rate = 5.00m, CompanyId = 1, CreatedBy = "system", CreatedDate = DateTime.UtcNow },
                    new TaxType { Name = "GST 10%", Rate = 10.00m, CompanyId = 1, CreatedBy = "system", CreatedDate = DateTime.UtcNow },
                    new TaxType { Name = "VAT 20%", Rate = 20.00m, CompanyId = 2, CreatedBy = "system", CreatedDate = DateTime.UtcNow }
                }.Where(tt => !existingTaxTypes.Any(et => et.Name == tt.Name && et.CompanyId == tt.CompanyId)).ToArray();

                if (newTaxTypes.Any())
                {
                    _dbContext.TaxTypes.AddRange(newTaxTypes);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Seeded {Count} new tax types.", newTaxTypes.Length);
                }
                else
                {
                    _logger.LogInformation("No new tax types to seed.");
                }

                _logger.LogInformation("Successfully seeded customers and related data.");
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
