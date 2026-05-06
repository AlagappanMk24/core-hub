using AspNet.Security.OAuth.GitHub;
using Core_API.Application.CrossCuttingConcerns.Authorization.Handlers;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Configuration.Settings;
using Core_API.Infrastructure.Persistence.Context;
using Core_API.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Core_API.Web.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring authentication and authorization services.
    /// </summary>
    /// <remarks>
    /// This class centralizes all authentication-related configuration including:
    /// JWT bearer authentication, Identity setup, external login providers, and authorization policies.
    /// </remarks>
    public static class AuthenticationExtensions
    {
        #region Identity and JWT Authentication

        /// <summary>
        /// Configures Identity framework and JWT bearer authentication for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration containing JWT and Identity settings.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>
        /// This method performs the following configurations:
        /// <list type="number">
        /// <item><description>Registers JWT settings from configuration</description></item>
        /// <item><description>Ensures a valid JWT secret key exists</description></item>
        /// <item><description>Configures ASP.NET Core Identity with ApplicationUser</description></item>
        /// <item><description>Sets up JWT bearer authentication with validation parameters</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when JWT settings are missing from configuration.
        /// </exception>
        public static IServiceCollection AddIdentityAndAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            #region JWT Configuration

            // Configure JWT settings from appsettings.json
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            // Generate and ensure secret key exists
            SecretKeyGenerator.EnsureSecretKeyExists(configuration, jwtSettings);

            #endregion

            #region Identity Configuration

            // Configure ASP.NET Core Identity with ApplicationUser
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Account configuration
                options.SignIn.RequireConfirmedAccount = false;

                // Password policy
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<CoreInvoiceDbContext>()
            .AddDefaultTokenProviders();

            #endregion

            #region JWT Authentication Configuration

            // Configure JWT Authentication
            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = jwtSettings.ValidAudience,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                options.MapInboundClaims = true;
            });

            #endregion

            return services;
        }

        #endregion

        #region Authorization Policies and Handlers

        /// <summary>
        /// Registers authorization handlers and configures role-based authorization policies.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>
        /// This method registers:
        /// <list type="bullet">
        /// <item><description><see cref="PermissionAuthorizationHandler"/> for permission-based authorization</description></item>
        /// <item><description><see cref="ImpersonationAuthorizationHandler"/> for user impersonation scenarios</description></item>
        /// <item><description>Role-based policies for Admin, User, and Customer roles</description></item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddAuthorizationAndHandlers(this IServiceCollection services)
        {
            #region Authorization Handlers

            // Register authorization handlers
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, ImpersonationAuthorizationHandler>();

            #endregion

            #region Authorization Policies

            // Configure role-based authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("User", policy => policy.RequireRole("User"));
                options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
            });

            #endregion

            return services;
        }
        #endregion

        #region External Login Providers

        /// <summary>
        /// Configures external OAuth authentication providers for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration containing external provider credentials.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>
        /// This method configures the following external providers:
        /// <list type="bullet">
        /// <item><description>Google OAuth 2.0</description></item>
        /// <item><description>GitHub OAuth 2.0</description></item>
        /// <item><description>Microsoft Account OAuth 2.0</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when required client IDs or secrets are missing from configuration.
        /// </exception>
        public static IServiceCollection AddExternalLogins(
          this IServiceCollection services,
          IConfiguration configuration)
        {
            services.AddAuthentication()
            #region Google Authentication
                .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = configuration["GoogleKeys:ClientId"]
                    ?? throw new ArgumentNullException("GoogleKeys:ClientId is missing");
                    options.ClientSecret = configuration["GoogleKeys:ClientSecret"]
                        ?? throw new ArgumentNullException("GoogleKeys:ClientSecret is missing");
                })
            #endregion
            #region GitHub Authentication
                .AddGitHub(GitHubAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = configuration["GitHubKeys:ClientId"]
                   ?? throw new ArgumentNullException("GitHubKeys:ClientId is missing");
                    options.ClientSecret = configuration["GitHubKeys:ClientSecret"]
                        ?? throw new ArgumentNullException("GitHubKeys:ClientSecret is missing");
                    options.CallbackPath = new PathString("/signin-github");
                })
            #endregion
            #region Microsoft Account Authentication
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = configuration["MicrosoftKeys:ClientId"]
                      ?? throw new ArgumentNullException("MicrosoftKeys:ClientId is missing");
                    options.ClientSecret = configuration["MicrosoftKeys:ClientSecret"]
                        ?? throw new ArgumentNullException("MicrosoftKeys:ClientSecret is missing");
                });
            #endregion
            return services;
        }
        #endregion
    }
}