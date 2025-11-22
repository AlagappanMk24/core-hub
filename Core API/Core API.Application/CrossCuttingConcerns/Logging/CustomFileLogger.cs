using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Core_API.Application.CrossCuttingConcerns.Logging
{
    public class CustomFileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly object _lock = new object();
        public CustomFileLogger(string logFilePath, IHttpContextAccessor httpContextAccessor)
        {
            _logFilePath = logFilePath;
            _httpContextAccessor = httpContextAccessor;

            // Ensure the log directory exists
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Ensure the log file is created
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Dispose();
            }
        }
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            lock (_lock)
            {
                // Extract contextual data
                string currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";

                string requestId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? "N/A";

                //In your logs, the IP address::1 indicates that the requests are coming from the local machine, which suggests these actions
                //(like login attempts or fetching employee details) are being performed locally or in a development environment.
                string ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "N/A";

                // Create a detailed and styled log entry
                string logEntry = $@"
                    ===============================================================
                    🕒 TIMESTAMP    : {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                    🔍 LOG LEVEL    : {logLevel.ToString().ToUpper()}
                    👤 USERNAME     : {currentUsername}
                    🌐 IP ADDRESS   : {ipAddress}
                    🛠️ REQUEST ID   : {requestId}
                    📄 MESSAGE      : {formatter(state, exception)}
                    📅 ACTION DATE  : {DateTime.Now:dd-MM-yyyy HH:mm:ss}
                    ===============================================================
                ";

                // Write to the log file
                File.AppendAllText(_logFilePath, logEntry);
            }
        }
        public void Dispose() { }
    }
}
