using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Json.Providers;
using DatabaseSeeder.Json.Extensions;
using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Configuration;

namespace DatabaseSeeder.Json.Tests.Providers;

public class JsonDataProviderTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _jsonFilePath;
    private readonly Mock<ILogger<JsonDataProvider<TestEntity>>> _loggerMock;
    private readonly List<TestEntity> _testData;

    public JsonDataProviderTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _jsonFilePath = Path.Combine(_tempDirectory, "test.json");
        _loggerMock = new Mock<ILogger<JsonDataProvider<TestEntity>>>();
        _testData = new List<TestEntity>
        {
            new() { Id = 1, Name = "Test 1" },
            new() { Id = 2, Name = "Test 2" }
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    private void CreateTestJsonFile(List<TestEntity> entities)
    {
        var json = JsonSerializer.Serialize(entities, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_jsonFilePath, json);
    }

    [Fact]
    public async Task GetSeedDataAsync_WithValidJson_ReturnsParsedEntities()
    {
        // Arrange
        CreateTestJsonFile(_testData);
        var provider = new JsonDataProvider<TestEntity>(_jsonFilePath, _loggerMock.Object);

        // Act
        var result = await provider.GetSeedDataAsync();

        // Assert
        var entities = result.ToList();
        Assert.Equal(2, entities.Count);
        Assert.Equal(_testData[0].Id, entities[0].Id);
        Assert.Equal(_testData[0].Name, entities[0].Name);
        Assert.Equal(_testData[1].Id, entities[1].Id);
        Assert.Equal(_testData[1].Name, entities[1].Name);
    }

    [Fact]
    public async Task GetSeedDataAsync_WithEmptyJson_ReturnsEmptyCollection()
    {
        // Arrange
        CreateTestJsonFile(new List<TestEntity>());
        var provider = new JsonDataProvider<TestEntity>(_jsonFilePath, _loggerMock.Object);

        // Act
        var result = await provider.GetSeedDataAsync();

        // Assert
        Assert.Empty(result);
        VerifyWarningLogged("No entities found in JSON file");
    }

    [Fact]
    public async Task GetSeedDataAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.json");
        var provider = new JsonDataProvider<TestEntity>(nonExistentPath, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => provider.GetSeedDataAsync());
    }

    [Fact]
    public async Task GetSeedDataAsync_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        File.WriteAllText(_jsonFilePath, "invalid json content");
        var provider = new JsonDataProvider<TestEntity>(_jsonFilePath, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => provider.GetSeedDataAsync());
        VerifyErrorLogged("Error deserializing JSON data from file");
    }

    [Fact]
    public async Task GetSeedDataAsync_WithCustomJsonOptions_UsesProvidedOptions()
    {
        // Arrange
        var json = """
        [
            { "ID": 1, "NAME": "Test 1" },
            { "ID": 2, "NAME": "Test 2" }
        ]
        """;
        File.WriteAllText(_jsonFilePath, json);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var provider = new JsonDataProvider<TestEntity>(_jsonFilePath, _loggerMock.Object, 
            jsonOptions: options);

        // Act
        var result = await provider.GetSeedDataAsync();

        // Assert
        var entities = result.ToList();
        Assert.Equal(2, entities.Count);
        Assert.Equal(1, entities[0].Id);
        Assert.Equal("Test 1", entities[0].Name);
    }

    [Fact]
    public void Constructor_WithNullFilePath_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new JsonDataProvider<TestEntity>(null!, _loggerMock.Object));
        Assert.Equal("jsonFilePath", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new JsonDataProvider<TestEntity>(_jsonFilePath, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void ShouldCleanExistingData_ReturnsConfiguredValue()
    {
        // Arrange
        var provider = new JsonDataProvider<TestEntity>(_jsonFilePath, _loggerMock.Object, 
            shouldCleanExisting: false);

        // Assert
        Assert.False(provider.ShouldCleanExistingData);
    }

    private void VerifyWarningLogged(string messageContains)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyErrorLogged(string messageContains)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}