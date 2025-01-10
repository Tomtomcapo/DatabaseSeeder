using Xunit;
using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Core.Implementation;

namespace DatabaseSeeder.Tests.Core;

public class SeederBuilderTests
{
    private readonly IServiceCollection _services;
    private readonly SeederBuilder _builder;

    public SeederBuilderTests()
    {
        _services = new ServiceCollection();
        _builder = new SeederBuilder(_services);
    }

    [Fact]
    public void Configure_SetsOptions()
    {
        // Arrange & Act
        _builder.Configure(options =>
        {
            options.EnableParallelization = true;
            options.MaxDegreeOfParallelization = 4;
        });

        var provider = _builder.Build().BuildServiceProvider();
        var options = provider.GetRequiredService<SeederOptions>();

        // Assert
        Assert.True(options.EnableParallelization);
        Assert.Equal(4, options.MaxDegreeOfParallelization);
    }

    [Fact]
    public void AddSeeder_RegistersSeeder()
    {
        // Arrange & Act
        _builder.AddSeeder<TestSeeder>();
        var provider = _builder.Build().BuildServiceProvider();

        // Assert
        var seeders = provider.GetServices<ISeeder>();
        Assert.Single(seeders);
        Assert.IsType<TestSeeder>(seeders.First());
    }

    [Fact]
    public void AddDataSeeder_RegistersDataSeeder()
    {
        // Arrange & Act
        _builder.AddDataSeeder<TestEntity, TestDataSeeder>();
        var provider = _builder.Build().BuildServiceProvider();

        // Assert
        var dataSeeder = provider.GetService<IDataSeeder<TestEntity>>();
        Assert.NotNull(dataSeeder);
        Assert.IsType<TestDataSeeder>(dataSeeder);
    }

    [Fact]
    public void Build_RegistersRequiredServices()
    {
        // Arrange
        _services.AddLogging();

        // Act
        var provider = _builder.Build().BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<SeederOptions>());
        Assert.NotNull(provider.GetService<ISeederOrchestrator>());
        Assert.NotNull(provider.GetService<ISeederRegistry>());
    }

    private class TestEntity { }

    private class TestSeeder : ISeeder
    {
        public int Order => 1;
        public string Name => "TestSeeder";
        public IEnumerable<Type> Dependencies => Enumerable.Empty<Type>();
        public Task SeedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class TestDataSeeder : IDataSeeder<TestEntity>
    {
        public bool ShouldCleanExistingData => false;
        public Task<IEnumerable<TestEntity>> GetSeedDataAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Enumerable.Empty<TestEntity>());
    }
}
