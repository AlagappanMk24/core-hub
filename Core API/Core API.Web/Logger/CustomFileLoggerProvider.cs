namespace Core_API.Web.Logger
{
    public class CustomFileLoggerProvider : ILoggerProvider
    {
        private readonly string _logFilePath;
        private CustomFileLogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomFileLoggerProvider(string logFilePath, IHttpContextAccessor httpContextAccessor)
        {
            _logFilePath = logFilePath;
            _httpContextAccessor = httpContextAccessor;
            _logger = new CustomFileLogger(_logFilePath, httpContextAccessor);
        }

        // Creates and returns a logger instance.
        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }
        public void Dispose() => _logger?.Dispose();
    }
}
