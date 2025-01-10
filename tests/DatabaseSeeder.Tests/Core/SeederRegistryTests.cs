using Xunit;
using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Core.Implementation;
using DatabaseSeeder.Exceptions;

namespace DatabaseSeeder.Tests.Core;

public class SeederRegistryTests
{
    [Fact]
    public void GetAllSeeders_WithDuplicateSeederTypes_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ISeeder, TestSeeder>();
        services.AddScoped<ISeeder, TestSeeder>(); // Adding same type twice
        var serviceProvider = services.BuildServiceProvider();
        var registry = new SeederRegistry(serviceProvider);

        // Act & Assert
        var exception = Assert.Throws<SeederException>(() => registry.GetAllSeeders().ToList());
        Assert.Contains("Multiple seeders of type", exception.Message);
    }

    private class TestSeeder : ISeeder
    {
        public int Order => 1;
        public string Name => "TestSeeder";
        public IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();
        public Task SeedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}