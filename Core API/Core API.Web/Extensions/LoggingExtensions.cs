using Core_API.Application.CrossCuttingConcerns.Logging;

namespace Core_API.Web.Extensions;

/// <summary>
/// Provides extension methods for configuring custom file-based logging.
/// </summary>
/// <remarks>
/// This class enables logging to flat files in addition to the default logging providers.
/// File-based logging is useful for long-term log retention and offline analysis.
/// </remarks>
public static class LoggingExtensions
{
    #region Constants

    private const string LogFilePathConfigurationKey = "Logging:LogFilePath";
    private const string DefaultLogsFolderName = "Logs";
    private const string LogFileNamePattern = "log-{0}.txt";
    private const string DateFormatForLogFile = "MM-dd-yyyy";

    #endregion

    #region Public Methods

    /// <summary>
    /// Configures custom file-based logging for the application.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <remarks>
    /// This method performs the following:
    /// <list type="number">
    /// <item><description>Retrieves the log file path from configuration or uses a default</description></item>
    /// <item><description>Creates the log directory if it doesn't exist</description></item>
    /// <item><description>Generates a dated log filename (e.g., log-01-15-2026.txt)</description></item>
    /// <item><description>Registers a <see cref="CustomFileLoggerProvider"/> with the logging factory</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// app.ConfigureCustomFileLogging();
    /// </code>
    /// </example>
    public static void ConfigureCustomFileLogging(this WebApplication app)
    {
        #region Configuration Retrieval

        var configuration = app.Configuration;
        var formattedDate = DateTime.Now.ToString(DateFormatForLogFile);
        var baseLogPath = configuration.GetValue<string>(LogFilePathConfigurationKey);

        #endregion

        #region Log Directory Setup

        // Use default path if not configured in appsettings.json
        if (string.IsNullOrEmpty(baseLogPath))
        {
            baseLogPath = Path.Combine(Directory.GetCurrentDirectory(), DefaultLogsFolderName);
            Directory.CreateDirectory(baseLogPath);
        }

        #endregion

        #region Log File Path Construction

        var logFilePath = Path.Combine(baseLogPath, string.Format(LogFileNamePattern, formattedDate));

        #endregion

        #region Logger Provider Registration

        // Resolve required services from the application container
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

        // Add custom file logging provider
        loggerFactory.AddProvider(new CustomFileLoggerProvider(logFilePath, httpContextAccessor));

        #endregion
    }

    #endregion
}