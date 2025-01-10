using System.Threading.Tasks;

namespace DatabaseSeeder.Core.Abstractions;

public interface ISeeder
{
    /// <summary>
    /// Gets the order in which this seeder should run
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Gets the name of the seeder for logging and debugging purposes
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the dependencies of this seeder
    /// </summary>
    IEnumerable<Type> Dependencies { get; }

    /// <summary>
    /// Executes the seeding operation
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
