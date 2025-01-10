using DatabaseSeeder.Core.Base;
using DatabaseSeeder.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DatabaseSeeder.Json.Sample.Domain;

namespace DatabaseSeeder.Json.Sample.Data.Seeders;

public class BookSeeder : BaseSeeder
{
    private readonly IDbContextFactory<BookstoreDbContext> _dbContextFactory;
    private readonly IDataSeeder<Book> _dataSeeder;

    public BookSeeder(
        IDbContextFactory<BookstoreDbContext> dbContextFactory,
        IDataSeeder<Book> dataSeeder,
        ILogger<BookSeeder> logger) : base(logger)
    {
        _dbContextFactory = dbContextFactory;
        _dataSeeder = dataSeeder;
    }

    public override int Order => 3;

    public override IEnumerable<Type> Dependencies => new[] { typeof(AuthorSeeder), typeof(CategorySeeder) };

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        LogInformation("Starting to seed books from JSON");

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (_dataSeeder.ShouldCleanExistingData)
        {
            dbContext.Books.RemoveRange(dbContext.Books);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var books = await _dataSeeder.GetSeedDataAsync(cancellationToken);
        await dbContext.Books.AddRangeAsync(books, cancellationToken);

        // Add category relationships based on CategoryIds in JSON
        foreach (var book in books)
        {
            var categories = await dbContext.Categories
                .Where(c => book.CategoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);
            
            foreach (var category in categories)
            {
                book.Categories.Add(category);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        LogInformation("Completed seeding books from JSON");
    }
}