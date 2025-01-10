using DatabaseSeeder.Core.Abstractions;
using SampleBookstore.Domain;

public class AuthorDataSeeder : IDataSeeder<Author>
{
    public bool ShouldCleanExistingData => true;

    public Task<IEnumerable<Author>> GetSeedDataAsync(CancellationToken cancellationToken = default)
    {
        var authors = new List<Author>
        {
            new Author 
            { 
                Id = 1, 
                Name = "George Orwell", 
                Biography = "English novelist and essayist",
                DateOfBirth = new DateTime(1903, 6, 25)
            },
            new Author 
            { 
                Id = 2, 
                Name = "J.R.R. Tolkien",
                Biography = "English writer and philologist",
                DateOfBirth = new DateTime(1892, 1, 3)
            },
            new Author 
            { 
                Id = 3, 
                Name = "Isaac Asimov",
                Biography = "American writer and professor",
                DateOfBirth = new DateTime(1920, 1, 2)
            }
        };

        return Task.FromResult(authors.AsEnumerable());
    }
}