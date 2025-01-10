using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Base;
using DatabaseSeeder.Core.Abstractions;

namespace DatabaseSeeder.Tests.Core;

// Make test entities public
public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public DbSet<TestEntity> TestEntities { get; set; }
}

public class TestRelationalSeeder : RelationalSeeder<TestEntity>
{
    public TestRelationalSeeder(
        DbContext dbContext,
        IDataSeeder<TestEntity> dataSeeder,
        ILogger logger) : base(dbContext, dataSeeder, logger) { }

    public override int Order => 1;
}

public class RelationalSeederTests
{
    private readonly TestDbContext _context;
    private readonly Mock<IDataSeeder<TestEntity>> _dataSeederMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly TestRelationalSeeder _seeder;

    public RelationalSeederTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _dataSeederMock = new Mock<IDataSeeder<TestEntity>>();
        _loggerMock = new Mock<ILogger>();
        _seeder = new TestRelationalSeeder(_context, _dataSeederMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SeedAsync_WithCleanData_CleansExistingDataBeforeSeeding()
    {
        // Arrange
        var existingEntity = new TestEntity { Id = 1, Name = "Existing" };
        await _context.TestEntities.AddAsync(existingEntity);
        await _context.SaveChangesAsync();

        var newEntities = new[] { new TestEntity { Id = 2, Name = "New" } };
        _dataSeederMock.Setup(x => x.ShouldCleanExistingData).Returns(true);
        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(newEntities);

        // Act
        await _seeder.SeedAsync();

        // Assert
        var entities = await _context.TestEntities.ToListAsync();
        Assert.Single(entities);
        Assert.Equal("New", entities[0].Name);
    }

    [Fact]
    public async Task SeedAsync_WithoutCleanData_AddsToExistingData()
    {
        // Arrange
        var existingEntity = new TestEntity { Id = 1, Name = "Existing" };
        await _context.TestEntities.AddAsync(existingEntity);
        await _context.SaveChangesAsync();

        var newEntities = new[] { new TestEntity { Id = 2, Name = "New" } };
        _dataSeederMock.Setup(x => x.ShouldCleanExistingData).Returns(false);
        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(newEntities);

        // Act
        await _seeder.SeedAsync();

        // Assert
        var entities = await _context.TestEntities.ToListAsync();
        Assert.Equal(2, entities.Count);
        Assert.Contains(entities, e => e.Name == "Existing");
        Assert.Contains(entities, e => e.Name == "New");
    }

    [Fact]
    public async Task SeedAsync_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        _dataSeederMock.Setup(x => x.GetSeedDataAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _seeder.SeedAsync());
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}