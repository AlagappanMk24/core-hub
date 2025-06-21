using Core_API.Infrastructure.Data.Context;
using Core_API.Web.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Core_API.Web.Utilities;
using Newtonsoft.Json;
using Core_API.Application.DI;
using Core_API.Infrastructure.DI;
using Microsoft.AspNetCore.Authentication.Google;
using Core_API.Web.Logger;
using AspNet.Security.OAuth.GitHub;
using Core_API.Infrastructure.Shared;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Services.Background;
using Core_API.Infrastructure.Data.Initializers;
using Core_API.Application.Authorization.Handlers;
using Core_API.Application.Authorization.Requirements;
using Core_API.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Stripe;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Core_API.Infrastructure.RateLimiting;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Configure services for dependency injection
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
ConfigureMiddleware(app);

// Start the application
app.Run();

/// <summary>
/// Configures services for the application, including database, authentication, and rate limiting.
/// </summary>
/// <param name="services">The service collection to configure.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Initialize rate limiting configuration from appsettings.json
    RateLimitConfig.Initialize(configuration);

    // Add background service for user cleanup
    services.AddHostedService<UserCleanupService>();

    // Configure database context
    ConfigureDatabase(services, configuration);

    // Configure JWT settings
    ConfigureJwtSettings(services, configuration);

    // Configure authentication services
    ConfigureAuthentication(services, configuration);

    // Configure authorization policies
    ConfigureAuthorization(services);

    // Generate JWT secret key if not present
    GenerateSecretKey(configuration);

    // Configure admin settings
    ConfigureAdminSettings(services, configuration);

    // Configure email settings
    ConfigureEmailSettings(services, configuration);

    // Configure SMS settings
    ConfigureSmsSettings(services, configuration);

    // Configure Stripe settings
    ConfigureStripeSettings(services, configuration);

    // Configure Stripe API
    ConfigureStripe(configuration);

    // Configure ASP.NET Core Identity
    ConfigureIdentity(services);

    // Configure CORS policy
    ConfigureCORS(services);

    // Configure Swagger for API documentation
    ConfigureSwagger(services);

    // Configure external login providers (Google, GitHub, Microsoft)
    ConfigureExternalLogins(services, configuration);

    // Configure rate limiting policies
    ConfigureRateLimiting(services, configuration);

    // Add controllers with custom model validation filter
    services.AddControllers(options =>
    {
        options.Filters.Add<ValidateModelFilter>();
    });

    // Add support for views
    services.AddControllersWithViews();

    // Add HTTP client for external API calls
    services.AddHttpClient();

    // Add HTTP context accessor
    services.AddHttpContextAccessor();

    // Register application and infrastructure services
    RegisterApplicationServices(services);
}

/// <summary>
/// Configures rate limiting policies for sensitive endpoints.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureRateLimiting(IServiceCollection services, IConfiguration configuration)
{
    services.AddRateLimiter(options =>
    {
        // Define custom response for rate limit exceeded
        options.OnRejected = async (context, token) =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            context.HttpContext.Response.StatusCode = 429;

            // Determine the policy name from endpoint metadata
            string policyName = "Unknown";
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>() is { } rateLimitAttribute)
            {
                policyName = rateLimitAttribute.PolicyName;
            }

            // Retrieve policy metadata or use default
            var policyMetadata = RateLimitConfig.Policies.TryGetValue(policyName, out var metadata)
                ? metadata
                : new RateLimitConfig.RateLimitPolicyMetadata("Too many requests. Please try again later.", 60);

            var retryAfterSeconds = policyMetadata.RetryAfterSeconds;
            context.HttpContext.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());

            // Log rate limit violation
            logger.LogWarning("Rate limit exceeded for policy {PolicyName} on endpoint {Endpoint}. Message: {Message}, Retry-After: {RetryAfterSeconds}s",
                policyName, endpoint?.DisplayName, policyMetadata.Message, retryAfterSeconds);

            // Return JSON response with error details
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                success = false,
                statusCode = 429,
                message = policyMetadata.Message,
                retryAfterSeconds
            }, token);
        };

        // Login policy: 5 requests per minute
        options.AddFixedWindowLimiter("LoginPolicy", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        // OTP validation policy: 10 requests per minute
        options.AddFixedWindowLimiter("OtpPolicy", opt =>
        {
            opt.PermitLimit = 10;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        // OTP resend policy: 3 requests per 5 minutes
        options.AddFixedWindowLimiter("ResendOtpPolicy", opt =>
        {
            opt.PermitLimit = 3;
            opt.Window = TimeSpan.FromMinutes(5);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        options.RejectionStatusCode = 429;
    });
}

/// <summary>
/// Configures the database context with SQL Server.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("CoreAPIDbConnection");

    // Add DbContext with SQL Server
    services.AddDbContext<CoreAPIDbContext>(options =>
        options.UseSqlServer(connectionString));
}

/// <summary>
/// Configures ASP.NET Core Identity for user management.
/// </summary>
/// <param name="services">The service collection.</param>
void ConfigureIdentity(IServiceCollection services)
{
    services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<CoreAPIDbContext>()
            .AddDefaultTokenProviders();
}

/// <summary>
/// Configures JWT settings from configuration.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureJwtSettings(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
}

