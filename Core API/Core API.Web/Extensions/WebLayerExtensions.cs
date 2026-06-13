using Core_API.Infrastructure.Configuration.Settings;
using Core_API.Web.Filters;
using Stripe;

namespace Core_API.Web.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring web layer services including controllers, CORS, and third-party integrations.
    /// </summary>
    /// <remarks>
    /// This class centralizes configuration for:
    /// - MVC controllers and filters
    /// - HTTP client and context accessors
    /// - SignalR real-time communication
    /// - CORS policies
    /// - Stripe payment integration
    /// - Application settings (Admin, Email, SMS)
    /// </remarks>
    public static class WebLayerExtensions
    {
        #region Public API

        /// <summary>
        /// Configures all web layer services for the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The same service collection so multiple calls can be chained.</returns>
        /// <remarks>
        /// This method orchestrates the configuration of:
        /// <list type="bullet">
        /// <item><description>Controllers with model validation filter</description></item>
        /// <item><description>HTTP client factory and HTTP context accessor</description></item>
        /// <item><description>SignalR for real-time features</description></item>
        /// <item><description>CORS policy for frontend access</description></item>
        /// <item><description>Stripe payment configuration</description></item>
        /// <item><description>Various application settings (Admin, Email, SMS)</description></item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddWebLayer(this IServiceCollection services, IConfiguration configuration)
        {
            #region Controllers and Filters

            // Add controllers with custom model validation filter
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelFilter>();
            });

            #endregion

            #region Core HTTP Services

            // Add HTTP client for external API calls
            services.AddHttpClient();

            // Add HTTP context accessor (required for ICurrentUserService)
            services.AddHttpContextAccessor();

            // Add SignalR
            services.AddSignalR();

            #endregion

            #region CORS Configuration

            ConfigureCORS(services);

            #endregion

            #region Third-Party Integrations

            // Configure Stripe API
            ConfigureStripe(configuration);

            #endregion


            #region Application Settings

            // Configure various application settings
            ConfigureAdminSettings(services, configuration);
            ConfigureEmailSettings(services, configuration);
            ConfigureSmsSettings(services, configuration);
            ConfigureOtpSettings(services, configuration);

            #endregion

            return services;
        }

        #endregion

        #region Private Configuration Methods

        /// <summary>
        /// Configures Cross-Origin Resource Sharing (CORS) policy to allow frontend applications.
        /// </summary>
        /// <param name="services">The service collection to add CORS configuration to.</param>
        /// <remarks>
        /// The configured policy allows requests from:
        /// <list type="bullet">
        /// <item><description>Angular development server (http://localhost:4200, https://localhost:4200)</description></item>
        /// <item><description>Additional development URLs (https://localhost:64622, http://localhost:64622)</description></item>
        /// </list>
        /// The policy allows any header, any method, and credentials.
        /// </remarks>
        private static void ConfigureCORS(IServiceCollection services)
        {
            // Add CORS policy to allow requests from the specified origin
            services.AddCors(options =>
            {
                options.AddPolicy("AllowMyOrigin", builder =>
                    builder.WithOrigins(
                            "http://localhost:4200",
                            "https://localhost:4200",
                            "https://localhost:64622",
                            "http://localhost:64622")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });
        }

        /// <summary>
        /// Configures Stripe payment gateway with the API key from configuration.
        /// </summary>
        /// <param name="configuration">The application configuration containing Stripe settings.</param>
        /// <remarks>
        /// The Stripe API key is read from the "Stripe:SecretKey" configuration path.
        /// In production, ensure this key is stored in a secure secret manager.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the Stripe secret key is missing from configuration.
        /// </exception>
        private static void ConfigureStripe(IConfiguration configuration)
        {
            var stripeSecretKey = configuration.GetSection("StripeKeys:SecretKey").Get<string>()
            ?? throw new ArgumentNullException("StripeKeys:SecretKey is missing from configuration");

            StripeConfiguration.ApiKey = stripeSecretKey;
        }

        /// <summary>
        /// Configures administrator settings from application configuration.
        /// </summary>
        /// <param name="services">The service collection to add settings to.</param>
        /// <param name="configuration">The application configuration containing admin settings.</param>
        /// <remarks>
        /// Settings are bound to <see cref="AdminSettings"/> class for strongly-typed access.
        /// </remarks>
        private static void ConfigureAdminSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AdminSettings>(configuration.GetSection("AdminSettings"));
        }

        /// <summary>
        /// Configures email service settings from application configuration.
        /// </summary>
        /// <param name="services">The service collection to add settings to.</param>
        /// <param name="configuration">The application configuration containing email settings.</param>
        /// <remarks>
        /// Settings include SMTP server configuration, credentials, and email templates.
        /// Settings are bound to <see cref="EmailSettings"/> class for strongly-typed access.
        /// </remarks>
        private static void ConfigureEmailSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        }

        /// <summary>
        /// Configures SMS service settings from application configuration.
        /// </summary>
        /// <param name="services">The service collection to add settings to.</param>
        /// <param name="configuration">The application configuration containing SMS settings.</param>
        /// <remarks>
        /// Settings include SMS provider configuration, API keys, and phone number formats.
        /// Settings are bound to <see cref="SMSSettings"/> class for strongly-typed access.
        /// </remarks>
        private static void ConfigureSmsSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SMSSettings>(configuration.GetSection("SMSSettings"));
        }

        /// <summary>
        /// Configures OTP service settings from application configuration.
        /// </summary>
        /// <param name="services">The service collection to add settings to.</param>
        /// <param name="configuration">The application configuration containing OTP settings.</param>
        /// <remarks>
        /// Settings include OTP provider configuration, API keys, and phone number formats.
        /// Settings are bound to <see cref="OtpSettings"/> class for strongly-typed access.
        /// </remarks>
        private static void ConfigureOtpSettings(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OtpSettings>(configuration.GetSection("OtpSettings"));
        }
        #endregion
    }
}