using DatabaseSeeder.EntityFramework;
using DatabaseSeeder.EntityFramework.Tests;

namespace DatabaseSeeder.EntityFramework.Tests;

public class EntityFrameworkSeederOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveExpectedValues()
    {
        // Arrange & Act
        var options = new EntityFrameworkSeederOptions<TestEntity>();

        // Assert
        Assert.Equal(1, options.Order);
        Assert.True(options.UseBatching);
        Assert.Equal(1000, options.BatchSize);
        Assert.True(options.UseTransactions);
        Assert.True(options.ValidateEntities);
        Assert.Null(options.ValidationFunction);
    }

    [Fact]
    public void Options_CanBeCustomized()
    {
        // Arrange
        var options = new EntityFrameworkSeederOptions<TestEntity>
        {
            Order = 42,
            UseBatching = false,
            BatchSize = 500,
            UseTransactions = false,
            ValidateEntities = false,
            ValidationFunction = entity => true
        };

        // Assert
        Assert.Equal(42, options.Order);
        Assert.False(options.UseBatching);
        Assert.Equal(500, options.BatchSize);
        Assert.False(options.UseTransactions);
        Assert.False(options.ValidateEntities);
        Assert.NotNull(options.ValidationFunction);
    }
}