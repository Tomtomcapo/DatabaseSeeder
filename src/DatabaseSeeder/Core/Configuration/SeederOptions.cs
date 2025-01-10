namespace DatabaseSeeder.Core.Configuration;

public class SeederOptions
{
    /// <summary>
    /// Gets or sets whether to throw an exception when a circular dependency is detected
    /// </summary>
    public bool ThrowOnCircularDependency { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to run seeders in parallel when possible
    /// </summary>
    public bool EnableParallelization { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum degree of parallelization when parallel execution is enabled
    /// </summary>
    public int MaxDegreeOfParallelization { get; set; } = 4;

    /// <summary>
    /// Gets or sets whether to continue seeding when an error occurs
    /// </summary>
    public bool ContinueOnError { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout for each seeder in seconds
    /// </summary>
    public int SeederTimeout { get; set; } = 300;
}