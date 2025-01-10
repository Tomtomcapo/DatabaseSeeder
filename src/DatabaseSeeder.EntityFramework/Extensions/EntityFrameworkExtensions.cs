using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.EntityFramework.Seeders;

namespace DatabaseSeeder.EntityFramework.Extensions;

public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Adds Entity Framework seeder support to the DatabaseSeeder
    /// </summary>
    public static SeederBuilder AddEntityFrameworkSeeder<TContext, TEntity>(
        this SeederBuilder builder,
        Action<EntityFrameworkSeederOptions<TEntity>>? configureOptions = null)
        where TContext : DbContext
        where TEntity : class
    {
        var options = new EntityFrameworkSeederOptions<TEntity>();
        configureOptions?.Invoke(options);

        // Register the options
        var services = builder.Services;
        services.AddSingleton(options);

        // Register the Entity Framework seeder
        builder.AddSeeder<EntityFrameworkSeeder<TContext, TEntity>>();

        return builder;
    }
}
