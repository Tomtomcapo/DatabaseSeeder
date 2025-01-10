using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Core.Implementation;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.Exceptions;
using System.Collections.Concurrent;

namespace DatabaseSeeder.Tests.Core;

public class SeederOrchestratorTests
{
    private readonly Mock<ILogger<SeederOrchestrator>> _loggerMock;
    private readonly Mock<ISeederRegistry> _registryMock;
    private readonly SeederOptions _options;
    private readonly SeederOrchestrator _orchestrator;

    public SeederOrchestratorTests()
    {
        _loggerMock = new Mock<ILogger<SeederOrchestrator>>();
        _registryMock = new Mock<ISeederRegistry>();
        _options = new SeederOptions();
        _orchestrator = new SeederOrchestrator(_registryMock.Object, _loggerMock.Object, _options);
    }

    [Fact]
    public async Task SeedAllAsync_ExecutesSeedersInCorrectOrder()
    {
        // Arrange
        var seeder1 = new TestSeeder(1, "Seeder1");
        var seeder2 = new TestSeeder2(2, "Seeder2");
        var seeder3 = new TestSeeder3(1, "Seeder3");

        _registryMock.Setup(x => x.GetAllSeeders())
            .Returns(new[] { seeder2, seeder1, seeder3 });

        var executionOrder = new List<string>();
        seeder1.ExecutionCallback = () => executionOrder.Add("Seeder1");
        seeder2.ExecutionCallback = () => executionOrder.Add("Seeder2");
        seeder3.ExecutionCallback = () => executionOrder.Add("Seeder3");

        // Act
        await _orchestrator.SeedAllAsync();

        // Assert
        Assert.Equal(3, executionOrder.Count);
        Assert.Contains("Seeder1", executionOrder.Take(2));
        Assert.Contains("Seeder3", executionOrder.Take(2));
        Assert.Equal("Seeder2", executionOrder[2]);
    }

    [Fact]
    public async Task SeedAllAsync_WithParallelization_ExecutesInParallel()
    {
        // Arrange
        _options.EnableParallelization = true;
        _options.MaxDegreeOfParallelization = 2;

        var seeder1 = new TestSeeder(1, "Seeder1");
        var seeder2 = new TestSeederP2(1, "Seeder2");
        var seeders = new ISeeder[] { seeder1, seeder2 };
        
        var startedCount = 0;
        var seederStartTcs = new TaskCompletionSource<bool>();
        var continueExecutionTcs = new TaskCompletionSource<bool>();
        var seederStates = new ConcurrentDictionary<string, bool>();

        _registryMock.Setup(x => x.GetAllSeeders())
            .Returns(() => seeders);

        seeder1.AsyncExecutionCallback = async () =>
        {
            seederStates.TryAdd("Seeder1", true);
            var count = Interlocked.Increment(ref startedCount);
            if (count == 2) seederStartTcs.TrySetResult(true);
            await continueExecutionTcs.Task;
        };

        seeder2.AsyncExecutionCallback = async () =>
        {
            seederStates.TryAdd("Seeder2", true);
            var count = Interlocked.Increment(ref startedCount);
            if (count == 2) seederStartTcs.TrySetResult(true);
            await continueExecutionTcs.Task;
        };

        // Act
        var seedTask = _orchestrator.SeedAllAsync();
        
        var bothStarted = await Task.WhenAny(
            seederStartTcs.Task,
            Task.Delay(TimeSpan.FromSeconds(5))
        ) == seederStartTcs.Task;

        // Assert
        Assert.True(bothStarted, 
            $"Both seeders should have started. Current count: {startedCount}. " +
            $"Started seeders: {string.Join(", ", seederStates.Keys)}");
        Assert.Equal(2, startedCount);

        // Cleanup
        continueExecutionTcs.SetResult(true);
        await seedTask;
    }

    [Fact]
    public async Task SeedAllAsync_WithTimeout_ThrowsException()
    {
        // Arrange
        _options.SeederTimeout = 1;
        var seeder = new TestSeeder(1, "Seeder1");

        seeder.AsyncExecutionCallback = async () =>
        {
            await Task.Delay(-1); // Never complete
        };

        _registryMock.Setup(x => x.GetAllSeeders())
            .Returns(new[] { seeder });

        // Act & Assert
        await Assert.ThrowsAsync<SeederException>(() => _orchestrator.SeedAllAsync());
    }

    [Fact]
    public async Task SeedAsync_WithSpecificTypes_OnlyExecutesSpecifiedSeeders()
    {
        // Arrange
        var seeder1 = new TestSeeder(1, "Seeder1");
        var seeder2 = new TestSeeder(1, "Seeder2");
        var executed1 = false;
        var executed2 = false;

        seeder1.SyncExecutionCallback = () => executed1 = true;
        seeder2.SyncExecutionCallback = () => executed2 = true;

        _registryMock.Setup(x => x.GetAllSeeders())
            .Returns(new[] { seeder1, seeder2 });

        // Act
        await _orchestrator.SeedAsync(new[] { seeder1.GetType() });

        // Assert
        Assert.True(executed1);
        Assert.False(executed2);
    }

    private class TestSeeder : ISeeder
    {
        public TestSeeder(int order, string name)
        {
            Order = order;
            Name = name;
        }

        public int Order { get; }
        public string Name { get; }
        public IEnumerable<Type> Dependencies => Array.Empty<Type>();
        public Action ExecutionCallback { get; set; }
        public Action SyncExecutionCallback { get; set; }
        public Func<Task> AsyncExecutionCallback { get; set; }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (AsyncExecutionCallback != null)
                {
                    await AsyncExecutionCallback();
                }
                else if (SyncExecutionCallback != null)
                {
                    await Task.Run(SyncExecutionCallback, cancellationToken);
                }
                else if (ExecutionCallback != null)
                {
                    ExecutionCallback();
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }

    private class TestSeeder2 : TestSeeder
    {
        public TestSeeder2(int order, string name) : base(order, name) { }
    }

    private class TestSeeder3 : TestSeeder
    {
        public TestSeeder3(int order, string name) : base(order, name) { }
    }

    private class TestSeederP2 : TestSeeder
    {
        public TestSeederP2(int order, string name) : base(order, name) { }
    }
}