using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Abstractions;

namespace DatabaseSeeder.Core.Base;

public abstract class BaseSeeder : ISeeder
{
    protected readonly ILogger _logger;

    protected BaseSeeder(ILogger logger)
    {
        _logger = logger;
    }

    public abstract int Order { get; }

    public virtual string Name => GetType().Name;

    public virtual IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();

    public abstract Task SeedAsync(CancellationToken cancellationToken = default);

    protected virtual void LogInformation(string message)
    {
        _logger.LogInformation($"[{Name}] {message}");
    }

    protected virtual void LogWarning(string message)
    {
        _logger.LogWarning($"[{Name}] {message}");
    }

    protected virtual void LogError(string message, Exception? exception = null)
    {
        _logger.LogError(exception, $"[{Name}] {message}");
    }
}
