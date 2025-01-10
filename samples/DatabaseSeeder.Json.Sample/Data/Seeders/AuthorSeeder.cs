using DatabaseSeeder.Core.Base;
using DatabaseSeeder.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DatabaseSeeder.Json.Sample.Domain;

namespace DatabaseSeeder.Json.Sample.Data.Seeders;

public class AuthorSeeder : BaseSeeder
{
    private readonly IDbContextFactory<BookstoreDbContext> _dbContextFactory;
    private readonly IDataSeeder<Author> _dataSeeder;

    public AuthorSeeder(
        IDbContextFactory<BookstoreDbContext> dbContextFactory,
        IDataSeeder<Author> dataSeeder,
        ILogger<AuthorSeeder> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _dataSeeder = dataSeeder;
    }

    public override int Order => 2;

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInformation("Starting to seed authors from JSON");

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            
        if (_dataSeeder.ShouldCleanExistingData)
        {
            dbContext.Authors.RemoveRange(dbContext.Authors);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var authors = await _dataSeeder.GetSeedDataAsync(cancellationToken);
        await dbContext.Authors.AddRangeAsync(authors, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        LogInformation("Completed seeding authors from JSON");
    }
}