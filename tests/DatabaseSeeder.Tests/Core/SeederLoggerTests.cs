using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Utilities;

namespace DatabaseSeeder.Tests.Core;

public class SeederLoggerTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly SeederLogger _seederLogger;
    private const string SeederName = "TestSeeder";

    public SeederLoggerTests()
    {
        _loggerMock = new Mock<ILogger>();
        _seederLogger = new SeederLogger(_loggerMock.Object, SeederName);
    }

    [Fact]
    public void LogInformation_IncludesSeederName()
    {
        // Arrange
        const string message = "Test message";

        // Act
        _seederLogger.LogInformation(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(SeederName) && v.ToString().Contains(message)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogWarning_IncludesSeederName()
    {
        // Arrange
        const string message = "Test warning";

        // Act
        _seederLogger.LogWarning(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(SeederName) && v.ToString().Contains(message)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_WithException_IncludesSeederNameAndException()
    {
        // Arrange
        const string message = "Test error";
        var exception = new Exception("Test exception");

        // Act
        _seederLogger.LogError(message, exception);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(SeederName) && v.ToString().Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void BeginScope_CreatesScopeWithSeederName()
    {
        // Arrange
        const string scopeName = "TestScope";
        var scopeProvider = new Mock<IExternalScopeProvider>();
        _loggerMock.Setup(x => x.BeginScope(It.IsAny<string>())).Returns(new Mock<IDisposable>().Object);

        // Act
        using var scope = _seederLogger.BeginScope(scopeName);

        // Assert
        _loggerMock.Verify(x => x.BeginScope($"{SeederName}:{scopeName}"), Times.Once);
    }
}
