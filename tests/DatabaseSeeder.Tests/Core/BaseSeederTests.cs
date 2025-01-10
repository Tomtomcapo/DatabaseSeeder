using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Base;

namespace DatabaseSeeder.Tests.Core;

public class BaseSeederTests
{
    private class TestSeeder : BaseSeeder
    {
        public TestSeeder(ILogger logger) : base(logger) { }
        public override int Order => 1;
        public override Task SeedAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private readonly Mock<ILogger> _loggerMock;
    private readonly TestSeeder _seeder;

    public BaseSeederTests()
    {
        _loggerMock = new Mock<ILogger>();
        _seeder = new TestSeeder(_loggerMock.Object);
    }

    [Fact]
    public void Name_ReturnsClassName()
    {
        // Assert
        Assert.Equal("TestSeeder", _seeder.Name);
    }

    [Fact]
    public void Dependencies_ReturnsEmptyByDefault()
    {
        // Assert
        Assert.Empty(_seeder.Dependencies);
    }
}
