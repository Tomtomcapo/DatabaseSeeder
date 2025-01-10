using System.Collections.Concurrent;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseSeeder.Core.Implementation;

public class SeederRegistry : ISeederRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, ISeeder> _seeders;
    private readonly SemaphoreSlim _initializationLock;

    public SeederRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _seeders = new ConcurrentDictionary<Type, ISeeder>();
        _initializationLock = new SemaphoreSlim(1, 1);
    }

    public IEnumerable<ISeeder> GetAllSeeders()
    {
        if (_seeders.Any())
        {
            return _seeders.Values;
        }

        return InitializeSeeders();
    }

    public ISeeder GetSeeder(Type seederType)
    {
        ArgumentNullException.ThrowIfNull(seederType);
        
        if (!typeof(ISeeder).IsAssignableFrom(seederType))
        {
            throw new ArgumentException($"Type {seederType.Name} does not implement ISeeder", nameof(seederType));
        }

        return _seeders.GetOrAdd(seederType, type =>
        {
            using var scope = _serviceProvider.CreateScope();
            return (ISeeder)scope.ServiceProvider.GetRequiredService(type);
        });
    }

    private IEnumerable<ISeeder> InitializeSeeders()
    {
        _initializationLock.Wait();
        try
        {
            if (_seeders.Any())
            {
                return _seeders.Values;
            }

            using var scope = _serviceProvider.CreateScope();
            var seeders = scope.ServiceProvider.GetServices<ISeeder>();
            var seederTypes = new HashSet<Type>();

            foreach (var seeder in seeders)
            {
                var seederType = seeder.GetType();
                if (!seederTypes.Add(seederType))
                {
                    throw new SeederException(
                        $"Multiple seeders of type '{seederType.Name}' were registered. Each seeder type must be unique.");
                }

                _seeders.TryAdd(seederType, seeder);
            }

            return _seeders.Values;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public void Dispose()
    {
        _initializationLock.Dispose();
    }
}