using Microsoft.Extensions.Logging;

namespace DatabaseSeeder.Utilities;

public class SeederLogger
{
    private readonly ILogger _logger;
    private readonly string _seederName;

    public SeederLogger(ILogger logger, string seederName)
    {
        _logger = logger;
        _seederName = seederName;
    }

    public void LogInformation(string message)
    {
        _logger.LogInformation($"[{_seederName}] {message}");
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning($"[{_seederName}] {message}");
    }

    public void LogError(string message, Exception? exception = null)
    {
        _logger.LogError(exception, $"[{_seederName}] {message}");
    }

    public IDisposable BeginScope(string scopeName)
    {
        return _logger.BeginScope($"{_seederName}:{scopeName}");
    }
}