/// <summary>
/// Generates a JWT secret key if not present in configuration.
/// </summary>
/// <param name="configuration">The application configuration.</param>
void GenerateSecretKey(IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

    // Check if SecretKey is null or empty and generate a new one if necessary
    if (string.IsNullOrEmpty(jwtSettings.SecretKey))
    {
        var secretKey = SecretKeyGenerator.GenerateSecretKey();
        jwtSettings ??= new JwtSettings();
        jwtSettings.SecretKey = secretKey;

        // Update appsettings.json with the new secret key
        var appSettingsFile = "appsettings.json";
        var json = System.IO.File.ReadAllText(appSettingsFile);
        dynamic jsonObj = JsonConvert.DeserializeObject(json);

        jsonObj["JwtSettings"]["SecretKey"] = secretKey;
        string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
        System.IO.File.WriteAllText(appSettingsFile, output);
    }
}

/// <summary>
/// Configures JWT authentication.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

    var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

    services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(opt =>
    {
        opt.SaveToken = true;
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = jwtSettings.ValidAudience,
            ValidIssuer = jwtSettings.ValidIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
}

/// <summary>
/// Configures authorization policies for role-based access control.
/// </summary>
/// <param name="services">The service collection.</param>
void ConfigureAuthorization(IServiceCollection services)
{
    // Register the permission handler
    services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

    // Register the impersonation handler
    services.AddScoped<IAuthorizationHandler, ImpersonationAuthorizationHandler>();

    // Configure authorization policies
    services.AddAuthorization(options =>
    {
        // Register policies for all defined permissions
        foreach (var permissionField in typeof(AuthorizationConstants.Permissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            if (permissionField.FieldType == typeof(string))
            {
                var permission = (string)permissionField.GetValue(null);
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        }
        options.AddPolicy("ManageRoles", policy =>
           policy.RequireRole("Admin").AddRequirements(new PermissionRequirement("Roles.Manage")));

        // Add policy for impersonation
        options.AddPolicy("ImpersonationAccess", policy =>
            policy.Requirements.Add(new ImpersonationAuthorizationRequirement()));

        // Grouped policies for common use cases
        options.AddPolicy("ProductAccess", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.IsInRole(AppConstants.Role_Vendor) ||
                context.User.IsInRole(AppConstants.Role_Supplier) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == AuthorizationConstants.Permissions.Product_View ||
                     c.Value == AuthorizationConstants.Permissions.Product_Manage))));

        options.AddPolicy("ProductManagement", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == AuthorizationConstants.Permissions.Product_Manage)));

        options.AddPolicy("OrderAccess", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.IsInRole(AppConstants.Role_CustomerSupport) ||
                context.User.IsInRole(AppConstants.Role_DeliveryAgent) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == AuthorizationConstants.Permissions.Order_View ||
                     c.Value == AuthorizationConstants.Permissions.Order_Manage))));

        options.AddPolicy("OrderManagement", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == AuthorizationConstants.Permissions.Order_Manage)));

        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole(AppConstants.Role_Admin, AppConstants.Role_Admin_Super));

        options.AddPolicy("UserManagement", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == AuthorizationConstants.Permissions.User_Manage)));

        // Additional grouped policies for other entities
        options.AddPolicy("CategoryManagement", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == AuthorizationConstants.Permissions.Category_Manage)));

        options.AddPolicy("InvoiceManagement", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == AuthorizationConstants.Permissions.Invoice_Manage)));

        options.AddPolicy("CustomerManagement", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.IsInRole(AppConstants.Role_CustomerSupport) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == AuthorizationConstants.Permissions.Customer_Manage)));

        options.AddPolicy("CompanyManagement", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == AuthorizationConstants.Permissions.Company_Manage)));

        options.AddPolicy("BrandManagement", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole(AppConstants.Role_Admin) ||
                context.User.IsInRole(AppConstants.Role_Admin_Super) ||
                context.User.IsInRole(AppConstants.Role_Manager) ||
                context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == AuthorizationConstants.Permissions.Brand_Manage)));
    });
}

/// <summary>
/// Configures admin settings from configuration.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureAdminSettings(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<AdminSettings>(configuration.GetSection("AdminSettings"));
}

/// <summary>
/// Configures email settings from configuration.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
}

