using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Core_API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthStates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EmailOTP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SmsOTP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PasswordVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Brand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstablishedYear = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StreetAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactUsSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUsSubmissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExportLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExportedUserCount = table.Column<int>(type: "int", nullable: false),
                    ExportTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QueryParameters = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImpersonationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImpersonatedUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpersonationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Timezones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UtcOffset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UtcOffsetString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timezones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategory_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFirstLogin = table.Column<bool>(type: "bit", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TwoFactorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwoFactorExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OtpIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VendorPictureUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendors_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MenuName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenuPermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleMenuPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    TimezoneId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Locations_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Locations_Timezones_TimezoneId",
                        column: x => x.TimezoneId,
                        principalTable: "Timezones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentDueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EstimatedDelivery = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShippingMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrackingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShippingContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CustomerNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ShippingAddress1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingAddress2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShippingState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShippingCountry = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShippingZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BillingAddress1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingAddress2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BillingCountry = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderHeaders_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderHeaders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    DiscountPrice = table.Column<double>(type: "float", nullable: false),
                    IsDiscounted = table.Column<bool>(type: "bit", nullable: false),
                    DiscountStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiscountEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    AllowBackorder = table.Column<bool>(type: "bit", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: true),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: true),
                    WeightInKg = table.Column<double>(type: "float", nullable: false),
                    WidthInCm = table.Column<double>(type: "float", nullable: false),
                    HeightInCm = table.Column<double>(type: "float", nullable: false),
                    LengthInCm = table.Column<double>(type: "float", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsNewArrival = table.Column<bool>(type: "bit", nullable: false),
                    IsTrending = table.Column<bool>(type: "bit", nullable: false),
                    MetaTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    SoldCount = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_SubCategory_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentDue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecurringInvoiceId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExternalReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Companies_CompanyId1",
                        column: x => x.CompanyId1,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_OrderHeaders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderHeaders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderHeaderId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    User = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderActivityLogs_OrderHeaders_OrderHeaderId",
                        column: x => x.OrderHeaderId,
                        principalTable: "OrderHeaders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderHeaderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetails_OrderHeaders_OrderHeaderId",
                        column: x => x.OrderHeaderId,
                        principalTable: "OrderHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSpecification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSpecification_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTag_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VariantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    DiscountPrice = table.Column<double>(type: "float", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariant_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttachmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceAttachments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Service = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxDetails_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Brand",
                columns: new[] { "Id", "Country", "CreatedBy", "CreatedDate", "Description", "EstablishedYear", "IsActive", "IsDeleted", "LogoUrl", "Name", "Slug", "UpdatedBy", "UpdatedDate", "WebsiteUrl" },
                values: new object[,]
                {
                    { 1, "United States", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1383), "Innovative technology solutions.", 2005, true, false, "/images/brands/tech-solutions-logo.png", "Tech Solutions", "tech-solutions", null, null, "https://www.techsolutions.com" },
                    { 2, "France", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1400), "Trendy clothing.", 2010, true, false, "/images/brands/fashion-forward-logo.png", "Fashion Forward", "fashion-forward", null, null, "https://www.fashionforward.com" },
                    { 3, "Canada", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1403), "Eco-friendly products.", 2015, true, false, "/images/brands/green-living-logo.png", "Green Living", "green-living", null, null, "https://www.greenliving.com" },
                    { 4, "United Kingdom", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1405), "Quality books.", 1995, true, false, "/images/brands/global-reads-logo.png", "Global Reads", "global-reads", null, null, "https://www.globalreads.com" },
                    { 5, "Australia", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1408), "Outdoor equipment.", 2008, true, false, "/images/brands/adventure-gear-logo.png", "Adventure Gear", "adventure-gear", null, null, "https://www.adventuregear.com" },
                    { 6, "South Korea", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1410), "Premium beauty products.", 2012, true, false, "/images/brands/glow-and-glam-logo.png", "Glow & Glam", "glow-and-glam", null, null, "https://www.glowandglam.com" },
                    { 7, "Germany", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1412), "Creative toys.", 2000, true, false, "/images/brands/fun-time-toys-logo.png", "Fun Time Toys", "fun-time-toys", null, null, "https://www.funtimetoys.com" },
                    { 8, "United States", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1414), "Health and wellness products.", 2018, true, false, "/images/brands/health-hub-logo.png", "Health Hub", "health-hub", null, null, "https://www.healthhub.com" },
                    { 9, "United States", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1416), "Pet care essentials.", 2016, true, false, "/images/brands/pet-paradise-logo.png", "Pet Paradise", "pet-paradise", null, null, "https://www.petparadise.com" },
                    { 10, "Italy", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1418), "Premium gourmet foods.", 2007, true, false, "/images/brands/gourmet-delights-logo.png", "Gourmet Delights", "gourmet-delights", null, null, "https://www.gourmetdelights.com" },
                    { 11, "Canada", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1420), "Eco-friendly furniture for modern homes.", 2018, true, false, "/images/brands/green-living-furniture-logo.png", "Green Living Furniture", "green-living-furniture", null, null, "https://www.greenliving.com/furniture" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Description", "DisplayOrder", "IsActive", "IsDeleted", "Name", "ParentCategoryId", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1773), "Explore the latest gadgets and electronic devices.", 1, true, false, "Electronics", null, null, null },
                    { 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1776), "Discover stylish clothing and accessories for all occasions.", 2, true, false, "Apparel", null, null, null },
                    { 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1778), "Find everything you need for your home and outdoor spaces.", 3, true, false, "Home & Garden", null, null, null },
                    { 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1779), "Immerse yourself in captivating stories and knowledge.", 4, true, false, "Books", null, null, null },
                    { 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1781), "Gear up for your active lifestyle and outdoor adventures.", 5, true, false, "Sports & Outdoors", null, null, null },
                    { 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1782), "Enhance your natural beauty and well-being.", 6, true, false, "Beauty & Personal Care", null, null, null },
                    { 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1783), "Unleash fun and creativity for all ages.", 7, true, false, "Toys & Games", null, null, null },
                    { 8, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1785), "Support your health with quality supplements and devices.", 8, true, false, "Health & Wellness", null, null, null },
                    { 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1786), "Everything you need for your furry friends.", 9, true, false, "Pet Supplies", null, null, null },
                    { 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1787), "Savor gourmet foods and premium drinks.", 10, true, false, "Food & Beverages", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "City", "CreatedBy", "CreatedDate", "IsDeleted", "Name", "PhoneNumber", "PostalCode", "State", "StreetAddress", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "Silicon City", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3989), false, "Tech Solutions Inc.", "555-123-4567", "94016", "CA", "123 Innovation Way", null, null },
                    { 2, "Fashionville", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3994), false, "Fashion Forward Ltd.", "212-987-6543", "10001", "NY", "456 Style Avenue", null, null },
                    { 3, "Eco City", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3996), false, "Green Living Co.", "404-555-7890", "30303", "GA", "789 Earth Street", null, null },
                    { 4, "Booktown", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3998), false, "Global Reads", "312-555-1122", "60602", "IL", "101 Literary Lane", null, null },
                    { 5, "Outdoorsville", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4000), false, "Adventure Gear Corp.", "720-555-3344", "80202", "CO", "222 Trail Road", null, null },
                    { 6, "Cosmetic City", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4001), false, "Glow & Glam", "310-555-0011", "90210", "CA", "333 Radiant Road", null, null },
                    { 7, "Toyland", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4003), false, "Fun Time Toys", "718-555-9988", "11201", "NY", "444 Playful Place", null, null },
                    { 8, "Healthville", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4005), false, "Health Hub Inc.", "713-555-2233", "77002", "TX", "555 Wellness Way", null, null },
                    { 9, "Pettown", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4006), false, "Pet Paradise", "305-555-4455", "33101", "FL", "666 Paw Street", null, null },
                    { 10, "Foodcity", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4008), false, "Gourmet Delights", "415-555-6677", "94105", "CA", "777 Flavor Lane", null, null }
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "CreatedBy", "CreatedDate", "IsDeleted", "Name", "Symbol", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "USD", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5828), false, "US Dollar", "$", null, null },
                    { 2, "EUR", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5831), false, "Euro", "€", null, null },
                    { 3, "GBP", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5833), false, "British Pound", "£", null, null },
                    { 4, "CAD", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5834), false, "Canadian Dollar", "C$", null, null },
                    { 5, "AUD", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5835), false, "Australian Dollar", "A$", null, null },
                    { 6, "JPY", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5836), false, "Japanese Yen", "¥", null, null },
                    { 7, "INR", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5837), false, "Indian Rupee", "₹", null, null },
                    { 8, "CHF", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5839), false, "Swiss Franc", "Fr", null, null }
                });

            migrationBuilder.InsertData(
                table: "Timezones",
                columns: new[] { "Id", "Abbreviation", "CreatedBy", "CreatedDate", "IsDeleted", "Name", "UpdatedBy", "UpdatedDate", "UtcOffset", "UtcOffsetString" },
                values: new object[,]
                {
                    { 1, "EST", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5897), false, "America/New_York", null, null, "-05:00", "EST" },
                    { 2, "GMT", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5902), false, "Europe/London", null, null, "+00:00", "GMT" },
                    { 3, "JST", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5904), false, "Asia/Tokyo", null, null, "+09:00", "JST" },
                    { 4, "AEDT", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5905), false, "Australia/Sydney", null, null, "+10:00", "AEDT" },
                    { 5, "EST", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5906), false, "America/Toronto", null, null, "-05:00", "EST" },
                    { 6, "CET", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5908), false, "Europe/Paris", null, null, "+01:00", "CET" },
                    { 7, "IST", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5909), false, "Asia/Mumbai", null, null, "+05:30", "IST" },
                    { 8, "CET", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5910), false, "Europe/Zurich", null, null, "+01:00", "CET" }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "CompanyId", "CreatedBy", "CreatedDate", "Email", "IsDeleted", "Name", "PhoneNumber", "UpdatedBy", "UpdatedDate", "Address1", "Address2", "City", "Country", "State", "ZipCode" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5969), "john.doe@example.com", false, "John Doe", "555-0101", null, null, "123 Maple St", "Apt 4B", "Springfield", "USA", "IL", "62701" },
                    { 2, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5973), "jane.smith@example.com", false, "Jane Smith", "555-0102", null, null, "456 Oak Ave", "Suite 201", "London", "UK", "Greater London", "SW1A 1AA" },
                    { 3, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5975), "hiroshi.tanaka@example.com", false, "Hiroshi Tanaka", "555-0103", null, null, "789 Sakura St", "2 Chome-1-1", "Tokyo", "Japan", "Tokyo", "100-0001" },
                    { 4, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5976), "emma.brown@example.com", false, "Emma Brown", "555-0104", null, null, "101 Pine Rd", "Level 5", "Sydney", "Australia", "NSW", "2000" },
                    { 5, 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5977), "liam.johnson@example.com", false, "Liam Johnson", "555-0105", null, null, "202 Birch Ln", "Unit 12", "Toronto", "Canada", "ON", "M5V 2T7" },
                    { 6, 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5978), "sophie.martin@example.com", false, "Sophie Martin", "555-0106", null, null, "303 Cedar St", "Batiment C", "Paris", "France", "Île-de-France", "75001" },
                    { 7, 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5980), "arjun.patel@example.com", false, "Arjun Patel", "555-0107", null, null, "404 Elm Dr", "Near Main Gate", "Mumbai", "India", "MH", "400001" },
                    { 8, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5981), "clara.fischer@example.com", false, "Clara Fischer", "555-0108", null, null, "505 Spruce Ct", "Block A", "Zurich", "Switzerland", "Zurich", "8001" }
                });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "CompanyId", "CreatedBy", "CreatedDate", "CurrencyId", "IsDeleted", "Name", "TimezoneId", "UpdatedBy", "UpdatedDate", "Address1", "Address2", "City", "Country", "State", "ZipCode" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(6765), 1, false, "Silicon City Office", 1, null, null, "123 Innovation Way", "Tech Park, Suite 100", "Silicon City", "USA", "CA", "94016" },
                    { 2, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(6769), 1, false, "Fashionville Store", 1, null, null, "456 Style Avenue", "Fashion Mall, Unit 22", "Fashionville", "USA", "NY", "10001" },
                    { 3, 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(6771), 1, false, "Eco City Warehouse", 1, null, null, "789 Earth Street", "Industrial Zone, Gate 5", "Eco City", "USA", "GA", "30303" },
                    { 4, 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(6772), 3, false, "London Bookstore", 2, null, null, "101 Literary Lane", "Off Charing Cross Rd", "London", "UK", "London", "WC1B 3PA" },
                    { 5, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(6774), 5, false, "Sydney Outlet", 4, null, null, "222 Trail Road", "Near Blue Mountains Entry", "Sydney", "Australia", "NSW", "2000" },
                    { 6, 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(6775), 2, false, "Paris Boutique", 6, null, null, "333 Radiant Road", "Galerie Vivienne", "Paris", "France", "Paris", "75002" },
                    { 7, 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(6776), 7, false, "Mumbai Store", 7, null, null, "444 Playful Place", "Linking Road, Bandra", "Mumbai", "India", "MH", "400002" },
                    { 8, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(6778), 8, false, "Zurich Tech Hub", 8, null, null, "555 Tech Park", "Innovation Center, Floor 3", "Zurich", "Switzerland", "", "8002" }
                });

            migrationBuilder.InsertData(
                table: "SubCategory",
                columns: new[] { "Id", "CategoryId", "CreatedBy", "CreatedDate", "Description", "DisplayOrder", "IsActive", "IsDeleted", "Name", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1958), "The latest smartphones from top brands.", 0, true, false, "Smartphones", null, null },
                    { 2, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1961), "Powerful laptops for work and play.", 0, true, false, "Laptops", null, null },
                    { 3, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1963), "High-definition televisions for home entertainment.", 0, true, false, "Televisions", null, null },
                    { 4, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1964), "Stylish clothing for men.", 0, true, false, "Menswear", null, null },
                    { 5, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1965), "Trendy clothing for women.", 0, true, false, "Womenswear", null, null },
                    { 6, 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1966), "Essential appliances for your kitchen.", 0, true, false, "Kitchen Appliances", null, null },
                    { 7, 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1967), "Tools for maintaining your garden.", 0, true, false, "Garden Tools", null, null },
                    { 8, 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(1968), "Imaginative and engaging fictional works.", 0, true, false, "Fiction", null, null },
                    { 9, 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3772), "Informative and factual books on various topics.", 0, true, false, "Non-Fiction", null, null },
                    { 10, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3798), "Gear for your outdoor adventures.", 0, true, false, "Camping & Hiking", null, null },
                    { 11, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3800), "Equipment and accessories for your fitness journey.", 0, true, false, "Fitness", null, null },
                    { 12, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3801), "High-quality audio devices and accessories.", 0, true, false, "Audio & Headphones", null, null },
                    { 13, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3802), "Fashionable accessories to complete your look.", 0, true, false, "Accessories", null, null },
                    { 14, 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3803), "Stylish and functional furniture for your home.", 0, true, false, "Furniture", null, null },
                    { 15, 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3804), "Engaging books for young readers.", 0, true, false, "Children’s Books", null, null },
                    { 16, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3806), "Bikes and accessories for cycling enthusiasts.", 0, true, false, "Cycling", null, null },
                    { 17, 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3807), "Products for radiant and healthy skin.", 0, true, false, "Skincare", null, null },
                    { 18, 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3808), "Solutions for strong and shiny hair.", 0, true, false, "Haircare", null, null },
                    { 19, 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3809), "Nutritious food for pets.", 0, true, false, "Pet Food", null, null },
                    { 20, 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3810), "Fun toys for your pets.", 0, true, false, "Pet Toys", null, null },
                    { 21, 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(3812), "Premium snacks for every occasion.", 0, true, false, "Gourmet Snacks", null, null }
                });

            migrationBuilder.InsertData(
                table: "Vendors",
                columns: new[] { "Id", "CompanyId", "CreatedBy", "CreatedDate", "Email", "IsDeleted", "PhoneNumber", "UpdatedBy", "UpdatedDate", "VendorName", "VendorPictureUrl" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4070), "vendor@techsolutions.com", false, "555-123-4567", null, null, "Tech Solutions Vendor", "/images/vendors/tech-solutions.png" },
                    { 2, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4075), "vendor@fashionforward.com", false, "212-987-6543", null, null, "Fashion Forward Vendor", "/images/vendors/fashion-forward.png" },
                    { 3, 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4077), "vendor@greenliving.com", false, "404-555-7890", null, null, "Green Living Vendor", "/images/vendors/green-living.png" },
                    { 4, 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4078), "vendor@globalreads.com", false, "312-555-1122", null, null, "Global Reads Vendor", "/images/vendors/global-reads.png" },
                    { 5, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4080), "vendor@adventuregear.com", false, "720-555-3344", null, null, "Adventure Gear Vendor", "/images/vendors/adventure-gear.png" },
                    { 6, 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4081), "vendor@glowandglam.com", false, "310-555-0011", null, null, "Glow & Glam Vendor", "/images/vendors/glow-and-glam.png" },
                    { 7, 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4083), "vendor@funtimetoys.com", false, "718-555-9988", null, null, "Fun Time Toys Vendor", "/images/vendors/fun-time-toys.png" },
                    { 8, 8, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4084), "vendor@healthhub.com", false, "713-555-2233", null, null, "Health Hub Vendor", "/images/vendors/health-hub.png" },
                    { 9, 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4086), "vendor@petparadise.com", false, "305-555-4455", null, null, "Pet Paradise Vendor", "/images/vendors/pet-paradise.png" },
                    { 10, 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4088), "vendor@gourmetdelights.com", false, "415-555-6677", null, null, "Gourmet Delights Vendor", "/images/vendors/gourmet-delights.png" },
                    { 11, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4089), "vendor@techtrend.com", false, "510-555-8899", null, null, "TechTrend Innovations", "/images/vendors/techtrend.png" },
                    { 12, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4091), "vendor@ecostyle.com", false, "646-555-1122", null, null, "EcoStyle Outfitters", "/images/vendors/ecostyle.png" }
                });

            migrationBuilder.InsertData(
                table: "OrderHeaders",
                columns: new[] { "Id", "BillingAddress1", "BillingAddress2", "BillingCity", "BillingCountry", "BillingState", "BillingZipCode", "AmountDue", "AmountPaid", "ApplicationUserId", "Carrier", "CreatedBy", "CreatedDate", "CustomerId", "CustomerNotes", "DeliveryMethod", "DeliveryStatus", "Discount", "EstimatedDelivery", "IsDeleted", "OrderDate", "OrderStatus", "OrderTotal", "PaymentDate", "PaymentDueDate", "PaymentIntentId", "PaymentMethod", "PaymentStatus", "SessionId", "ShippingCharges", "ShippingContactName", "ShippingContactPhone", "ShippingDate", "ShippingMethod", "Subtotal", "Tax", "TrackingNumber", "TrackingUrl", "TransactionId", "UpdatedBy", "UpdatedDate", "ShippingAddress1", "ShippingAddress2", "ShippingCity", "ShippingCountry", "ShippingState", "ShippingZipCode" },
                values: new object[,]
                {
                    { 1, "123 Maple St", "Suite 100", "Springfield", "USA", "IL", "94016", 0.00m, 649.99m, null, "UPS", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7123), 1, "Deliver to front porch", "Ground", "InTransit", 13.00m, new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipped", 649.99m, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2025, 4, 30), null, "CreditCard", "Paid", null, 15.00m, "John Doe", "555-0101", new DateTime(2025, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Standard", 599.99m, 48.00m, "TRK123456", null, null, null, null, "123 Maple St", "Suite 100", "Springfield", "USA", "IL", "94016" },
                    { 2, "456 Style Avenue", "Oak Ave", "London", "USA", "NY", "10001", 129.59m, 0.00m, null, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7153), 2, null, "Air", "Pending", 0.00m, new DateTime(2025, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Processing", 129.59m, null, new DateOnly(2025, 5, 2), null, "PayPal", "Pending", null, 0.00m, "Jane Smith", "555-0102", null, "Express", 119.99m, 9.60m, null, null, null, null, null, "456 Style Avenue", "Oak Ave", "London", "USA", "NY", "10001" },
                    { 3, "789 Earth Street", "Industrial Zone Ave ", "Eco City", "USA", "GA", "30303", 0.00m, 799.99m, null, "FedEx", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7164), 2, "Leave at reception", "Ground", "Delivered", 14.88m, new DateTime(2025, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered", 799.99m, new DateTime(2025, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2025, 5, 3), null, "CreditCard", "Paid", null, 20.00m, "Hiroshi Tanaka", "555-0103", new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Standard", 735.99m, 58.88m, "TRK789012", null, null, null, null, "789 Earth Street", "Industrial Zone Ave ", "Eco City", "USA", "GA", "30303" },
                    { 4, "101 Literary Lane", "Off Charing Cross Rd", "London", "UK", "London", "WC1B 3PA", 0.00m, 108.00m, null, "DHL", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7173), 3, null, "Air", "InTransit", 0.00m, new DateTime(2025, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipped", 108.00m, new DateTime(2025, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2025, 5, 4), null, "DebitCard", "Paid", null, 0.00m, "Emma Brown", "555-0104", new DateTime(2025, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Express", 100.00m, 8.00m, "TRK345678", null, null, null, null, "101 Literary Lane", "Off Charing Cross Rd", "London", "UK", "London", "WC1B 3PA" },
                    { 5, "222 Trail Road", "Near Blue Mountains Entry", "Sydney", "Australia", "NSW", "2000", 75.60m, 0.00m, null, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7272), 4, "Fragile items", "Ground", "Pending", 0.00m, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Processing", 75.60m, null, new DateOnly(2025, 5, 5), null, "PayPal", "Pending", null, 0.00m, "Liam Johnson", "555-0105", null, "Standard", 70.00m, 5.60m, null, null, null, null, null, "222 Trail Road", "Near Blue Mountains Entry", "Sydney", "Australia", "NSW", "2000" },
                    { 6, "333 Radiant Road", "Galerie Vivienne", "Paris", "France", "Paris", "75002", 0.00m, 70.19m, null, "UPS", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7288), 4, null, "Ground", "InTransit", 0.00m, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shipped", 70.19m, new DateTime(2025, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2025, 5, 6), null, "CreditCard", "Paid", null, 0.00m, "Sophie Martin", "555-0106", new DateTime(2025, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Standard", 64.99m, 5.20m, "TRK901234", null, null, null, null, "333 Radiant Road", "Galerie Vivienne", "Paris", "France", "Paris", "75002" },
                    { 7, "444 Playful Place", "Linking Road, Bandra", "Mumbai", "India", "MH", "400002", 0.00m, 16.19m, null, "FedEx", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7296), 4, "Urgent delivery", "Air", "Delivered", 0.00m, new DateTime(2025, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delivered", 16.19m, new DateTime(2025, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2025, 5, 7), null, "DebitCard", "Paid", null, 0.00m, "Arjun Patel", "555-0107", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Express", 14.99m, 1.20m, "TRK567890", null, null, null, null, "444 Playful Place", "Linking Road, Bandra", "Mumbai", "India", "MH", "400002" },
                    { 8, "555 Tech Park", "Innovation Center, Floor 3", "Zurich", "Switzerland", "", "8002", 151.19m, 0.00m, null, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7303), 5, null, "Ground", "Pending", 0.00m, new DateTime(2025, 4, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Processing", 151.19m, null, new DateOnly(2025, 5, 8), null, "CreditCard", "Pending", null, 0.00m, "Clara Fischer", "555-0108", null, "Standard", 139.99m, 11.20m, null, null, null, null, null, "555 Tech Park", "Innovation Center, Floor 3", "Zurich", "Switzerland", "", "8002" },
                    { 9, "777 Skyline Boulevard", "Sky Tower, Apt 905", "Toronto", "Canada", "ON", "M5V 2T6", 151.19m, 0.00m, null, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(7311), 6, "Gift wrap required", "Ground", "Pending", 0.00m, new DateTime(2025, 4, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Processing", 151.19m, null, new DateOnly(2025, 5, 8), null, "PayPal", "Pending", null, 0.00m, "Clara Fischer", "555-0109", null, "Standard", 139.99m, 11.20m, null, null, null, null, null, "777 Skyline Boulevard", "Sky Tower, Apt 905", "Toronto", "Canada", "ON", "M5V 2T6" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AllowBackorder", "AverageRating", "Barcode", "BrandId", "CategoryId", "CreatedBy", "CreatedDate", "Description", "DiscountEndDate", "DiscountPrice", "DiscountStartDate", "HeightInCm", "IsActive", "IsDeleted", "IsDiscounted", "IsFeatured", "IsNewArrival", "IsTrending", "LengthInCm", "MetaDescription", "MetaTitle", "Price", "SKU", "ShortDescription", "SoldCount", "StockQuantity", "SubCategoryId", "Title", "TotalReviews", "UpdatedBy", "UpdatedDate", "VendorId", "Views", "WeightInKg", "WidthInCm" },
                values: new object[,]
                {
                    { 1, false, 4.7000000000000002, "789012345678", 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4248), "Immerse yourself in stunning visuals with this 55-inch 4K Ultra HD Smart TV. Featuring High Dynamic Range (HDR) for vibrant colors and deep contrast, built-in Wi-Fi, and access to all your favorite streaming apps. Enjoy a cinematic experience in the comfort of your living room.", new DateTime(2025, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 219.99000000000001, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 71.200000000000003, true, false, true, true, false, true, 8.3000000000000007, "Shop our 55-inch 4K UHD Smart TV with HDR technology for the ultimate home entertainment experience.", "55 inch 4K Smart TV | Tech Solutions", 699.99000000000001, "ELC-SMTV-001", "55-inch 4K Smart TV with HDR and built-in streaming apps", 120, 50, 3, "Smart TV 55 inch 4K UHD with HDR", 85, null, null, 1, 2500, 15.5, 123.5 },
                    { 2, true, 4.5, "456789012345", 2, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4267), "Experience ultimate comfort with our premium 100% combed cotton men's t-shirt. Designed for a classic fit and exceptional softness, this navy blue tee is a versatile wardrobe staple perfect for everyday wear. Available in multiple sizes.", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 16.989999999999998, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 70.0, true, false, true, false, true, false, 0.5, "Classic fit men's navy blue t-shirt made from 100% premium combed cotton for all-day comfort.", "Men's Premium Cotton T-Shirt | Fashion Forward", 24.0, "APP-MTSRT-002-NVY", "Premium soft cotton t-shirt for men in navy blue", 250, 30, 4, "Premium Cotton T-Shirt - Men's (Navy Blue)", 45, null, null, 2, 1800, 0.20000000000000001, 50.0 },
                    { 3, false, 4.7999999999999998, "123456789012", 3, 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4276), "Get your gardening tasks done with ease using our durable 3-piece garden tool set. Includes a sturdy trowel, hand fork, and cultivator, all featuring comfortable wooden handles for a secure grip. Perfect for both novice and experienced gardeners.", null, 40.0, null, 30.0, true, false, false, false, true, false, 5.0, "High-quality 3-piece garden tool set with comfortable wooden handles for all your gardening needs.", "Essential Garden Tool Set | Green Living", 40.0, "HGN-TLSET-003-WD", "3-piece garden tool set with wooden handles", 45, 30, 7, "Essential Garden Tool Set (3-Piece with Wooden Handles)", 28, null, null, 3, 950, 0.90000000000000002, 12.0 },
                    { 4, false, 4.5999999999999996, "567890123456", 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4281), "Experience lightning-fast performance with our ultra-slim 15.6-inch laptop. Featuring a powerful processor, 512GB SSD storage, and 16GB RAM for seamless multitasking. The vibrant Full HD display and long-lasting battery make it perfect for work and or entertainment on the go.", new DateTime(2025, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 899.99000000000001, new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1.8, true, false, true, true, true, true, 24.199999999999999, "Powerful and portable 15.6-inch laptop with SSD storage for fast performance anywhere you go.", "Ultra-Slim 15.6\" Laptop | Tech Solutions", 999.99000000000001, "ELC-LPTOP-004", "15.6\" ultra-slim laptop with SSD and powerful performance", 85, 35, 2, "Ultra-Slim Laptop 15.6\" with SSD", 62, null, null, 1, 3200, 1.8, 35.600000000000001 },
                    { 5, false, 4.7999999999999998, "345678901234", 2, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4287), "Add a touch of elegance to any outfit with our exclusive designer leather handbag. Crafted from premium genuine leather with stylish gold-tone hardware and multiple interior compartments for organization. The adjustable shoulder strap and handle offer versatile carrying options.", new DateTime(2025, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 129.99000000000001, new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 25.0, true, false, true, true, false, true, 12.0, "Elegant black leather handbag with multiple compartments and versatile carrying options.", "Designer Black Leather Handbag | Fashion Forward", 149.99000000000001, "APP-HBAG-005-BLK", "Premium black leather handbag with gold accents", 68, 25, 5, "Designer Leather Handbag - Women's (Black)", 45, null, null, 2, 1950, 0.80000000000000004, 35.0 },
                    { 6, false, 4.9000000000000004, "234567890123", 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4294), "Capture life's special moments with exceptional clarity using our premium DSLR camera. Features a high-quality 24.1 megapixel CMOS sensor, 4K video recording, and includes a versatile 18-55mm lens. Perfect for both amateur and professional photography enthusiasts.", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 799.99000000000001, new DateTime(2025, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 10.0, true, false, true, true, false, false, 7.7999999999999998, "Professional-grade DSLR camera with 24.1MP sensor and 4K video capability for stunning photos and videos.", "Premium DSLR Camera with Lens | Tech Solutions", 899.99000000000001, "ELC-CAM-006", "High-resolution 24.1MP DSLR camera with 4K video and 18-55mm lens", 42, 25, 12, "Premium Digital SLR Camera with 18-55mm Lens", 36, null, null, 1, 1680, 0.69999999999999996, 12.9 },
                    { 7, true, 4.7000000000000002, "890123456789", 3, 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4319), "Indulge in the refreshing taste of premium organic green tea with our exclusive gift set. Includes 6 distinct varieties of hand-picked green tea leaves packaged in elegant tins. Perfect for tea enthusiasts or as a thoughtful gift for any special occasion.", null, 45.0, null, 8.0, true, false, false, false, true, false, 20.0, "Premium selection of 6 organic green tea varieties presented in an elegant gift box.", "Organic Green Tea Gift Set | Green Living", 45.0, "FBD-TEA-007", "Organic green tea gift set of 6 premium varieties", 38, 40, 21, "Organic Green Tea Gift Set (Variety Pack)", 22, null, null, 3, 890, 0.5, 25.0 },
                    { 8, true, 4.5, "978123456789", 4, 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4324), "Dive into suspense with this collection of three gripping mystery novels by bestselling authors. Perfect for fans of thrilling plots and unexpected twists.", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 34.990000000000002, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22.0, true, false, true, false, true, false, 6.0, "Three gripping mystery novels for suspenseful reading.", "Mystery Novel Collection | Global Reads", 39.990000000000002, "BOK-MYST-008", "Set of three mystery novels", 60, 50, 8, "Mystery Novel Collection", 35, null, null, 4, 1200, 1.2, 15.0 },
                    { 9, false, 4.7000000000000002, "123456789014", 5, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4330), "Conquer any trail with these durable waterproof hiking boots. Designed for comfort and traction, featuring breathable materials and ankle support.", new DateTime(2025, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 109.98999999999999, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, true, false, true, true, true, true, 20.0, "Durable waterproof hiking boots for outdoor adventures.", "Waterproof Hiking Boots | Adventure Gear", 129.99000000000001, "SPO-BOOT-009", "Waterproof hiking boots for all terrains", 80, 40, 10, "Waterproof Hiking Boots", 55, null, null, 5, 2100, 1.0, 25.0 },
                    { 10, true, 4.7000000000000002, "678901234567", 6, 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4337), "Turn back the clock with this comprehensive anti-aging skincare collection. This five-piece set includes cleanser, toner, day cream with SPF 30, night serum, and eye cream, all formulated with powerful ingredients for rejuvenation.", new DateTime(2025, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 75.989999999999995, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, true, false, true, true, true, true, 8.0, "Complete 5-piece anti-aging skincare routine for youthful skin.", "Anti-Aging Skincare Collection | Glow & Glam", 89.989999999999995, "BPC-AAGE-010", "Five-piece anti-aging skincare set", 110, 30, 17, "Anti-Aging Skincare Collection Set", 82, null, null, 6, 2800, 0.59999999999999998, 20.0 },
                    { 11, false, 4.9000000000000004, "789012345670", 7, 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4343), "Spark your child's interest in STEM with this interactive learning robot. Programmable via an app, it teaches coding and responds to voice commands, growing with your child’s skills.", new DateTime(2025, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 69.989999999999995, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 22.0, true, false, true, true, true, true, 12.0, "Educational robot for kids aged 6-12 to learn coding and STEM.", "Interactive Learning Robot | Fun Time Toys", 0.0, "TOY-ROBOT-011", "Educational programmable robot for kids Price = 79.99", 135, 25, null, "Interactive Learning Robot for Kids", 98, null, null, 7, 3500, 0.5, 15.0 },
                    { 12, false, 4.5999999999999996, "123456789013", 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4349), "Elevate your smartphone mobile videography with this advanced 3-axis smartphone stabilizer gimbal. Features intelligent tracking and a foldable design for portability.", new DateTime(2025, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 69.989999999999995, new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 19.0, true, false, true, false, true, false, 5.0, "Professional 3-axis gimbal for smooth, professional smartphone videos.", "Smartphone Stabilizer Gimbal | Tech Solutions", 85.0, "ELC-GIMB-012", "Portable 3-axis smartphone gimbal stabilizer", 58, 40, 12, "Smartphone Stabilizer Gimbal", 42, null, null, 1, 1680, 0.40000000000000002, 12.0 },
                    { 13, true, 4.7999999999999998, "234567890124", 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4354), "Transform your home with this smart home starter kit, including a hub, smart plugs, sensors, and smart bulbs, all app-controlled via app.", new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 149.99000000000001, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 25.0, true, false, true, true, true, true, 15.0, "Automate your home with this all-in-one smart home starter kit.", "Smart Home Starter Kit | Tech Solutions", 179.99000000000001, "ELC-SMHM-013", "Complete smart home automation kit", 65, 30, null, "Smart Home Starter Kit", 38, null, null, 1, 2900, 1.2, 30.0 },
                    { 14, true, 4.5999999999999996, "345678901235", 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4361), "Enjoy crystal-clear sound and true wireless freedom with these earbuds. Features noise cancellation, up to 24 hours of battery life, and a compact charging case.", new DateTime(2025, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 49.990000000000002, new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 4.0, true, false, true, true, true, true, 3.0, "High-quality wireless earbuds with noise cancellation and long battery life.", "Wireless Earbuds | Tech Solutions", 59.990000000000002, "ELC-EARB-014", "True wireless earbuds with noise cancellation", 150, 60, 12, "Wireless Earbuds with Charging Case", 95, null, null, 11, 3000, 0.10000000000000001, 8.0 },
                    { 15, true, 4.5, "456789123456", 3, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4366), "Practice yoga with peace of mind on this eco-friendly yoga mat made from sustainable materials. Non-slip surface, lightweight, and extra cushioning for comfort.", new DateTime(2025, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 34.990000000000002, new DateTime(2025, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 183.0, true, false, true, false, true, false, 0.59999999999999998, "Sustainable yoga mat with non-slip surface for comfortable practice.", "Eco-Friendly Yoga Mat | Green Living", 39.990000000000002, "SPO-YOGA-015", "Sustainable non-slip yoga mat", 70, 50, 11, "Eco-Friendly Yoga Mat", 40, null, null, 3, 1400, 1.0, 61.0 },
                    { 16, true, 4.7000000000000002, "567890123457", 2, 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4372), "Elevate your formal attire with this luxury silk tie, handcrafted with intricate patterns and a smooth finish.", null, 45.0, null, 150.0, true, false, false, false, true, false, 1.0, "Premium silk tie for men with elegant design.", "Luxury Silk Tie | Fashion Forward", 45.0, "APP-TIE-016", "Handcrafted luxury silk tie for men", 25, 30, 13, "Luxury Silk Tie", 20, null, null, 12, 900, 0.10000000000000001, 8.0 },
                    { 17, false, 4.7999999999999998, "678901234568", 9, 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4377), "Keep your pet comfortable with this orthopedic pet bed, featuring featuring a cooling gel layer for temperature regulation and support.", new DateTime(2025, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 59.990000000000002, new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.0, true, false, true, true, true, true, 60.0, "Orthopedic pet bed for ultimate pet comfort.", "Cooling Pet Bed | Pet Comfort", 69.989999999999995, "PET-BED-017", "Cooling gel pet bed", 85, 20, null, "Pet Bed with Cooling Gel", 60, null, null, 9, 2000, 2.0, 80.0 },
                    { 18, true, 4.9000000000000004, "789012345679", 10, 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4383), "Indulge in our handcrafted gourmet chocolate truffle box, featuring featuring 24 assorted premium chocolates made with the finest ingredients.", new DateTime(2025, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 24.989999999999998, new DateTime(2025, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 5.0, true, false, true, true, true, true, 20.0, "Handcrafted assorted chocolate truffles for a luxurious treat.", "Gourmet Chocolate Truffle Box | Gourmet Delights", 29.989999999999998, "FOD-CHOC-018", "Box of 24 assorted gourmet truffles", 90, 50, 21, "Gourmet Chocolate Truffle Box", 70, null, null, 10, 2500, 0.5, 20.0 },
                    { 19, false, 4.5999999999999996, "890123456780", 11, 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4398), "Enhance your workspace with this ergonomic high-back office chair, featuring adjustable height, lumbar support, and breathable mesh.", new DateTime(2025, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 129.99000000000001, new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 50.0, true, false, true, false, true, false, 65.0, "Comfortable ergonomic office chair with lumbar support.", "High-Back Office Chair | Green Living", 149.99000000000001, "HMG-CHAIR-019", "Ergonomic high-back office chair", 45, 30, 14, "High-Back Office Chair", 30, null, null, 3, 1100, 15.0, 65.0 },
                    { 20, true, 4.7999999999999998, "901234567891", 4, 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4403), "Ignite your child's love for reading with this collection of adventure stories, designed for children ages aged 6-10 with colorful illustrations.", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 15.99, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 25.0, true, false, true, true, true, true, 2.0, "Exciting adventure stories for young readers with vibrant illustrations.", "Kids’ Adventure Storybook | Global Reads", 19.989999999999998, "BOK-KIDS-020", "Adventure storybook for kids aged 6-10", 65, 40, 15, "Kids’ Adventure Storybook", 45, null, null, 4, 1800, 0.59999999999999998, 20.0 },
                    { 21, false, 4.7000000000000002, "012345678902", 5, 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4409), "Ride with confidence on this high-performance mountain bike, featuring featuring front suspension and durable aluminum frame.", new DateTime(2025, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 349.99000000000001, new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 110.0, true, false, true, true, true, true, 30.0, "High-performance bike with suspension for mountain trails.", "Mountain Bike with Suspension | Adventure Gear", 399.99000000000001, "SPO-BIKE-021", "High-performance mountain bike", 75, 20, 16, "Mountain Bike with Suspension", 50, null, null, 5, 2200, 14.0, 70.0 },
                    { 22, true, 4.5999999999999996, "123456789015", 9, 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4415), "Provide your dog with nutritious, organic food made from high-quality ingredients, free from artificial additives.", new DateTime(2025, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 24.989999999999998, new DateTime(2025, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 40.0, true, false, true, false, true, false, 15.0, "Nutritious organic dog food for your pet’s health.", "Organic Dog Food | Pet Paradise", 29.989999999999998, "PET-FOOD-022", "Organic dog food (5kg)", 80, 50, 19, "Organic Dog Food (5kg)", 55, null, null, 9, 1700, 5.0, 30.0 },
                    { 23, true, 4.7000000000000002, "234567890126", 6, 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4421), "Nourish your hair with this natural shampoo and conditioner set, formulated with organic ingredients for for healthy scalp and hair.", new DateTime(2025, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 19.989999999999998, new DateTime(2025, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 20.0, true, false, true, true, true, true, 8.0, "Organic shampoo and conditioner for healthy hair and scalp.", "Natural Shampoo & Conditioner | Glow & Glam", 24.989999999999998, "BPC-SHAMP-023", "Organic shampoo and conditioner set", 100, 40, 18, "Natural Shampoo & Conditioner Set", 70, null, null, 6, 1900, 0.80000000000000004, 15.0 }
                });

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "Id", "CompanyId", "CompanyId1", "CreatedBy", "CreatedDate", "CustomerId", "Discount", "ExternalReference", "InvoiceNumber", "InvoiceType", "IsDeleted", "IssueDate", "LocationId", "Notes", "OrderId", "PONumber", "PaidAmount", "PaymentDue", "PaymentMethod", "PaymentTerms", "RecurringInvoiceId", "ShippingAmount", "Status", "Subtotal", "Tax", "TotalAmount", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, 1, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8290), 1, 0m, "REF-001", "INV-2025-001", 0, false, new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Thank you for your purchase!", 1, "PO-001", 721.99m, new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Credit Card", "Net 30", null, 20.00m, 3, 649.99m, 52.00m, 721.99m, null, null },
                    { 2, 2, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8305), 2, 10.00m, "REF-002", "INV-2025-002", 0, false, new DateTime(2025, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Please pay by due date.", 2, "PO-002", 0m, new DateTime(2025, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bank Transfer", "Net 30", null, 15.00m, 1, 129.99m, 10.40m, 145.39m, null, null },
                    { 3, 1, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8312), 3, 0m, "REF-003", "INV-2025-003", 2, false, new DateTime(2025, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Proforma invoice for approval.", 3, "PO-003", 888.99m, new DateTime(2025, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Credit Card", "Net 30", null, 25.00m, 3, 799.99m, 64.00m, 888.99m, null, null },
                    { 4, 5, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8319), 4, 0m, "REF-004", "INV-2025-004", 0, false, new DateTime(2025, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Partial payment received.", 4, "PO-004", 50.00m, new DateTime(2025, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bank Transfer", "Net 30", null, 10.00m, 2, 109.95m, 8.80m, 128.75m, null, null },
                    { 5, 6, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8325), 5, 0m, "REF-005", "INV-2025-005", 1, false, new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, "Recurring invoice for monthly subscription.", 5, "PO-005", 0m, new DateTime(2025, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Direct Debit", "Net 30", null, 0m, 0, 75.99m, 6.08m, 82.07m, null, null },
                    { 6, 7, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8330), 6, 0m, "REF-006", "INV-2025-006", 0, false, new DateTime(2025, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, "Payment overdue, please settle ASAP.", 6, "PO-006", 0m, new DateTime(2025, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Credit Card", "Net 30", null, 8.00m, 4, 69.99m, 5.60m, 83.59m, null, null },
                    { 7, 4, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8335), 7, 0m, "REF-007", "INV-2025-007", 3, false, new DateTime(2025, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Credit note for returned item.", 7, "PO-007", -17.27m, new DateTime(2025, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Refund", "Immediate", null, 0m, 3, -15.99m, -1.28m, -17.27m, null, null },
                    { 8, 1, null, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8343), 8, 0m, "REF-008", "INV-2025-008", 0, false, new DateTime(2025, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, "Invoice for recent purchase.", 8, "PO-008", 0m, new DateTime(2025, 5, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bank Transfer", "Net 30", null, 15.00m, 1, 149.99m, 12.00m, 176.99m, null, null }
                });

            migrationBuilder.InsertData(
                table: "OrderActivityLogs",
                columns: new[] { "Id", "ActivityType", "Description", "Details", "OrderHeaderId", "Timestamp", "User" },
                values: new object[,]
                {
                    { 1, 0, "Order placed by customer", "{\"CustomerId\": 1}", 1, new DateTime(2025, 4, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "john.doe" },
                    { 2, 3, "Payment completed via CreditCard", "{\"Amount\": 649.99}", 1, new DateTime(2025, 4, 1, 10, 5, 0, 0, DateTimeKind.Unspecified), "john.doe" },
                    { 3, 5, "Order shipped via UPS", "{\"TrackingNumber\": \"TRK123456\"}", 1, new DateTime(2025, 4, 3, 9, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { 4, 0, "Order placed by customer", "{\"CustomerId\": 2}", 2, new DateTime(2025, 4, 2, 11, 0, 0, 0, DateTimeKind.Unspecified), "jane.smith" },
                    { 5, 0, "Order placed by customer", "{\"CustomerId\": 2}", 3, new DateTime(2025, 4, 3, 12, 0, 0, 0, DateTimeKind.Unspecified), "hiroshi.tanaka" },
                    { 6, 3, "Payment completed via CreditCard", "{\"Amount\": 799.99}", 3, new DateTime(2025, 4, 3, 12, 10, 0, 0, DateTimeKind.Unspecified), "hiroshi.tanaka" },
                    { 7, 5, "Order shipped via FedEx", "{\"TrackingNumber\": \"TRK789012\"}", 3, new DateTime(2025, 4, 5, 8, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { 8, 0, "Order placed by customer", "{\"CustomerId\": 3}", 4, new DateTime(2025, 4, 4, 14, 0, 0, 0, DateTimeKind.Unspecified), "emma.brown" },
                    { 9, 3, "Payment completed via DebitCard", "{\"Amount\": 109.95}", 4, new DateTime(2025, 4, 4, 14, 5, 0, 0, DateTimeKind.Unspecified), "emma.brown" },
                    { 10, 5, "Order shipped via DHL", "{\"TrackingNumber\": \"TRK345678\"}", 4, new DateTime(2025, 4, 6, 10, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { 11, 0, "Order placed by customer", "{\"CustomerId\": 4}", 5, new DateTime(2025, 4, 5, 15, 0, 0, 0, DateTimeKind.Unspecified), "liam.johnson" },
                    { 12, 0, "Order placed by customer", "{\"CustomerId\": 4}", 6, new DateTime(2025, 4, 6, 16, 0, 0, 0, DateTimeKind.Unspecified), "sophie.martin" },
                    { 13, 3, "Payment completed via CreditCard", "{\"Amount\": 69.99}", 6, new DateTime(2025, 4, 6, 16, 5, 0, 0, DateTimeKind.Unspecified), "sophie.martin" },
                    { 14, 5, "Order shipped via UPS", "{\"TrackingNumber\": \"TRK901234\"}", 6, new DateTime(2025, 4, 8, 11, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { 15, 0, "Order placed by customer", "{\"CustomerId\": 4}", 7, new DateTime(2025, 4, 7, 17, 0, 0, 0, DateTimeKind.Unspecified), "arjun.patel" },
                    { 16, 3, "Payment completed via DebitCard", "{\"Amount\": 15.99}", 7, new DateTime(2025, 4, 7, 17, 5, 0, 0, DateTimeKind.Unspecified), "arjun.patel" },
                    { 17, 5, "Order shipped via FedEx", "{\"TrackingNumber\": \"TRK567890\"}", 7, new DateTime(2025, 4, 9, 9, 0, 0, 0, DateTimeKind.Unspecified), "system" },
                    { 18, 0, "Order placed by customer", "{\"CustomerId\": 5}", 8, new DateTime(2025, 4, 8, 18, 0, 0, 0, DateTimeKind.Unspecified), "clara.fischer" },
                    { 19, 0, "Order placed by customer", "{\"CustomerId\": 6}", 9, new DateTime(2025, 4, 8, 19, 0, 0, 0, DateTimeKind.Unspecified), "clara.fischer" }
                });

            migrationBuilder.InsertData(
                table: "OrderDetails",
                columns: new[] { "Id", "Count", "CreatedBy", "CreatedDate", "IsDeleted", "OrderHeaderId", "Price", "ProductId", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8021), false, 1, 649.99000000000001, 1, null, null },
                    { 2, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8024), false, 2, 129.99000000000001, 5, null, null },
                    { 3, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8026), false, 3, 799.99000000000001, 6, null, null },
                    { 4, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8112), false, 4, 109.95, 9, null, null },
                    { 5, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8114), false, 5, 75.989999999999995, 10, null, null },
                    { 6, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8115), false, 6, 69.989999999999995, 11, null, null },
                    { 7, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8116), false, 7, 15.99, 8, null, null },
                    { 8, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8118), false, 8, 149.99000000000001, 13, null, null },
                    { 9, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8119), false, 9, 149.99000000000001, 13, null, null },
                    { 10, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8121), false, 9, 69.989999999999995, 12, null, null },
                    { 11, 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8122), false, 3, 899.99000000000001, 4, null, null }
                });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "ImageUrl", "IsDeleted", "ProductId", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4586), "/images/products/smarttv_main.jpg", false, 1, null, null },
                    { 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4588), "/images/products/smarttv_side.jpg", false, 1, null, null },
                    { 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4589), "/images/products/smarttv_ports.jpg", false, 1, null, null },
                    { 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4590), "/images/products/tshirt_navy_front.jpg", false, 2, null, null },
                    { 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4591), "/images/products/tshirt_navy_back.jpg", false, 2, null, null },
                    { 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4592), "/images/products/tshirt_navy_detail.jpg", false, 2, null, null },
                    { 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4593), "/images/products/gardentools_set.jpg", false, 3, null, null },
                    { 8, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4594), "/images/products/gardentools_trowel.jpg", false, 3, null, null },
                    { 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4595), "/images/products/gardentools_fork.jpg", false, 3, null, null },
                    { 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4596), "/images/products/laptop_front.jpg", false, 4, null, null },
                    { 11, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4597), "/images/products/laptop_open.jpg", false, 4, null, null },
                    { 12, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4599), "/images/products/laptop_side.jpg", false, 4, null, null },
                    { 13, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4600), "/images/products/handbag_black_front.jpg", false, 5, null, null },
                    { 14, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4602), "/images/products/handbag_black_side.jpg", false, 5, null, null },
                    { 15, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4603), "/images/products/handbag_black_inside.jpg", false, 5, null, null },
                    { 16, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4604), "/images/products/camera_front.jpg", false, 6, null, null },
                    { 17, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4605), "/images/products/camera_side.jpg", false, 6, null, null },
                    { 18, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4606), "/images/products/camera_with_lens.jpg", false, 6, null, null },
                    { 19, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4607), "/images/products/teaset_box.jpg", false, 7, null, null },
                    { 20, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4608), "/images/products/teaset_tins.jpg", false, 7, null, null },
                    { 21, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4609), "/images/products/teaset_varieties.jpg", false, 7, null, null },
                    { 22, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4610), "/images/products/mysterybooks_cover.jpg", false, 8, null, null },
                    { 23, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4611), "/images/products/mysterybooks_stack.jpg", false, 8, null, null },
                    { 24, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4612), "/images/products/mysterybooks_side.jpg", false, 8, null, null },
                    { 25, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4613), "/images/products/hikingboots_pair.jpg", false, 9, null, null },
                    { 26, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4614), "/images/products/hikingboots_sole.jpg", false, 9, null, null },
                    { 27, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4615), "/images/products/hikingboots_side.jpg", false, 9, null, null },
                    { 28, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4616), "/images/products/skincare_set_box.jpg", false, 10, null, null },
                    { 29, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4617), "/images/products/skincare_products.jpg", false, 10, null, null },
                    { 30, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4618), "/images/products/skincare_ingredients.jpg", false, 10, null, null },
                    { 31, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4619), "/images/products/robot_front.jpg", false, 11, null, null },
                    { 32, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4620), "/images/products/robot_side.jpg", false, 11, null, null },
                    { 33, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4621), "/images/products/robot_app.jpg", false, 11, null, null },
                    { 34, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4622), "/images/products/gimbal_main.jpg", false, 12, null, null },
                    { 35, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4624), "/images/products/gimbal_folded.jpg", false, 12, null, null },
                    { 36, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4625), "/images/products/gimbal_in_use.jpg", false, 12, null, null },
                    { 37, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4626), "/images/products/smarthome_kit_box.jpg", false, 13, null, null },
                    { 38, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4627), "/images/products/smarthome_components.jpg", false, 13, null, null },
                    { 39, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4628), "/images/products/smarthome_app.jpg", false, 13, null, null },
                    { 40, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4629), "/images/products/earbuds_case.jpg", false, 14, null, null },
                    { 41, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4630), "/images/products/earbuds_pair.jpg", false, 14, null, null },
                    { 42, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4631), "/images/products/earbuds_charging.jpg", false, 14, null, null },
                    { 43, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4633), "/images/products/yogamat_top.jpg", false, 15, null, null },
                    { 44, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4634), "/images/products/yogamat_rolled.jpg", false, 15, null, null },
                    { 45, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4635), "/images/products/yogamat_texture.jpg", false, 15, null, null },
                    { 46, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4636), "/images/products/tie_pattern.jpg", false, 16, null, null },
                    { 47, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4637), "/images/products/tie_knot.jpg", false, 16, null, null },
                    { 48, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4638), "/images/products/tie_folded.jpg", false, 16, null, null },
                    { 49, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4639), "/images/products/petbed_top.jpg", false, 17, null, null },
                    { 50, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4640), "/images/products/petbed_side.jpg", false, 17, null, null },
                    { 51, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4641), "/images/products/petbed_in_use.jpg", false, 17, null, null },
                    { 52, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4642), "/images/products/chocolates_box.jpg", false, 18, null, null },
                    { 53, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4643), "/images/products/chocolates_assorted.jpg", false, 18, null, null },
                    { 54, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4644), "/images/products/chocolates_closeup.jpg", false, 18, null, null },
                    { 55, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4645), "/images/products/officechair_front.jpg", false, 19, null, null },
                    { 56, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4646), "/images/products/officechair_side.jpg", false, 19, null, null },
                    { 57, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4647), "/images/products/officechair_back.jpg", false, 19, null, null },
                    { 58, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4648), "/images/products/storybook_cover.jpg", false, 20, null, null },
                    { 59, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4649), "/images/products/storybook_pages.jpg", false, 20, null, null },
                    { 60, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4650), "/images/products/storybook_illustrations.jpg", false, 20, null, null },
                    { 61, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4651), "/images/products_mountainbike_frame.jpg", false, 21, null, null },
                    { 62, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4652), "/images/products_mountainbike_side.jpg", false, 21, null, null },
                    { 63, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4653), "/images/products_mountainbike_tires.jpg", false, 21, null, null },
                    { 64, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4654), "/images/products/dogfood_bag.jpg", false, 22, null, null },
                    { 65, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4656), "/images/products/dogfood_contents.jpg", false, 22, null, null },
                    { 66, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4657), "/images/products/dogfood_dog.jpg", false, 22, null, null },
                    { 67, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4658), "/images/products/shampoo_bottles.jpg", false, 23, null, null },
                    { 68, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4659), "/images/products/shampoo_ingredients.jpg", false, 23, null, null },
                    { 69, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4660), "/images/products/shampoo_texture.jpg", false, 23, null, null }
                });

            migrationBuilder.InsertData(
                table: "ProductSpecification",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsDeleted", "Key", "ProductId", "UpdatedBy", "UpdatedDate", "Value" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4810), false, "Screen Size", 1, null, null, "55 inches" },
                    { 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4814), false, "Resolution", 1, null, null, "4K Ultra HD (3840 x 2160)" },
                    { 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4815), false, "Display Technology", 1, null, null, "LED" },
                    { 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4817), false, "HDR", 1, null, null, "Yes" },
                    { 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4818), false, "Smart TV", 1, null, null, "Yes" },
                    { 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4819), false, "Connectivity", 1, null, null, "Wi-Fi, Bluetooth, HDMI, USB" },
                    { 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4820), false, "Material", 2, null, null, "100% Combed Cotton" },
                    { 8, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4821), false, "Color", 2, null, null, "Navy Blue" },
                    { 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4822), false, "Care Instructions", 2, null, null, "Machine wash cold, tumble dry low" },
                    { 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4824), false, "Material", 3, null, null, "Stainless Steel with Wooden Handles" },
                    { 11, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4825), false, "Pieces", 3, null, null, "3" },
                    { 12, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4826), false, "Tool Length", 3, null, null, "30 cm" },
                    { 13, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4827), false, "Processor", 4, null, null, "Intel Core i7" },
                    { 14, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4828), false, "RAM", 4, null, null, "16 GB" },
                    { 15, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4830), false, "Storage", 4, null, null, "512 GB SSD" },
                    { 16, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4831), false, "Display", 4, null, null, "15.6-inch Full HD (1920 x 1080)" },
                    { 17, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4832), false, "Battery Life", 4, null, null, "Up to 10 hours" },
                    { 18, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4833), false, "Operating System", 4, null, null, "Windows 11" },
                    { 19, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4834), false, "Material", 5, null, null, "Genuine Leather" },
                    { 20, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4835), false, "Color", 5, null, null, "Black" },
                    { 21, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4837), false, "Dimensions", 5, null, null, "35 x 25 x 12 cm" },
                    { 22, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4838), false, "Hardware", 5, null, null, "Gold-tone" },
                    { 23, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4839), false, "Megapixels", 6, null, null, "24.1 MP" },
                    { 24, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4840), false, "Sensor Type", 6, null, null, "CMOS" },
                    { 25, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4842), false, "Video Resolution", 6, null, null, "4K" },
                    { 26, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4843), false, "Lens", 6, null, null, "18-55mm" },
                    { 27, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4845), false, "ISO Range", 6, null, null, "100-25600" },
                    { 28, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4846), false, "Varieties", 7, null, null, "6" },
                    { 29, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4847), false, "Organic", 7, null, null, "Yes" },
                    { 30, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4848), false, "Weight", 7, null, null, "300g total (50g each)" },
                    { 31, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4849), false, "Packaging", 7, null, null, "Metal tins in gift box" },
                    { 32, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4850), false, "Format", 8, null, null, "Hardcover" },
                    { 33, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4852), false, "Pages", 8, null, null, "384" },
                    { 34, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4853), false, "Genre", 8, null, null, "Historical Fiction" },
                    { 35, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4854), false, "Language", 8, null, null, "English" },
                    { 36, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4855), false, "Material", 9, null, null, "Waterproof Leather and Mesh" },
                    { 37, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4856), false, "Sole", 9, null, null, "Rubber with Multi-directional Traction" },
                    { 38, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4857), false, "Closure", 9, null, null, "Lace-up" },
                    { 39, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4859), false, "Gender", 9, null, null, "Unisex" },
                    { 40, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4860), false, "Pieces", 10, null, null, "5" },
                    { 41, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4861), false, "Skin Type", 10, null, null, "All" },
                    { 42, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4862), false, "Key Ingredients", 10, null, null, "Peptides, Hyaluronic Acid, Antioxidants" },
                    { 43, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4863), false, "SPF", 10, null, null, "30 (Day Cream)" },
                    { 44, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(4864), false, "Age Range", 11, null, null, "6-12 years" },
                    { 45, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5003), false, "Programmable", 11, null, null, "Yes" },
                    { 46, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5005), false, "Battery Life", 11, null, null, "4 hours" },
                    { 47, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5007), false, "Connectivity", 11, null, null, "Bluetooth" },
                    { 48, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5008), false, "App Compatibility", 11, null, null, "iOS and Android" },
                    { 49, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5009), false, "Axes", 12, null, null, "3-axis" },
                    { 50, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5010), false, "Battery Life", 12, null, null, "12 hours" },
                    { 51, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5012), false, "Compatibility", 12, null, null, "Most smartphones up to 6.7 inches" },
                    { 52, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5013), false, "Weight", 12, null, null, "400g" },
                    { 53, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5015), false, "Components", 13, null, null, "1 Hub, 2 Plugs, 2 Sensors, 3 Bulbs" },
                    { 54, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5016), false, "Connectivity", 13, null, null, "Wi-Fi, Bluetooth" },
                    { 55, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5017), false, "Voice Assistant Compatibility", 13, null, null, "Alexa, Google Assistant, Siri" },
                    { 56, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5018), false, "App Control", 13, null, null, "Yes" },
                    { 57, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5019), false, "Battery Life", 14, null, null, "24 Hours" },
                    { 58, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5020), false, "Features", 14, null, null, "Noise Cancellation" },
                    { 59, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5022), false, "Color", 14, null, null, "Black" },
                    { 60, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5023), false, "Material", 15, null, null, "Eco-Friendly TPE" },
                    { 61, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5024), false, "Thickness", 15, null, null, "6mm" },
                    { 62, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5025), false, "Dimensions", 15, null, null, "61 x 183 cm" },
                    { 63, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5026), false, "Material", 16, null, null, "100% Silk" },
                    { 64, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5027), false, "Width", 16, null, null, "6 cm" },
                    { 65, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5029), false, "Pattern", 16, null, null, "Striped" },
                    { 66, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5030), false, "Size", 17, null, null, "Medium" },
                    { 67, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5031), false, "Material", 17, null, null, "Polyurethane Foam, Gel" },
                    { 68, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5032), false, "Dimensions", 17, null, null, "80 x 60 x 15 cm" },
                    { 69, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5033), false, "Count", 18, null, null, "24 Pieces" },
                    { 70, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5034), false, "Flavors", 18, null, null, "Assorted" },
                    { 71, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5035), false, "Packaging", 18, null, null, "Gift Box" },
                    { 72, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5037), false, "Max Weight Capacity", 19, null, null, "120 kg" },
                    { 73, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5038), false, "Material", 19, null, null, "Mesh, Metal" },
                    { 74, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5039), false, "Adjustments", 19, null, null, "Height, Lumbar" },
                    { 75, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5040), false, "Color", 19, null, null, "Blue" },
                    { 76, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5042), false, "Age Range", 20, null, null, "6-10 Years" },
                    { 77, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5043), false, "Pages", 20, null, null, "150" },
                    { 78, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5044), false, "Format", 20, null, null, "Hardcover" },
                    { 79, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5045), false, "Frame Material", 21, null, null, "Aluminum" },
                    { 80, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5047), false, "Suspension", 21, null, null, "Front" },
                    { 81, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5048), false, "Wheel Size", 21, null, null, "26 inches" },
                    { 82, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5049), false, "Gears", 21, null, null, "21-Speed" },
                    { 83, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5050), false, "Weight", 22, null, null, "5 kg" },
                    { 84, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5051), false, "Ingredients", 22, null, null, "Organic Chicken, Vegetables" },
                    { 85, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5053), false, "Type", 22, null, null, "Dry Food" },
                    { 86, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5054), false, "Volume", 23, null, null, "500 ml each" },
                    { 87, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5055), false, "Ingredients", 23, null, null, "Aloe Vera, Coconut Oil" },
                    { 88, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5056), false, "Type", 23, null, null, "Shampoo & Conditioner" }
                });

            migrationBuilder.InsertData(
                table: "ProductTag",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "IsDeleted", "ProductId", "TagName", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5170), false, 1, "4K", null, null },
                    { 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5172), false, 1, "Smart TV", null, null },
                    { 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5173), false, 1, "HDR", null, null },
                    { 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5271), false, 1, "Home Entertainment", null, null },
                    { 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5272), false, 2, "Men's Fashion", null, null },
                    { 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5273), false, 2, "Casual Wear", null, null },
                    { 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5275), false, 2, "Cotton", null, null },
                    { 8, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5276), false, 3, "Gardening", null, null },
                    { 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5277), false, 3, "Tools", null, null },
                    { 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5278), false, 3, "New Arrival", null, null },
                    { 11, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5279), false, 4, "Computing", null, null },
                    { 12, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5280), false, 4, "SSD", null, null },
                    { 13, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5282), false, 4, "Lightweight", null, null },
                    { 14, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5283), false, 4, "New Arrival", null, null },
                    { 15, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5284), false, 5, "Women's Fashion", null, null },
                    { 16, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5285), false, 5, "Leather", null, null },
                    { 17, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5286), false, 5, "Designer", null, null },
                    { 18, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5287), false, 5, "Trending", null, null },
                    { 19, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5288), false, 6, "Photography", null, null },
                    { 20, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5289), false, 6, "4K Video", null, null },
                    { 21, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5290), false, 6, "Featured", null, null },
                    { 22, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5291), false, 7, "Organic", null, null },
                    { 23, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5292), false, 7, "Gift Set", null, null },
                    { 24, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5293), false, 7, "New Arrival", null, null },
                    { 25, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5294), false, 8, "Historical Fiction", null, null },
                    { 26, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5296), false, 8, "Bestseller", null, null },
                    { 27, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5297), false, 8, "New Arrival", null, null },
                    { 28, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5298), false, 9, "Outdoor", null, null },
                    { 29, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5299), false, 9, "Waterproof", null, null },
                    { 30, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5300), false, 9, "Unisex", null, null },
                    { 31, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5301), false, 9, "Trending", null, null },
                    { 32, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5302), false, 10, "Anti-Aging", null, null },
                    { 33, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5303), false, 10, "Beauty", null, null },
                    { 34, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5304), false, 10, "New Arrival", null, null },
                    { 35, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5305), false, 10, "Trending", null, null },
                    { 36, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5306), false, 11, "Educational", null, null },
                    { 37, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5307), false, 11, "STEM", null, null },
                    { 38, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5308), false, 11, "Kids", null, null },
                    { 39, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5309), false, 11, "New Arrival", null, null },
                    { 40, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5310), false, 12, "Photography", null, null },
                    { 41, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5312), false, 12, "Accessories", null, null },
                    { 42, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5314), false, 12, "New Arrival", null, null },
                    { 43, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5315), false, 13, "Smart Home", null, null },
                    { 44, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5316), false, 13, "IoT", null, null },
                    { 45, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5317), false, 13, "New Arrival", null, null },
                    { 46, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5318), false, 13, "Trending", null, null },
                    { 47, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5319), false, 14, "Audio", null, null },
                    { 48, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5320), false, 14, "Wireless", null, null },
                    { 49, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5321), false, 14, "Noise Cancelling", null, null },
                    { 50, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5322), false, 15, "Fitness", null, null },
                    { 51, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5323), false, 15, "Yoga", null, null },
                    { 52, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5324), false, 15, "Eco-Friendly", null, null },
                    { 53, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5325), false, 16, "Men's Accessories", null, null },
                    { 54, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5326), false, 16, "Silk", null, null },
                    { 55, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5327), false, 16, "Formal Wear", null, null },
                    { 56, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5328), false, 17, "Pet Supplies", null, null },
                    { 57, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5329), false, 17, "Comfort", null, null },
                    { 58, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5330), false, 17, "Durable", null, null },
                    { 59, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5331), false, 18, "Gourmet", null, null },
                    { 60, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5332), false, 18, "Sweets", null, null },
                    { 61, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5333), false, 18, "Gift", null, null },
                    { 62, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5335), false, 19, "Office Furniture", null, null },
                    { 63, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5336), false, 19, "Ergonomic", null, null },
                    { 64, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5337), false, 19, "Home Office", null, null },
                    { 65, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5338), false, 20, "Children's Books", null, null },
                    { 66, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5339), false, 20, "Fiction", null, null },
                    { 67, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5340), false, 20, "Educational", null, null },
                    { 68, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5341), false, 21, "Cycling", null, null },
                    { 69, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5342), false, 21, "Outdoor", null, null },
                    { 70, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5343), false, 21, "Sports", null, null },
                    { 71, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5345), false, 22, "Pet Food", null, null },
                    { 72, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5346), false, 22, "Organic", null, null },
                    { 73, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5347), false, 22, "Dog Supplies", null, null },
                    { 74, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5348), false, 23, "Hair Care", null, null },
                    { 75, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5349), false, 23, "Beauty", null, null },
                    { 76, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5350), false, 23, "Natural Ingredients", null, null },
                    { 77, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5351), false, 23, "Shampoo", null, null }
                });

            migrationBuilder.InsertData(
                table: "ProductVariant",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "DiscountPrice", "IsDeleted", "Price", "ProductId", "SKU", "StockQuantity", "UpdatedBy", "UpdatedDate", "VariantName" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5528), 16.989999999999998, false, 20.0, 2, "APP-MTSRT-002-NVY-S", 25, null, null, "Size - S" },
                    { 2, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5535), 16.989999999999998, false, 20.0, 2, "APP-MTSRT-002-NVY-M", 30, null, null, "Size - M" },
                    { 3, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5537), 16.989999999999998, false, 20.0, 2, "APP-MTSRT-002-NVY-L", 25, null, null, "Size - L" },
                    { 4, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5539), 18.989999999999998, false, 22.0, 2, "APP-MTSRT-002-NVY-XL", 20, null, null, "Size - XL" },
                    { 5, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5541), 129.99000000000001, false, 149.99000000000001, 5, "APP-HBAG-005-BLK", 15, null, null, "Color - Black" },
                    { 6, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5543), 129.99000000000001, false, 149.99000000000001, 5, "APP-HBAG-005-BRN", 10, null, null, "Color - Brown" },
                    { 7, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5545), 109.95, false, 129.94999999999999, 9, "SPT-HBOOT-009-07", 8, null, null, "Size - US 7 / EU 38" },
                    { 8, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5546), 109.95, false, 129.94999999999999, 9, "SPT-HBOOT-009-08", 10, null, null, "Size - US 8 / EU 39" },
                    { 9, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5548), 109.95, false, 129.94999999999999, 9, "SPT-HBOOT-009-09", 12, null, null, "Size - US 9 / EU 40" },
                    { 10, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5551), 109.95, false, 129.94999999999999, 9, "SPT-HBOOT-009-10", 10, null, null, "Size - US 10 / EU 41" },
                    { 11, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5553), 109.95, false, 129.94999999999999, 9, "SPT-HBOOT-009-11", 5, null, null, "Size - US 11 / EU 42" },
                    { 12, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5555), 649.99000000000001, false, 699.99000000000001, 1, "ELC-SMTV-001-55", 30, null, null, "Size - 55 inch" },
                    { 13, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5556), 849.99000000000001, false, 899.99000000000001, 1, "ELC-SMTV-001-65", 20, null, null, "Size - 65 inch" },
                    { 14, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5558), 899.99000000000001, false, 999.99000000000001, 4, "ELC-LPTOP-004-16-512", 20, null, null, "RAM - 16GB / Storage - 512GB" },
                    { 15, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5560), 1199.99, false, 1299.99, 4, "ELC-LPTOP-004-32-1TB", 15, null, null, "RAM - 32GB / Storage - 1TB" },
                    { 16, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5562), 75.989999999999995, false, 89.989999999999995, 10, "BPC-AAGE-010-NORM", 15, null, null, "For Normal Skin" },
                    { 17, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5563), 75.989999999999995, false, 89.989999999999995, 10, "BPC-AAGE-010-DRY", 10, null, null, "For Dry Skin" },
                    { 18, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5565), 79.989999999999995, false, 94.989999999999995, 10, "BPC-AAGE-010-SENS", 5, null, null, "For Sensitive Skin" },
                    { 19, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5567), 799.99000000000001, false, 899.99000000000001, 6, "ELC-CAM-006-BLK", 15, null, null, "Color - Black" },
                    { 20, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5569), 799.99000000000001, false, 899.99000000000001, 6, "ELC-CAM-006-SLV", 10, null, null, "Color - Silver" },
                    { 21, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5570), 999.99000000000001, false, 1099.99, 6, "ELC-CAM-006-135L", 8, null, null, "Kit with 18-135mm Lens" },
                    { 22, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5572), 45.0, false, 45.0, 7, "FBD-TEA-007-6VP", 20, null, null, "6 Variety Pack" },
                    { 23, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5574), 65.0, false, 75.0, 7, "FBD-TEA-007-12VP", 15, null, null, "12 Variety Pack" },
                    { 24, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5575), 34.990000000000002, false, 39.990000000000002, 8, "BOK-MYST-008-PB", 30, null, null, "Paperback Set" },
                    { 25, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5577), 49.990000000000002, false, 59.990000000000002, 8, "BOK-MYST-008-HC", 15, null, null, "Hardcover Set" },
                    { 26, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5579), 24.989999999999998, false, 29.989999999999998, 8, "BOK-MYST-008-EB", 100, null, null, "E-book Collection" },
                    { 27, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5580), 69.989999999999995, false, 79.989999999999995, 11, "TOY-ROBOT-011-BLU", 10, null, null, "Color - Blue" },
                    { 28, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5582), 69.989999999999995, false, 79.989999999999995, 11, "TOY-ROBOT-011-PNK", 8, null, null, "Color - Pink" },
                    { 29, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5584), 89.989999999999995, false, 99.989999999999995, 11, "TOY-ROBOT-011-DLX", 5, null, null, "Deluxe Edition" },
                    { 30, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5586), 69.989999999999995, false, 85.0, 12, "ELC-GIMB-012-STD", 25, null, null, "Standard Edition" },
                    { 31, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5607), 99.989999999999995, false, 120.0, 12, "ELC-GIMB-012-PRO", 15, null, null, "Pro Edition" },
                    { 32, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5609), 149.99000000000001, false, 179.99000000000001, 13, "ELC-SMHM-013-BAS", 15, null, null, "Basic Kit" },
                    { 33, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5611), 219.99000000000001, false, 249.99000000000001, 13, "ELC-SMHM-013-EXT", 10, null, null, "Extended Kit" },
                    { 34, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5612), 49.990000000000002, false, 59.990000000000002, 14, "ELC-EARB-014-BLK", 30, null, null, "Color - Black" },
                    { 35, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5614), 49.990000000000002, false, 59.990000000000002, 14, "ELC-EARB-014-WHT", 20, null, null, "Color - White" },
                    { 36, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5616), 49.990000000000002, false, 59.990000000000002, 14, "ELC-EARB-014-BLU", 10, null, null, "Color - Blue" },
                    { 37, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5617), 34.990000000000002, false, 39.990000000000002, 15, "SPO-YOGA-015-GRN", 20, null, null, "Color - Green" },
                    { 38, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5619), 34.990000000000002, false, 39.990000000000002, 15, "SPO-YOGA-015-BLU", 15, null, null, "Color - Blue" },
                    { 39, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5622), 39.990000000000002, false, 44.990000000000002, 15, "SPO-YOGA-015-6MM", 10, null, null, "Thickness - 6mm" },
                    { 40, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5623), 45.0, false, 45.0, 16, "APP-TIE-016-PSLY", 10, null, null, "Pattern - Paisley" },
                    { 41, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5625), 45.0, false, 45.0, 16, "APP-TIE-016-STR", 8, null, null, "Pattern - Striped" },
                    { 42, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5627), 45.0, false, 45.0, 16, "APP-TIE-016-NVY", 12, null, null, "Color - Navy Blue" },
                    { 43, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5696), 59.990000000000002, false, 69.989999999999995, 17, "PET-BED-017-SML", 8, null, null, "Size - Small" },
                    { 44, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5699), 79.989999999999995, false, 89.989999999999995, 17, "PET-BED-017-MED", 7, null, null, "Size - Medium" },
                    { 45, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5700), 99.989999999999995, false, 109.98999999999999, 17, "PET-BED-017-LRG", 5, null, null, "Size - Large" },
                    { 46, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5703), 24.989999999999998, false, 29.989999999999998, 18, "FBD-CHOC-018-24PC", 20, null, null, "24 Piece Box" },
                    { 47, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5704), 39.990000000000002, false, 49.990000000000002, 18, "FBD-CHOC-018-48PC", 10, null, null, "48 Piece Box" },
                    { 48, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5706), 179.99000000000001, false, 199.99000000000001, 19, "FURN-OFCH-019-BLK", 15, null, null, "Color - Black" },
                    { 49, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5708), 179.99000000000001, false, 199.99000000000001, 19, "FURN-OFCH-019-GRY", 10, null, null, "Color - Grey" },
                    { 50, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5709), 199.99000000000001, false, 229.99000000000001, 19, "FURN-OFCH-019-LUM", 8, null, null, "With Lumbar Support" },
                    { 51, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5711), 12.0, false, 15.0, 20, "BOK-KIDS-020-HC", 30, null, null, "Hardcover" },
                    { 52, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5713), 7.9900000000000002, false, 9.9900000000000002, 20, "BOK-KIDS-020-PB", 40, null, null, "Paperback" },
                    { 53, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5716), 449.0, false, 499.0, 21, "SPO-MTBIKE-021-M27", 5, null, null, "Size - Medium (27.5\" Wheels)" },
                    { 54, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5717), 499.0, false, 549.0, 21, "SPO-MTBIKE-021-L29", 4, null, null, "Size - Large (29\" Wheels)" },
                    { 55, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5719), 30.0, false, 35.0, 22, "PET-DGFD-022-CK5", 25, null, null, "Flavor - Chicken (5kg Bag)" },
                    { 56, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5721), 30.0, false, 35.0, 22, "PET-DGFD-022-BF5", 20, null, null, "Flavor - Beef (5kg Bag)" },
                    { 57, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5722), 50.0, false, 60.0, 22, "PET-DGFD-022-LB10", 15, null, null, "Flavor - Lamb (10kg Bag)" },
                    { 58, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5724), 12.99, false, 15.0, 23, "BPC-SHMP-023-NORM250", 30, null, null, "Type - Normal Hair (250ml)" },
                    { 59, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5726), 12.99, false, 15.0, 23, "BPC-SHMP-023-OILY250", 25, null, null, "Type - Oily Hair (250ml)" },
                    { 60, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(5727), 21.989999999999998, false, 25.0, 23, "BPC-SHMP-023-DRY500", 20, null, null, "Type - Dry Hair (500ml)" }
                });

            migrationBuilder.InsertData(
                table: "InvoiceAttachments",
                columns: new[] { "Id", "AttachmentContent", "AttachmentName", "CreatedBy", "CreatedDate", "InvoiceId", "IsDeleted", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "/files/invoices/INV-2025-001.pdf", "Invoice_INV-2025-001.pdf", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8528), 1, false, null, null },
                    { 2, "/files/invoices/INV-2025-002.pdf", "Invoice_INV-2025-002.pdf", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8530), 2, false, null, null },
                    { 3, "/files/invoices/INV-2025-003.pdf", "Invoice_INV-2025-003.pdf", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8532), 3, false, null, null },
                    { 4, "/files/invoices/INV-2025-004.pdf", "Invoice_INV-2025-004.pdf", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8533), 4, false, null, null },
                    { 5, "/files/invoices/INV-2025-005.pdf", "Invoice_INV-2025-005.pdf", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8534), 5, false, null, null },
                    { 6, "/files/invoices/INV-2025-006.pdf", "Invoice_INV-2025-006.pdf", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8536), 6, false, null, null },
                    { 7, "/files/invoices/INV-2025-007.pdf", "CreditNote_INV-2025-007.pdf", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8537), 7, false, null, null },
                    { 8, "/files/invoices/INV-2025-008.pdf", "Invoice_INV-2025-008.pdf", null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8538), 8, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "InvoiceItems",
                columns: new[] { "Id", "Amount", "CreatedBy", "CreatedDate", "Description", "InvoiceId", "IsDeleted", "Price", "Service", "Unit", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, 649.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8397), "55-inch 4K Smart TV with HDR", 1, false, 649.99m, "Smart TV 55 inch", "Unit", null, null },
                    { 2, 129.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8401), "Premium black leather handbag", 2, false, 129.99m, "Leather Handbag", "Unit", null, null },
                    { 3, 799.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8404), "24.1MP DSLR camera with 18-55mm lens", 3, false, 799.99m, "DSLR Camera", "Unit", null, null },
                    { 4, 109.95m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8452), "Waterproof hiking boots size US 9", 4, false, 109.95m, "Hiking Boots", "Unit", null, null },
                    { 5, 75.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8454), "Anti-aging skincare collection for normal skin", 5, false, 75.99m, "Skincare Set", "Unit", null, null },
                    { 6, 69.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8456), "Interactive learning robot for kids", 6, false, 69.99m, "Learning Robot", "Unit", null, null },
                    { 7, -15.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8459), "Credit for returned historical fiction book", 7, false, -15.99m, "Book Return", "Unit", null, null },
                    { 8, 149.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8461), "Smart home starter kit with hub and bulbs", 8, false, 149.99m, "Smart Home Kit", "Unit", null, null },
                    { 9, 49.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8463), "2-year extended warranty for Smart TV", 1, false, 49.99m, "Extended Warranty", "Unit", null, null },
                    { 10, 29.99m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8465), "Tripod accessory for DSLR camera", 3, false, 29.99m, "Camera Tripod", "Unit", null, null }
                });

            migrationBuilder.InsertData(
                table: "TaxDetails",
                columns: new[] { "Id", "Amount", "CreatedBy", "CreatedDate", "InvoiceId", "IsDeleted", "Rate", "TaxType", "UpdatedBy", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, 52.00m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8594), 1, false, 8.00m, "VAT", null, null },
                    { 2, 10.40m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8598), 2, false, 8.00m, "GST", null, null },
                    { 3, 64.00m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8599), 3, false, 8.00m, "Consumption Tax", null, null },
                    { 4, 8.80m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8601), 4, false, 8.00m, "GST", null, null },
                    { 5, 6.08m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8603), 5, false, 8.00m, "VAT", null, null },
                    { 6, 5.60m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8604), 6, false, 8.00m, "GST", null, null },
                    { 7, -1.28m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8606), 7, false, 8.00m, "VAT", null, null },
                    { 8, 12.00m, null, new DateTime(2025, 6, 21, 6, 52, 50, 775, DateTimeKind.Utc).AddTicks(8608), 8, false, 8.00m, "VAT", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Action_Timestamp",
                table: "ActivityLogs",
                columns: new[] { "Action", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TargetUserId_Timestamp",
                table: "ActivityLogs",
                columns: new[] { "TargetUserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsDeleted_DeletedDate",
                table: "AspNetUsers",
                columns: new[] { "IsDeleted", "DeletedDate" },
                filter: "IsDeleted = 1");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuthStates_ExpiresAt",
                table: "AuthStates",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuthStates_UserId",
                table: "AuthStates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthTokens_Token",
                table: "AuthTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthTokens_UserId",
                table: "AuthTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyId",
                table: "Customers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceAttachments_InvoiceId",
                table: "InvoiceAttachments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CompanyId",
                table: "Invoices",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CompanyId1",
                table: "Invoices",
                column: "CompanyId1");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_LocationId",
                table: "Invoices",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderId",
                table: "Invoices",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CompanyId",
                table: "Locations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CurrencyId",
                table: "Locations",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_TimezoneId",
                table: "Locations",
                column: "TimezoneId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderActivityLogs_OrderHeaderId_Timestamp",
                table: "OrderActivityLogs",
                columns: new[] { "OrderHeaderId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderHeaderId",
                table: "OrderDetails",
                column: "OrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_ApplicationUserId",
                table: "OrderHeaders",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_CustomerId",
                table: "OrderHeaders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubCategoryId",
                table: "Products",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_VendorId",
                table: "Products",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecification_ProductId",
                table: "ProductSpecification",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTag_ProductId",
                table: "ProductTag",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_ProductId",
                table: "ProductVariant",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ApplicationUserId",
                table: "RefreshTokens",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuPermissions_PermissionId",
                table: "RoleMenuPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuPermissions_RoleId_MenuName_PermissionId",
                table: "RoleMenuPermissions",
                columns: new[] { "RoleId", "MenuName", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_ApplicationUserId",
                table: "ShoppingCarts",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_ProductId",
                table: "ShoppingCarts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategory_CategoryId",
                table: "SubCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxDetails_InvoiceId",
                table: "TaxDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Timezones_Name",
                table: "Timezones",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_CompanyId",
                table: "Vendors",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_ProductId",
                table: "WishlistItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId",
                table: "WishlistItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "AdminActivityLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuthStates");

            migrationBuilder.DropTable(
                name: "AuthTokens");

            migrationBuilder.DropTable(
                name: "ContactUsSubmissions");

            migrationBuilder.DropTable(
                name: "ExportLogs");

            migrationBuilder.DropTable(
                name: "ImpersonationLogs");

            migrationBuilder.DropTable(
                name: "InvoiceAttachments");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "OrderActivityLogs");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductSpecification");

            migrationBuilder.DropTable(
                name: "ProductTag");

            migrationBuilder.DropTable(
                name: "ProductVariant");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RoleMenuPermissions");

            migrationBuilder.DropTable(
                name: "ShoppingCarts");

            migrationBuilder.DropTable(
                name: "TaxDetails");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "OrderHeaders");

            migrationBuilder.DropTable(
                name: "Brand");

            migrationBuilder.DropTable(
                name: "SubCategory");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Timezones");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
