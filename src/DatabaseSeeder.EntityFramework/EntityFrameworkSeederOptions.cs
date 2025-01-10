namespace DatabaseSeeder.EntityFramework;

public class EntityFrameworkSeederOptions<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets or sets the order in which this seeder should run
    /// </summary>
    public int Order { get; set; } = 1;
    /// <summary>
    /// Gets or sets whether to use batching when seeding large datasets
    /// </summary>
    public bool UseBatching { get; set; } = true;

    /// <summary>
    /// Gets or sets the batch size when batching is enabled
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to use transactions when seeding
    /// </summary>
    public bool UseTransactions { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate entities before seeding
    /// </summary>
    public bool ValidateEntities { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom validation function for entities
    /// </summary>
    public Func<TEntity, bool>? ValidationFunction { get; set; }
}