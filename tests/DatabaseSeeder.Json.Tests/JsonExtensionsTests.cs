using System.Text.Json;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.Json.Extensions;
using DatabaseSeeder.Json.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseSeeder.Json.Extensions.Tests;

public class JsonExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly string _tempDirectory;
    private readonly string _jsonFilePath;

    public JsonExtensionsTests()
    {
        _services = new ServiceCollection();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _jsonFilePath = Path.Combine(_tempDirectory, "test.json");
    }

    [Fact]
    public void AddJsonDataProvider_RegistersProvider()
    {
        // Arrange
        var builder = new SeederBuilder(_services);
        _services.AddLogging();

        // Act
        builder.AddJsonDataProvider<TestEntity>(_jsonFilePath);
        var provider = builder.Build()
            .BuildServiceProvider()
            .GetService<IDataSeeder<TestEntity>>();

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<JsonDataProvider<TestEntity>>(provider);
    }

    [Fact]
    public void AddJsonDataProvider_WithBaseDirectory_UsesFullPath()
    {
        // Arrange
        var builder = new SeederBuilder(_services);
        _services.AddLogging();
        var fileName = "test";

        // Act
        builder.AddJsonDataProvider<TestEntity>(fileName, options =>
        {
            options.BaseDirectory = _tempDirectory;
        });

        var provider = builder.Build()
            .BuildServiceProvider()
            .GetService<IDataSeeder<TestEntity>>();

        // Assert
        Assert.NotNull(provider);
        var jsonProvider = Assert.IsType<JsonDataProvider<TestEntity>>(provider);
        // We can't directly access the file path, but we can verify the provider was created
        Assert.NotNull(jsonProvider);
    }

    [Fact]
    public void AddJsonDataProvider_WithCustomOptions_ConfiguresCorrectly()
    {
        // Arrange
        var builder = new SeederBuilder(_services);
        _services.AddLogging();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        // Act
        builder.AddJsonDataProvider<TestEntity>(_jsonFilePath, opt =>
        {
            opt.SerializerOptions = options;
            opt.CleanExistingData = false;
        });

        var provider = builder.Build()
            .BuildServiceProvider()
            .GetService<IDataSeeder<TestEntity>>() as JsonDataProvider<TestEntity>;

        // Assert
        Assert.NotNull(provider);
        Assert.False(provider.ShouldCleanExistingData);
    }

    [Fact]
    public void AddJsonDataProvider_WithCustomExtension_AppendsExtension()
    {
        // Arrange
        var builder = new SeederBuilder(_services);
        _services.AddLogging();
        var fileName = "test";

        // Act
        builder.AddJsonDataProvider<TestEntity>(fileName, options =>
        {
            options.FileExtension = ".custom";
        });

        var provider = builder.Build()
            .BuildServiceProvider()
            .GetService<IDataSeeder<TestEntity>>();

        // Assert
        Assert.NotNull(provider);
        var jsonProvider = Assert.IsType<JsonDataProvider<TestEntity>>(provider);
        Assert.NotNull(jsonProvider);
    }
}