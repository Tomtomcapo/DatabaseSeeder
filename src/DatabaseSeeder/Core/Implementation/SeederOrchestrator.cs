using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.Exceptions;
using DatabaseSeeder.Utilities;
using Microsoft.Extensions.Logging;

namespace DatabaseSeeder.Core.Implementation;

public class SeederOrchestrator : ISeederOrchestrator
{
    private readonly ISeederRegistry _registry;
    private readonly ILogger<SeederOrchestrator> _logger;
    private readonly SeederOptions _options;
    private readonly DependencyResolver _dependencyResolver;

    public SeederOrchestrator(
        ISeederRegistry registry,
        ILogger<SeederOrchestrator> logger,
        SeederOptions options)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _dependencyResolver = new DependencyResolver(options.ThrowOnCircularDependency);
    }

    public Task SeedAllAsync(CancellationToken cancellationToken = default) =>
        ExecuteSeedersAsync(GetOrderedSeeders(), cancellationToken);

    public Task SeedAsync(IEnumerable<Type> seederTypes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(seederTypes);
        var seeders = GetOrderedSeeders().Where(s => seederTypes.Contains(s.GetType()));
        return ExecuteSeedersAsync(seeders, cancellationToken);
    }

    public IEnumerable<ISeeder> GetOrderedSeeders() => 
        _dependencyResolver.ResolveOrder(_registry.GetAllSeeders());

    private async Task ExecuteSeedersAsync(IEnumerable<ISeeder> seeders, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(seeders);
        
        if (_options.EnableParallelization)
        {
            await ExecuteSeedersInParallelAsync(seeders, cancellationToken);
            return;
        }
        
        await ExecuteSeedersSequentiallyAsync(seeders, cancellationToken);
    }

    private async Task ExecuteSeedersInParallelAsync(IEnumerable<ISeeder> seeders, CancellationToken cancellationToken)
    {
        var seedersByOrder = seeders
            .GroupBy(s => s.Order)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var group in seedersByOrder)
        {
            using var throttler = new SemaphoreSlim(Math.Min(group.Count(), _options.MaxDegreeOfParallelization));
            var tasks = group.Select(async seeder =>
            {
                await throttler.WaitAsync(cancellationToken);
                try
                {
                    await ExecuteSeederWithTimeoutAsync(seeder, cancellationToken);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    private async Task ExecuteSeedersSequentiallyAsync(IEnumerable<ISeeder> seeders, CancellationToken cancellationToken)
    {
        foreach (var seeder in seeders)
        {
            await ExecuteSeederWithTimeoutAsync(seeder, cancellationToken);
        }
    }

    private async Task ExecuteSeederWithTimeoutAsync(ISeeder seeder, CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromSeconds(_options.SeederTimeout);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            var seedTask = seeder.SeedAsync(timeoutCts.Token);
            
            if (await Task.WhenAny(seedTask, Task.Delay(timeout, timeoutCts.Token)) != seedTask)
            {
                throw new SeederException($"Seeder {seeder.Name} timed out after {_options.SeederTimeout} seconds");
            }

            await seedTask;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is not SeederException)
        {
            _logger.LogError(ex, $"Error executing seeder: {seeder.Name}");
            if (!_options.ContinueOnError)
            {
                throw new SeederException($"Seeder {seeder.Name} failed: {ex.Message}", ex);
            }
        }
    }
}