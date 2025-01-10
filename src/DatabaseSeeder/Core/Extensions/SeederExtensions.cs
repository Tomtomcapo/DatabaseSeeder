using Microsoft.EntityFrameworkCore;
using DatabaseSeeder.Core.Abstractions;

namespace DatabaseSeeder.Core.Extensions;

public static class SeederExtensions
{
    public static async Task WithTransactionAsync(
        this ISeeder seeder,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Database.CurrentTransaction != null)
        {
            await seeder.SeedAsync(cancellationToken);
            return;
        }

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await seeder.SeedAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public static async Task ExecuteInBatchesAsync<T>(
        this IEnumerable<T> source,
        int batchSize,
        Func<IEnumerable<T>, Task> action)
    {
        var batch = new List<T>(batchSize);
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == batchSize)
            {
                await action(batch);
                batch.Clear();
            }
        }

        if (batch.Any())
        {
            await action(batch);
        }
    }
}
