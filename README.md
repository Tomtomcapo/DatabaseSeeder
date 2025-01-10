# DatabaseSeeder

A powerful, flexible, and dependency-aware database seeding framework for .NET applications.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Built for .NET 8](https://img.shields.io/badge/.NET-8-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)

## What is Database Seeding?

Database seeding is the process of populating a database with initial data. This could be:
- Test data for development
- Default configuration values
- Required lookup tables
- Sample data for demonstrations
- Initial user accounts and roles
- And more!

While seeding might sound simple, it can become complex when dealing with:
- Dependencies between different types of data
- Proper ordering of data insertion
- Handling relationships between entities
- Managing large datasets
- Maintaining data consistency
- Different environments (development, testing, production)

This is where DatabaseSeeder comes in!

## Features

🚀 **Core Features**
- Dependency-aware seeding with automatic ordering
- Support for parallel seeding execution
- Transaction management
- Batched data operations for large datasets
- Flexible data validation
- Comprehensive logging

🛠️ **Technical Features**
- Entity Framework Core integration
- JSON data source support
- Custom data provider extensibility
- Configurable execution options
- Dependency injection support
- Asynchronous operations

🔧 **Developer Experience**
- Fluent configuration API
- Clear separation of concerns
- Easy to extend and customize
- Comprehensive testing support
- Rich logging and diagnostics

## Quick Start

### 1. Install the NuGet Package

```bash
dotnet add package DatabaseSeeder
dotnet add package DatabaseSeeder.EntityFramework  # If using Entity Framework
dotnet add package DatabaseSeeder.Json             # If using JSON data sources
```

### 2. Define Your Seeders

```csharp
public class CategorySeeder : BaseSeeder
{
    private readonly MyDbContext _dbContext;
    private readonly IDataSeeder<Category> _dataSeeder;

    public CategorySeeder(
        MyDbContext dbContext,
        IDataSeeder<Category> dataSeeder,
        ILogger<CategorySeeder> logger) : base(logger)
    {
        _dbContext = dbContext;
        _dataSeeder = dataSeeder;
    }

    public override int Order => 1;  // Lower numbers run first

    public override async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dataSeeder.GetSeedDataAsync(cancellationToken);
        await _dbContext.Categories.AddRangeAsync(categories, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

### 3. Configure Services

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDatabaseSeeder()
        .Configure(options =>
        {
            options.EnableParallelization = true;
            options.MaxDegreeOfParallelization = 4;
        })
        .AddSeeder<CategorySeeder>()
        .AddSeeder<ProductSeeder>()
        .AddDataSeeder<Category, CategoryDataSeeder>()
        .Build();
}
```

### 4. Execute Seeding

```csharp
public async Task SeedDatabase(IServiceProvider services)
{
    var orchestrator = services.GetRequiredService<ISeederOrchestrator>();
    await orchestrator.SeedAllAsync();
}
```

## Key Concepts

### Seeders

Seeders are classes that handle the actual data insertion into your database. They can:
- Define dependencies on other seeders
- Specify execution order
- Handle transaction management
- Implement custom validation logic

### Data Seeders

Data seeders provide the actual data to be seeded. They:
- Can read from various sources (JSON, CSV, hardcoded data, etc.)
- Can implement custom data transformation
- Control whether existing data should be cleaned
- Handle data validation

### Orchestrator

The orchestrator manages the seeding process by:
- Resolving dependencies between seeders
- Managing execution order
- Handling parallel execution
- Managing transactions
- Providing error handling and logging

## Advanced Features

### Dependency Management

DatabaseSeeder automatically handles dependencies between seeders:

```csharp
public class ProductSeeder : BaseSeeder
{
    public override int Order => 2;
    
    public override IEnumerable<Type> Dependencies => 
        new[] { typeof(CategorySeeder) };  // Products depend on Categories
}
```

### Parallel Execution

Enable parallel seeding for better performance:

```csharp
services.AddDatabaseSeeder()
    .Configure(options =>
    {
        options.EnableParallelization = true;
        options.MaxDegreeOfParallelization = 4;
        options.ContinueOnError = false;
    })
    .Build();
```

### Transaction Management

Control transaction behavior:

```csharp
public class EntityFrameworkSeederOptions<TEntity>
{
    public bool UseTransactions { get; set; } = true;
    public bool UseBatching { get; set; } = true;
    public int BatchSize { get; set; } = 1000;
}
```

### JSON Data Source

Easily seed from JSON files:

```csharp
services.AddDatabaseSeeder()
    .AddJsonDataProvider<Category>("categories.json", options =>
    {
        options.BaseDirectory = "SeedData";
        options.ValidateSchema = true;
    })
    .Build();
```

## Best Practices

1. **Order Matters**: Use the `Order` property to control execution sequence. Lower numbers run first.

2. **Clear Dependencies**: Always explicitly declare seeder dependencies using the `Dependencies` property.

3. **Transaction Control**: Use transactions for related data to maintain consistency.

4. **Batch Operations**: Enable batching for large datasets to improve performance.

5. **Validation**: Implement proper validation in your seeders to ensure data integrity.

6. **Error Handling**: Configure appropriate error handling strategies based on your needs.

## Common Patterns

### Clean and Seed Pattern

```csharp
public override async Task SeedAsync(CancellationToken cancellationToken)
{
    if (_dataSeeder.ShouldCleanExistingData)
    {
        await CleanExistingDataAsync(cancellationToken);
    }

    var entities = await _dataSeeder.GetSeedDataAsync(cancellationToken);
    await SeedEntitiesAsync(entities, cancellationToken);
}
```

### Validation Pattern

```csharp
protected virtual async Task<IEnumerable<TEntity>> ValidateEntitiesAsync(
    IEnumerable<TEntity> entities,
    CancellationToken cancellationToken)
{
    var validEntities = new List<TEntity>();
    foreach (var entity in entities)
    {
        if (await ValidateEntityAsync(entity, cancellationToken))
        {
            validEntities.Add(entity);
        }
    }
    return validEntities;
}
```

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 📚 [Documentation](docs/README.md)
- 🐛 [Issue Tracker](https://github.com/Tomtomcapo/DatabaseSeeder/issues)

---

Built with ❤️ by the .NET community