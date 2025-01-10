using DatabaseSeeder.Core.Base;
using DatabaseSeeder.Core.Abstractions;
using Microsoft.Extensions.Logging;
using SampleBookstore.Data;
using SampleBookstore.Domain;
using Microsoft.EntityFrameworkCore;

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
        LogInformation("Starting to seed books");

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (_dataSeeder.ShouldCleanExistingData)
        {
            dbContext.Books.RemoveRange(dbContext.Books);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var books = await _dataSeeder.GetSeedDataAsync(cancellationToken);
        await dbContext.Books.AddRangeAsync(books, cancellationToken);

        // Add category relationships
        var book1984 = books.First(b => b.Title == "1984");
        var theHobbit = books.First(b => b.Title == "The Hobbit");
        var foundation = books.First(b => b.Title == "Foundation");

        var fiction = await dbContext.Categories.FindAsync(new object[] { 1 }, cancellationToken);
        var sciFi = await dbContext.Categories.FindAsync(new object[] { 3 }, cancellationToken);

        if (fiction != null)
        {
            book1984.Categories.Add(fiction);
            theHobbit.Categories.Add(fiction);
        }

        if (sciFi != null)
        {
            book1984.Categories.Add(sciFi);
            foundation.Categories.Add(sciFi);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        LogInformation("Completed seeding books");
    }
}