namespace DatabaseSeeder.Core.Abstractions;

public interface ISeederOrchestrator
{
    /// <summary>
    /// Executes all registered seeders in the correct order
    /// </summary>
    Task SeedAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes specific seeders by their types
    /// </summary>
    Task SeedAsync(IEnumerable<Type> seederTypes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all registered seeders in their execution order
    /// </summary>
    IEnumerable<ISeeder> GetOrderedSeeders();
}
