using DatabaseSeeder.Core.Abstractions;
using SampleBookstore.Domain;

public class BookDataSeeder : IDataSeeder<Book>
{
    public bool ShouldCleanExistingData => true;

    public Task<IEnumerable<Book>> GetSeedDataAsync(CancellationToken cancellationToken = default)
    {
        var books = new List<Book>
        {
            new Book 
            { 
                Id = 1,
                Title = "1984",
                AuthorId = 1,
                Price = 19.99m,
                PublishedDate = new DateTime(1949, 6, 8),
                Description = "A dystopian social science fiction novel"
            },
            new Book 
            { 
                Id = 2,
                Title = "The Hobbit",
                AuthorId = 2,
                Price = 24.99m,
                PublishedDate = new DateTime(1937, 9, 21),
                Description = "A fantasy novel set in Middle-earth"
            },
            new Book 
            { 
                Id = 3,
                Title = "Foundation",
                AuthorId = 3,
                Price = 21.99m,
                PublishedDate = new DateTime(1951, 5, 1),
                Description = "The first novel in the Foundation series"
            }
        };

        return Task.FromResult(books.AsEnumerable());
    }
}