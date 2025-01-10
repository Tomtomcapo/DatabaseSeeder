namespace DatabaseSeeder.Core.Abstractions;

public interface IDataSeeder<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the seed data for the entity
    /// </summary>
    Task<IEnumerable<TEntity>> GetSeedDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether the seeder should clean existing data before seeding
    /// </summary>
    bool ShouldCleanExistingData { get; }
}
