namespace DatabaseSeeder.Exceptions;

public class CircularDependencyException : SeederException
{
    public IEnumerable<Type> DependencyCycle { get; }

    public CircularDependencyException(IEnumerable<Type> dependencyCycle)
        : base($"Circular dependency detected: {FormatDependencyCycle(dependencyCycle)}")
    {
        DependencyCycle = dependencyCycle;
    }

    private static string FormatDependencyCycle(IEnumerable<Type> cycle)
    {
        return string.Join(" -> ", cycle.Select(t => t.Name)) + " -> " + cycle.First().Name;
    }
}