/// <summary>
/// Configures SMS settings from configuration.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureSmsSettings(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<SMSSettings>(configuration.GetSection("SMSSettings"));
}

/// <summary>
/// Configures Stripe settings from configuration.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureStripeSettings(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<StripeSettings>(configuration.GetSection("Stripe"));
}

/// <summary>
/// Configures external login providers (Google, GitHub, Microsoft).
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="configuration">The application configuration.</param>
void ConfigureExternalLogins(IServiceCollection services, IConfiguration configuration)
{
    services.AddAuthentication()
    .AddGoogle(GoogleDefaults.AuthenticationScheme, googleOptions =>
    {
        googleOptions.ClientId = configuration.GetSection("GoogleKeys:ClientId").Value;
        googleOptions.ClientSecret = configuration.GetSection("GoogleKeys:ClientSecret").Value;
    })
    .AddGitHub(GitHubAuthenticationDefaults.AuthenticationScheme, githubOptions =>
     {
         githubOptions.ClientId = configuration.GetSection("GitHubKeys:ClientId").Value;
         githubOptions.ClientSecret = configuration.GetSection("GitHubKeys:ClientSecret").Value;
         githubOptions.CallbackPath = new PathString("/signin-github"); // Must match GitHub settings
     })
    .AddMicrosoftAccount(microsoftOptions =>
    {
        microsoftOptions.ClientId = configuration.GetSection("MicrosoftKeys:ClientId").Value;
        microsoftOptions.ClientSecret = configuration.GetSection("MicrosoftKeys:ClientSecret").Value;
    });
      //.AddFacebook(fbOptions =>
    //{
    //    fbOptions.AppId = configuration.GetSection("FacebookKeys:AppId").Value;
    //    fbOptions.AppSecret = configuration.GetSection("FacebookKeys:AppSecret").Value;
    //})
}

/// <summary>
/// Configures CORS policy to allow frontend requests.
/// </summary>
/// <param name="services">The service collection.</param>
void ConfigureCORS(IServiceCollection services)
{
    // Add CORS policy to allow requests from the specified origin
    services.AddCors(options =>
    {
        options.AddPolicy("AllowMyOrigin",
            builder => builder.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials());
    });
}

/// <summary>
/// Configures Stripe API key.
/// </summary>
/// <param name="configuration">The application configuration.</param>
void ConfigureStripe(IConfiguration configuration)
{
    StripeConfiguration.ApiKey = configuration.GetSection("Stripe:SecretKey").Get<string>();
}

/// <summary>
/// Configures Swagger for API documentation with JWT support.
/// </summary>
/// <param name="services">The service collection.</param>
void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization Example : 'Bearer eyeleuieeesfjfjdue",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
             {
                 new OpenApiSecurityScheme
                 {
                      Reference= new OpenApiReference
                      {
                          Type=ReferenceType.SecurityScheme,
                          Id="Bearer",

                      },
                      Name="Bearer",
                      In=ParameterLocation.Header
                 },
                 new List<string>()
             }

        });
    });
}

/// <summary>
/// Registers application and infrastructure services.
/// </summary>
/// <param name="services">The service collection.</param>
void RegisterApplicationServices(IServiceCollection services)
{
    services.AddApplicationDependencies()
            .AddInfrastructureDependencies();
}

/// <summary>
/// Configures the HTTP request pipeline with middleware.
/// </summary>
/// <param name="app">The web application.</param>
void ConfigureMiddleware(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // Enable Swagger in development
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Redirect HTTP to HTTPS
    app.UseHttpsRedirection();

    // Apply CORS policy
    app.UseCors("AllowMyOrigin");

    // Serve static files
    app.UseStaticFiles();

    // Apply rate limiting
    app.UseRateLimiter();

    // Enable authentication
    app.UseAuthentication();

    // Enable authorization
    app.UseAuthorization();

    // Map controller endpoints
    app.MapControllers();

    // Seed database with initial data
    SeedDatabase();

    // Configure Logging
    ConfigureCustomLogging(app);
}

/// <summary>
/// Seeds the database with initial data.
/// </summary>
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            // Create a cancellation token with a timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            dbInitializer.Initialize(cts.Token).GetAwaiter().GetResult();
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Database seeding was canceled: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding: {Message}", ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Configures custom file-based logging.
/// </summary>
/// <param name="app">The web application.</param>
void ConfigureCustomLogging(WebApplication app)
{
    string formattedDate = DateTime.Now.ToString("MM-dd-yyyy");
    string baseLogPath = builder.Configuration.GetValue<string>("Logging:LogFilePath");
    string logFilePath = Path.Combine(baseLogPath, $"log-{formattedDate}.txt");

    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    loggerFactory.AddProvider(new CustomFileLoggerProvider(logFilePath, httpContextAccessor));
}