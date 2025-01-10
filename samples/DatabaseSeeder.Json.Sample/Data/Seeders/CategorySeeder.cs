using DatabaseSeeder.Core.Base;
using DatabaseSeeder.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DatabaseSeeder.Json.Sample.Domain;

namespace DatabaseSeeder.Json.Sample.Data.Seeders;

public class CategorySeeder : BaseSeeder
{
    private readonly IDbContextFactory<BookstoreDbContext> _dbContextFactory;
    private readonly IDataSeeder<Category> _dataSeeder;

    public CategorySeeder(
        IDbContextFactory<BookstoreDbContext> dbContextFactory,
        IDataSeeder<Category> dataSeeder,
        ILogger<CategorySeeder> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _dataSeeder = dataSeeder;
    }

    public override int Order => 1;

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInformation("Starting to seed categories from JSON");

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (_dataSeeder.ShouldCleanExistingData)
        {
            dbContext.Categories.RemoveRange(dbContext.Categories);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var categories = await _dataSeeder.GetSeedDataAsync(cancellationToken);
        await dbContext.Categories.AddRangeAsync(categories, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        LogInformation("Completed seeding categories from JSON");
    }
}