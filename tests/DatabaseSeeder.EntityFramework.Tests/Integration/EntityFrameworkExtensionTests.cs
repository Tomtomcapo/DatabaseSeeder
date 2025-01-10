using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.EntityFramework.Seeders;
using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;

namespace DatabaseSeeder.EntityFramework.Tests;

public class EntityFrameworkExtensionsTests
{
    [Fact]
    public void AddEntityFrameworkSeeder_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add DbContext to services
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        var builder = new SeederBuilder(services);

        // Act
        // Add required services
        services.AddScoped<IDataSeeder<TestEntity>>(sp =>
            new Mock<IDataSeeder<TestEntity>>().Object);
        services.AddLogging(builder => builder.AddConsole());

        builder.AddEntityFrameworkSeeder<TestDbContext, TestEntity>(options =>
        {
            options.Order = 42;
            options.UseBatching = true;
            options.BatchSize = 100;
        });

        var serviceProvider = builder.Build().BuildServiceProvider();

        // Assert
        var seeder = serviceProvider.GetService<ISeeder>();
        Assert.NotNull(seeder);
        Assert.IsType<EntityFrameworkSeeder<TestDbContext, TestEntity>>(seeder);
    }

    [Fact]
    public void AddEntityFrameworkSeeder_WithDefaultOptions_UsesDefaultValues()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add DbContext to services
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add required services
        services.AddScoped<IDataSeeder<TestEntity>>(sp =>
            new Mock<IDataSeeder<TestEntity>>().Object);

        // Add logging
        services.AddLogging();
        services.AddSingleton<ILoggerFactory>(new LoggerFactory());
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        var builder = new SeederBuilder(services);

        // Act
        builder.AddEntityFrameworkSeeder<TestDbContext, TestEntity>();
        var serviceProvider = builder.Build().BuildServiceProvider();

        // Assert
        var seeder = serviceProvider.GetService<ISeeder>() as EntityFrameworkSeeder<TestDbContext, TestEntity>;
        Assert.NotNull(seeder);
        Assert.Equal(1, seeder.Order); // Default order should be 1
    }
}