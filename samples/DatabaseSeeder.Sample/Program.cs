using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Extensions;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.EntityFramework.Extensions;
using SampleBookstore.Domain;
using SampleBookstore.DataSeeding;
using SampleBookstore.Data;

var builder = Host.CreateApplicationBuilder(args);

// Add services to the container
builder.Services.AddDbContextFactory<BookstoreDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure DatabaseSeeder
var seederBuilder = builder.Services.AddDatabaseSeeder()
    .Configure(options =>
    {
        options.EnableParallelization = false; // Disable parallelization for this example
        options.ThrowOnCircularDependency = true;
        options.ContinueOnError = false;
        options.SeederTimeout = 300; // 5 minutes timeout
    })
    .AddSeeder<CategorySeeder>()
    .AddSeeder<AuthorSeeder>()
    .AddSeeder<BookSeeder>()
    .AddDataSeeder<Category, CategoryDataSeeder>()
    .AddDataSeeder<Author, AuthorDataSeeder>()
    .AddDataSeeder<Book, BookDataSeeder>();

// Build the configuration
seederBuilder.Build();

var host = builder.Build();

// Run the seeding process
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting database seeding");

        var dbContextFactory = services.GetRequiredService<IDbContextFactory<BookstoreDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.MigrateAsync(); // Ensure database is created and up to date

        var orchestrator = services.GetRequiredService<ISeederOrchestrator>();
        await orchestrator.SeedAllAsync();

        logger.LogInformation("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
        throw;
    }
}

await host.RunAsync();