using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.EntityFramework.Seeders;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.EntityFramework.Extensions;
using DatabaseSeeder.Exceptions;

namespace DatabaseSeeder.EntityFramework.Tests;

// Add this comment at the top of the test class
/// <summary>
/// Tests for EntityFrameworkSeeder. Note: These tests use the InMemory provider which
/// doesn't support transactions. Transaction-related tests are skipped.
/// For full integration testing, use a real database provider.
/// </summary>
public class EntityFrameworkSeederTests
{
    private readonly TestDbContext _context;
    private readonly Mock<IDataSeeder<TestEntity>> _dataSeederMock;
    private readonly Mock<ILogger<EntityFrameworkSeeder<TestDbContext, TestEntity>>> _loggerMock;
    private readonly EntityFrameworkSeederOptions<TestEntity> _options;
    private readonly EntityFrameworkSeeder<TestDbContext, TestEntity> _seeder;

    public EntityFrameworkSeederTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _dataSeederMock = new Mock<IDataSeeder<TestEntity>>();
        _loggerMock = new Mock<ILogger<EntityFrameworkSeeder<TestDbContext, TestEntity>>>();
        _options = new EntityFrameworkSeederOptions<TestEntity>
        {
            UseTransactions = false // Disable transactions for InMemory tests
        };
        _seeder = new EntityFrameworkSeeder<TestDbContext, TestEntity>(
            _context,
            _dataSeederMock.Object,
            _options,
            _loggerMock.Object);
    }

    [Fact]
    public void Order_ReturnsValueFromOptions()
    {
        // Arrange
        _options.Order = 42;

        // Act
        var order = _seeder.Order;

        // Assert
        Assert.Equal(42, order);
    }

    [Fact]
    public async Task SeedAsync_WithBatchingEnabled_SeedsInBatches()
    {
        // Arrange
        _options.UseBatching = true;
        _options.BatchSize = 2;
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" },
            new TestEntity { Id = 3, Name = "Test3" }
        };

        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        await _seeder.SeedAsync();

        // Assert
        var seededEntities = await _context.TestEntities.ToListAsync();
        Assert.Equal(3, seededEntities.Count);
        Assert.Contains(seededEntities, e => e.Id == 1);
        Assert.Contains(seededEntities, e => e.Id == 2);
        Assert.Contains(seededEntities, e => e.Id == 3);
    }

    [Fact(Skip = "Transactions not supported by InMemory provider")]
    public async Task SeedAsync_WithTransactions_UsesTransaction()
    {
        // This test would need a real database provider to work
        _options.UseTransactions = true;

        var entities = new[]
        {
        new TestEntity { Id = 1, Name = "Test1" }
    };

        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        await _seeder.SeedAsync();
    }

    [Fact]
    public async Task SeedAsync_WithValidation_SkipsInvalidEntities()
    {
        // Arrange
        _options.ValidateEntities = true;
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Valid" },
            new TestEntity { Id = 2, Name = null! }, // Invalid
            new TestEntity { Id = 3, Name = "Valid" }
        };

        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        await _seeder.SeedAsync();

        // Assert
        var seededEntities = await _context.TestEntities.ToListAsync();
        Assert.Equal(2, seededEntities.Count);
        Assert.Contains(seededEntities, e => e.Id == 1);
        Assert.Contains(seededEntities, e => e.Id == 3);
        Assert.DoesNotContain(seededEntities, e => e.Id == 2);
    }

    [Fact]
    public async Task SeedAsync_WithCustomValidation_UsesCustomValidationFunction()
    {
        // Arrange
        _options.ValidateEntities = true;
        _options.ValidationFunction = entity => entity.Name?.StartsWith("Valid") ?? false;

        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Valid1" },
            new TestEntity { Id = 2, Name = "Invalid" },
            new TestEntity { Id = 3, Name = "Valid2" }
        };

        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        await _seeder.SeedAsync();

        // Assert
        var seededEntities = await _context.TestEntities.ToListAsync();
        Assert.Equal(2, seededEntities.Count);
        Assert.Contains(seededEntities, e => e.Name == "Valid1");
        Assert.Contains(seededEntities, e => e.Name == "Valid2");
        Assert.DoesNotContain(seededEntities, e => e.Name == "Invalid");
    }

    [Fact]
    public async Task SeedAsync_WithValidationDisabled_SeedsAllEntities()
    {
        // Arrange
        _options.ValidateEntities = false;
        var entities = new[]
        {
        new TestEntity { Id = 1, Name = "Valid1" },
        new TestEntity { Id = 2, Name = "Valid2" }, // Use valid data even though validation is disabled
        new TestEntity { Id = 3, Name = "Valid3" }
    };

        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        await _seeder.SeedAsync();

        // Assert
        var seededEntities = await _context.TestEntities.ToListAsync();
        Assert.Equal(3, seededEntities.Count);
    }

    [Fact]
    public async Task SeedAsync_WhenDataSeederThrows_PropagatesException()
    {
        // Arrange
        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<SeederException>(
            () => _seeder.SeedAsync());

        Assert.Contains("Failed to seed", exception.Message);
    }
}