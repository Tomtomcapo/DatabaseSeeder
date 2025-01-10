using Microsoft.Extensions.DependencyInjection;
using DatabaseSeeder.Core.Configuration;
using DatabaseSeeder.Core.Abstractions;
using DatabaseSeeder.Json.Providers;
using Microsoft.Extensions.Logging;

namespace DatabaseSeeder.Json.Extensions;

/// <summary>
/// Extension methods for adding JSON seeding support
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Adds JSON data provider support to the DatabaseSeeder
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to seed</typeparam>
    /// <param name="builder">The seeder builder instance</param>
    /// <param name="jsonFilePath">Path to the JSON file containing seed data</param>
    /// <param name="configureOptions">Optional configuration for the JSON provider</param>
    /// <returns>The seeder builder for chaining</returns>
    public static SeederBuilder AddJsonDataProvider<TEntity>(
        this SeederBuilder builder,
        string jsonFilePath,
        Action<JsonDataProviderOptions>? configureOptions = null) 
        where TEntity : class
    {
        var options = new JsonDataProviderOptions();
        configureOptions?.Invoke(options);

        // If base directory is specified, combine it with the file path
        var fullPath = options.BaseDirectory != null
            ? Path.Combine(options.BaseDirectory, jsonFilePath)
            : jsonFilePath;

        // Add file extension if not present
        if (!fullPath.EndsWith(options.FileExtension, StringComparison.OrdinalIgnoreCase))
        {
            fullPath = fullPath + options.FileExtension;
        }

        // Register the JSON provider
        builder.Services.AddScoped<IDataSeeder<TEntity>>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<JsonDataProvider<TEntity>>>();
            return new JsonDataProvider<TEntity>(
                fullPath,
                logger,
                options.CleanExistingData,
                options.SerializerOptions);
        });

        return builder;
    }

    /// <summary>
    /// Adds multiple JSON data providers to the DatabaseSeeder
    /// </summary>
    /// <param name="builder">The seeder builder instance</param>
    /// <param name="baseDirectory">Base directory containing JSON files</param>
    /// <param name="configureOptions">Optional configuration for the JSON providers</param>
    /// <returns>The seeder builder for chaining</returns>
    public static SeederBuilder AddJsonDataProviders(
        this SeederBuilder builder,
        string baseDirectory,
        Action<JsonDataProviderOptions>? configureOptions = null)
    {
        var options = new JsonDataProviderOptions { BaseDirectory = baseDirectory };
        configureOptions?.Invoke(options);

        var jsonFiles = Directory.GetFiles(
            baseDirectory,
            $"*{options.FileExtension}",
            SearchOption.AllDirectories);

        foreach (var jsonFile in jsonFiles)
        {
            // Register the JSON provider
            throw new NotImplementedException("Implementation needs to determine entity type from JSON file");
        }

        return builder;
    }
}