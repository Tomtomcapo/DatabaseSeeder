using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Abstractions;

namespace DatabaseSeeder.Core.Base;

public abstract class RelationalSeeder<TEntity> : BaseSeeder where TEntity : class
{
    protected readonly DbContext _dbContext;
    protected readonly IDataSeeder<TEntity> _dataSeeder;

    protected RelationalSeeder(
        DbContext dbContext,
        IDataSeeder<TEntity> dataSeeder,
        ILogger logger) : base(logger)
    {
        _dbContext = dbContext;
        _dataSeeder = dataSeeder;
    }

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInformation($"Starting to seed {typeof(TEntity).Name}");

        try
        {
            if (_dataSeeder.ShouldCleanExistingData)
            {
                await CleanExistingDataAsync(cancellationToken);
            }

            var entities = await _dataSeeder.GetSeedDataAsync(cancellationToken);
            await SeedEntitiesAsync(entities, cancellationToken);

            LogInformation($"Successfully seeded {typeof(TEntity).Name}");
        }
        catch (Exception ex)
        {
            LogError($"Failed to seed {typeof(TEntity).Name}", ex);
            throw;
        }
    }

    protected virtual async Task CleanExistingDataAsync(CancellationToken cancellationToken)
    {
        _dbContext.Set<TEntity>().RemoveRange(_dbContext.Set<TEntity>());
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    protected virtual async Task SeedEntitiesAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}