using Microsoft.Extensions.Logging;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly TestLogger _logger = new();

    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose() { }

    public List<LogEntry> Logs => _logger.Logs;

    public class TestLogger : ILogger
    {
        public List<LogEntry> Logs { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
                                TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(new LogEntry
            {
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            });
        }
    }

    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; } = "";
        public Exception? Exception { get; set; }
    }

    private class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}