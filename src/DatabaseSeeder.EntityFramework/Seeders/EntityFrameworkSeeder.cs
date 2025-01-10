using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Base;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Core.Extensions;
using DatabaseSeeder.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace DatabaseSeeder.EntityFramework.Seeders;

/// <summary>
/// A seeder implementation for Entity Framework that supports batching and transactions
/// </summary>
public class EntityFrameworkSeeder<TContext, TEntity> : RelationalSeeder<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    private readonly EntityFrameworkSeederOptions<TEntity> _options;
    private readonly ILogger<EntityFrameworkSeeder<TContext, TEntity>> _logger;
    private readonly HashSet<TEntity> _validationFailures;

    public override int Order => _options.Order;

    public EntityFrameworkSeeder(
        TContext dbContext,
        IDataSeeder<TEntity> dataSeeder,
        EntityFrameworkSeederOptions<TEntity> options,
        ILogger<EntityFrameworkSeeder<TContext, TEntity>> logger)
        : base(dbContext, dataSeeder, logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validationFailures = new HashSet<TEntity>();
    }

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_options.UseTransactions)
            {
                await this.WithTransactionAsync(_dbContext, cancellationToken);
                return;
            }

            await base.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed {EntityType}", typeof(TEntity).Name);
            throw new SeederException($"Failed to seed {typeof(TEntity).Name}", ex);
        }
    }

    protected override async Task SeedEntitiesAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        var validatedEntities = await ValidateEntitiesAsync(entities, cancellationToken);

        if (_options.UseBatching)
        {
            await SeedEntitiesInBatchesAsync(validatedEntities, cancellationToken);
            return;
        }

        await base.SeedEntitiesAsync(validatedEntities, cancellationToken);
    }

    private async Task<IEnumerable<TEntity>> ValidateEntitiesAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        if (!_options.ValidateEntities)
        {
            return entities;
        }

        _validationFailures.Clear();
        var validEntities = new List<TEntity>();

        foreach (var entity in entities)
        {
            if (await ValidateEntityAsync(entity, cancellationToken))
            {
                validEntities.Add(entity);
            }
            else
            {
                _validationFailures.Add(entity);
            }
        }

        if (_validationFailures.Any())
        {
            _logger.LogWarning(
                "Validation failed for {FailureCount} entities of type {EntityType}",
                _validationFailures.Count,
                typeof(TEntity).Name);
        }

        return validEntities;
    }

    private async Task<bool> ValidateEntityAsync(TEntity entity, CancellationToken cancellationToken)
    {
        try
        {
            if (_options.ValidationFunction != null)
            {
                return _options.ValidationFunction(entity);
            }

            // Default validation using EF Core's validation
            var validationContext = new ValidationContext(entity);
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(entity, validationContext, validationResults, validateAllProperties: true))
            {
                foreach (var validationResult in validationResults)
                {
                    _logger.LogWarning(
                        "Validation failed for entity {EntityType}: {ValidationMessage}",
                        typeof(TEntity).Name,
                        validationResult.ErrorMessage);
                }
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error validating entity of type {EntityType}",
                typeof(TEntity).Name);
            return false;
        }
    }

    private async Task SeedEntitiesInBatchesAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
    {
        await entities.ExecuteInBatchesAsync(
            _options.BatchSize,
            async batch =>
            {
                await _dbContext.Set<TEntity>().AddRangeAsync(batch, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            });
    }
}
