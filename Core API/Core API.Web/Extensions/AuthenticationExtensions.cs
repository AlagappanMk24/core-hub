using AspNet.Security.OAuth.GitHub;
using Core_API.Application.CrossCuttingConcerns.Authorization.Handlers;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Configuration.Settings;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

namespace Core_API.Web.Extensions;

/// <summary>
/// Provides extension methods for configuring authentication and authorization services.
/// Implements industry-standard security patterns including JWT bearer authentication,
/// Identity framework, external OAuth providers, and comprehensive authorization policies.
/// </summary>
public static class AuthenticationExtensions
{
    #region Public Methods

    /// <summary>
    /// Configures Identity framework and JWT bearer authentication with production-ready settings.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when JWT settings are invalid.</exception>
    public static IServiceCollection AddIdentityAndAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure JWT settings with validation
        var jwtSettings = ConfigureJwtSettings(services, configuration);

        // Configure Identity with security best practices
        ConfigureIdentity(services);

        // Configure JWT bearer authentication
        ConfigureJwtAuthentication(services, jwtSettings);

        return services;
    }

    /// <summary>
    /// Configures authorization handlers and policies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection for method chaining.</returns>
    public static IServiceCollection AddAuthorizationAndHandlers(
        this IServiceCollection services)
    {
        // Register custom authorization handlers
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ImpersonationAuthorizationHandler>();

        // Configure comprehensive authorization policies
        services.AddAuthorization(options =>
        {
            // Role-based policies
            options.AddPolicy("Admin", policy =>
                policy.RequireRole("Admin")
                      .RequireAuthenticatedUser());

            options.AddPolicy("User", policy =>
                policy.RequireRole("User", "Admin")
                      .RequireAuthenticatedUser());

            options.AddPolicy("Customer", policy =>
                policy.RequireRole("Customer")
                      .RequireAuthenticatedUser());

            // Policy with requirements
            options.AddPolicy("AdminOrOwner", policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.FindFirst("uid")?.Value ==
                    context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value));

            // Policy with custom requirements (to be implemented)
            // options.AddPolicy("RequirePermission", policy =>
            //     policy.Requirements.Add(new PermissionRequirement("some.permission")));
        });

        return services;
    }

    /// <summary>
    /// Configures external OAuth authentication providers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for method chaining.</returns>
    public static IServiceCollection AddExternalLogins(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authenticationBuilder = services.AddAuthentication();

        // Configure Google OAuth
        ConfigureGoogleAuth(authenticationBuilder, configuration);

        // Configure GitHub OAuth
        ConfigureGitHubAuth(authenticationBuilder, configuration);

        // Configure Microsoft OAuth
        ConfigureMicrosoftAuth(authenticationBuilder, configuration);

        return services;
    }

    /// <summary>
    /// Adds Swagger security definition for JWT authentication.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection for method chaining.</returns>
    public static IServiceCollection AddSwaggerSecurity(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' followed by a space and your JWT token.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9'"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    #endregion

    #region Private Configuration Methods

    /// <summary>
    /// Configures JWT settings with validation and secret key generation.
    /// </summary>
    private static JwtSettings ConfigureJwtSettings(
        IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind and validate JWT settings
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing from configuration");

        // Validate settings
        ValidateJwtSettings(jwtSettings);

        // Ensure secret key exists (generate if missing)
        SecretKeyGenerator.EnsureSecretKeyExists(configuration, jwtSettings);

        // Register settings for DI
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        return jwtSettings;
    }

    /// <summary>
    /// Validates JWT settings for security best practices.
    /// </summary>
    private static void ValidateJwtSettings(JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is required");

        if (settings.SecretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters");

        if (string.IsNullOrWhiteSpace(settings.ValidIssuer))
            throw new InvalidOperationException("JWT ValidIssuer is required");

        if (string.IsNullOrWhiteSpace(settings.ValidAudience))
            throw new InvalidOperationException("JWT ValidAudience is required");

        if (settings.ExpireHours is < 1 or > 72)
            throw new InvalidOperationException("JWT ExpireHours must be between 1 and 72");

        if (settings.OtpExpireMinutes is < 1 or > 30)
            throw new InvalidOperationException("JWT OtpExpireMinutes must be between 1 and 30");
    }

    /// <summary>
    /// Configures ASP.NET Core Identity with security best practices.
    /// </summary>
    private static void ConfigureIdentity(IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            // Sign-in settings
            options.SignIn.RequireConfirmedAccount = false;
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;

            // Password settings (balance security with user experience)
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings (protect against brute force)
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            // Token settings
            options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<CoreInvoiceDbContext>()
        .AddDefaultTokenProviders()
        .AddUserManager<UserManager<ApplicationUser>>()
        .AddSignInManager<SignInManager<ApplicationUser>>();
    }

    /// <summary>
    /// Configures JWT bearer authentication with comprehensive validation.
    /// </summary>
    private static void ConfigureJwtAuthentication(
        IServiceCollection services,
        JwtSettings jwtSettings)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Basic configuration
            options.SaveToken = true;
            options.RequireHttpsMetadata = !IsDevelopment(); // Force HTTPS in production

            // Token validation parameters
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.ValidIssuer,
                ValidAudience = jwtSettings.ValidAudience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes),
                RequireExpirationTime = true,
                RequireSignedTokens = true
            };

            // Event handlers for better error handling and logging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    if (context.Exception is SecurityTokenExpiredException expiredEx)
                    {
                        logger.LogWarning("Token expired: {ExpiredAt}", expiredEx.Expires);
                        context.Response.Headers.Add("Token-Expired", "true");
                        context.Response.Headers.Add("Token-Expired-At",
                            expiredEx.Expires.ToString("o"));
                    }
                    else
                    {
                        logger.LogError(context.Exception, "Authentication failed");
                    }

                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    logger.LogWarning("Authentication challenge: {Error}, {ErrorDescription}",
                        context.Error, context.ErrorDescription);

                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    logger.LogDebug("Token validated for user: {UserId}", userId);

                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    // Support token from query string for SignalR
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/notificationHub"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });
    }

    /// <summary>
    /// Configures Google OAuth authentication.
    /// </summary>
    private static void ConfigureGoogleAuth(
        AuthenticationBuilder builder,
        IConfiguration configuration)
    {
        var clientId = configuration["GoogleKeys:ClientId"];
        var clientSecret = configuration["GoogleKeys:ClientSecret"];

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            builder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.SaveTokens = true;
                options.Scope.Add("profile");
                options.Scope.Add("email");
            });
        }
    }

    /// <summary>
    /// Configures GitHub OAuth authentication.
    /// </summary>
    private static void ConfigureGitHubAuth(
        AuthenticationBuilder builder,
        IConfiguration configuration)
    {
        var clientId = configuration["GitHubKeys:ClientId"];
        var clientSecret = configuration["GitHubKeys:ClientSecret"];

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            builder.AddGitHub(GitHubAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = new PathString("/signin-github");
                options.SaveTokens = true;
                options.Scope.Add("user:email");
            });
        }
    }

    /// <summary>
    /// Configures Microsoft Account OAuth authentication.
    /// </summary>
    private static void ConfigureMicrosoftAuth(
        AuthenticationBuilder builder,
        IConfiguration configuration)
    {
        var clientId = configuration["MicrosoftKeys:ClientId"];
        var clientSecret = configuration["MicrosoftKeys:ClientSecret"];

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            builder.AddMicrosoftAccount(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.SaveTokens = true;
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Scope.Add("openid");
            });
        }
    }

    /// <summary>
    /// Determines if the environment is development.
    /// </summary>
    private static bool IsDevelopment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }

    #endregion
}