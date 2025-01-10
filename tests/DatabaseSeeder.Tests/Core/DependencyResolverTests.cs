using Xunit;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Exceptions;
using DatabaseSeeder.Utilities;

namespace DatabaseSeeder.Tests.Core;

public class DependencyResolverTests
{
    private readonly DependencyResolver _resolverWithThrow;
    private readonly DependencyResolver _resolverWithoutThrow;

    public DependencyResolverTests()
    {
        _resolverWithThrow = new DependencyResolver(throwOnCircularDependency: true);
        _resolverWithoutThrow = new DependencyResolver(throwOnCircularDependency: false);
    }

    private class SeederA : ISeeder
    {
        public SeederA() { }
        public int Order { get; set; }
        public string Name { get; set; }
        public IEnumerable<Type> Dependencies { get; set; } = Array.Empty<Type>();
        public Task SeedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class SeederB : ISeeder
    {
        public SeederB() { }
        public int Order { get; set; }
        public string Name { get; set; }
        public IEnumerable<Type> Dependencies { get; set; } = Array.Empty<Type>();
        public Task SeedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class SeederC : ISeeder
    {
        public SeederC() { }
        public int Order { get; set; }
        public string Name { get; set; }
        public IEnumerable<Type> Dependencies { get; set; } = Array.Empty<Type>();
        public Task SeedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    [Fact]
    public void ResolveOrder_WithNoDependencies_ReturnsOrderedByPriority()
    {
        // Arrange
        var seeders = new ISeeder[]
        {
            new SeederA { Order = 2, Name = "Seeder1" },
            new SeederB { Order = 1, Name = "Seeder2" },
            new SeederC { Order = 3, Name = "Seeder3" }
        };

        // Act
        var result = _resolverWithThrow.ResolveOrder(seeders).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Seeder2", result[0].Name);
        Assert.Equal("Seeder1", result[1].Name);
        Assert.Equal("Seeder3", result[2].Name);
    }

    [Fact]
    public void ResolveOrder_WithDependencies_ReturnsCorrectOrder()
    {
        // Arrange
        var seeder2 = new SeederB { Order = 1, Name = "Seeder2" };
        var seeder1 = new SeederA 
        { 
            Order = 1, 
            Name = "Seeder1",
            Dependencies = new[] { typeof(SeederB) }
        };

        var seeders = new ISeeder[] { seeder1, seeder2 };

        // Act
        var result = _resolverWithThrow.ResolveOrder(seeders).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Seeder2", result[0].Name);
        Assert.Equal("Seeder1", result[1].Name);
    }

    [Fact]
    public void ResolveOrder_WithCircularDependencies_ThrowsException()
    {
        // Arrange
        var seeder1 = new SeederA
        {
            Order = 1,
            Name = "Seeder1",
            Dependencies = new[] { typeof(SeederB) }
        };

        var seeder2 = new SeederB
        {
            Order = 1,
            Name = "Seeder2",
            Dependencies = new[] { typeof(SeederA) }
        };

        var seeders = new ISeeder[] { seeder1, seeder2 };

        // Act & Assert
        var exception = Assert.Throws<CircularDependencyException>(
            () => _resolverWithThrow.ResolveOrder(seeders).ToList());
    }

    [Fact]
    public void ResolveOrder_WithCircularDependencies_SkipsWhenNotThrowing()
    {
        // Arrange
        var seeder1 = new SeederA
        {
            Order = 1,
            Name = "Seeder1",
            Dependencies = new[] { typeof(SeederB) }
        };

        var seeder2 = new SeederB
        {
            Order = 1,
            Name = "Seeder2",
            Dependencies = new[] { typeof(SeederA) }
        };

        var seeders = new ISeeder[] { seeder1, seeder2 };

        // Act
        var result = _resolverWithoutThrow.ResolveOrder(seeders).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Name == "Seeder1");
        Assert.Contains(result, s => s.Name == "Seeder2");
    }
}