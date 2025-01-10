using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Base;
using DatabaseSeeder.Core.Abstractions;
using SampleBookstore.Domain;

namespace SampleBookstore.DataSeeding;

public class CategoryDataSeeder : IDataSeeder<Category>
{
    public bool ShouldCleanExistingData => true;

    public Task<IEnumerable<Category>> GetSeedDataAsync(CancellationToken cancellationToken = default)
    {
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Fiction", Description = "Fictional literature" },
            new Category { Id = 2, Name = "Non-Fiction", Description = "Non-fictional literature" },
            new Category { Id = 3, Name = "Science Fiction", Description = "Science fiction literature" },
            new Category { Id = 4, Name = "Mystery", Description = "Mystery and detective fiction" },
            new Category { Id = 5, Name = "Biography", Description = "Biographical works" }
        };

        return Task.FromResult(categories.AsEnumerable());
    }
}