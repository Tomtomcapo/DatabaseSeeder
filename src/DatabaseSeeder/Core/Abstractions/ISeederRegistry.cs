using DatabaseSeeder.Core.Abstractions;

namespace DatabaseSeeder.Core.Abstractions;

public interface ISeederRegistry
{
    IEnumerable<ISeeder> GetAllSeeders();
    ISeeder GetSeeder(Type seederType);
}
