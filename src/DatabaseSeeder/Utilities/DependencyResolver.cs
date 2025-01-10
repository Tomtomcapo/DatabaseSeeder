using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Exceptions;

namespace DatabaseSeeder.Utilities;

public class DependencyResolver
{
    private readonly bool _throwOnCircularDependency;
    private readonly HashSet<Type> _visited;
    private readonly HashSet<Type> _processing;
    private readonly List<ISeeder> _resolved;
    private readonly Dictionary<Type, ISeeder> _seederMap;

    public DependencyResolver(bool throwOnCircularDependency)
    {
        _throwOnCircularDependency = throwOnCircularDependency;
        _visited = new HashSet<Type>();
        _processing = new HashSet<Type>();
        _resolved = new List<ISeeder>();
        _seederMap = new Dictionary<Type, ISeeder>();
    }

    public IEnumerable<ISeeder> ResolveOrder(IEnumerable<ISeeder> seeders)
    {
        // Reset state
        _visited.Clear();
        _processing.Clear();
        _resolved.Clear();
        _seederMap.Clear();

        // Build seeder map
        foreach (var seeder in seeders)
        {
            _seederMap[seeder.GetType()] = seeder;
        }

        // First, resolve dependencies
        foreach (var seeder in seeders)
        {
            if (!_visited.Contains(seeder.GetType()))
            {
                Visit(seeder);
            }
        }

        // Then group by Order and sort within each group
        var orderedGroups = _resolved
            .GroupBy(s => s.Order)
            .OrderBy(g => g.Key);

        var result = new List<ISeeder>();

        foreach (var group in orderedGroups)
        {
            // Within each Order group, ensure dependencies come first
            var groupSeeders = group.ToList();
            var orderedGroup = new List<ISeeder>();
            var processed = new HashSet<Type>();

            // First, add seeders that are dependencies of others
            foreach (var seeder in groupSeeders)
            {
                if (groupSeeders.Any(s => s.Dependencies.Contains(seeder.GetType())))
                {
                    orderedGroup.Add(seeder);
                    processed.Add(seeder.GetType());
                }
            }

            // Then add remaining seeders
            foreach (var seeder in groupSeeders)
            {
                if (!processed.Contains(seeder.GetType()))
                {
                    orderedGroup.Add(seeder);
                }
            }

            result.AddRange(orderedGroup);
        }

        return result;
    }

    private void Visit(ISeeder seeder)
    {
        var seederType = seeder.GetType();

        if (_processing.Contains(seederType))
        {
            if (_throwOnCircularDependency)
            {
                throw new CircularDependencyException(new[] { seederType });
            }
            return;
        }

        if (_visited.Contains(seederType))
        {
            return;
        }

        _processing.Add(seederType);

        foreach (var dependencyType in seeder.Dependencies)
        {
            if (_seederMap.TryGetValue(dependencyType, out var dependencySeeder))
            {
                Visit(dependencySeeder);
            }
        }

        if (!_resolved.Contains(seeder))
        {
            _resolved.Add(seeder);
        }

        _processing.Remove(seederType);
        _visited.Add(seederType);
    }
}