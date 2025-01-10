using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using DatabaseSeeder.Core.Abstractions;

namespace DatabaseSeeder.Json.Providers;

/// <summary>
/// Provides data seeding capabilities from JSON files
/// </summary>
/// <typeparam name="TEntity">The type of entity to seed</typeparam>
public class JsonDataProvider<TEntity> : IDataSeeder<TEntity> where TEntity : class
{
    private readonly string _jsonFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<JsonDataProvider<TEntity>> _logger;
    private readonly bool _shouldCleanExisting;

    /// <summary>
    /// Initializes a new instance of the JsonDataProvider
    /// </summary>
    /// <param name="jsonFilePath">Path to the JSON file containing seed data</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="shouldCleanExisting">Whether to clean existing data before seeding</param>
    /// <param name="jsonOptions">Optional JSON serialization options</param>
    public JsonDataProvider(
        string jsonFilePath,
        ILogger<JsonDataProvider<TEntity>> logger,
        bool shouldCleanExisting = true,
        JsonSerializerOptions? jsonOptions = null)
    {
        _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _shouldCleanExisting = shouldCleanExisting;
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }

    /// <inheritdoc />
    public bool ShouldCleanExistingData => _shouldCleanExisting;

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetSeedDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reading seed data from JSON file: {FilePath}", _jsonFilePath);
            
            if (!File.Exists(_jsonFilePath))
            {
                throw new FileNotFoundException($"JSON seed file not found: {_jsonFilePath}");
            }

            using var fileStream = File.OpenRead(_jsonFilePath);
            var entities = await JsonSerializer.DeserializeAsync<List<TEntity>>(
                fileStream,
                _jsonOptions,
                cancellationToken);

            if (entities == null || !entities.Any())
            {
                _logger.LogWarning("No entities found in JSON file: {FilePath}", _jsonFilePath);
                return Enumerable.Empty<TEntity>();
            }

            _logger.LogInformation("Successfully loaded {Count} entities from JSON file", entities.Count);
            return entities;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing JSON data from file: {FilePath}", _jsonFilePath);
            throw;
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            _logger.LogError(ex, "Unexpected error reading JSON file: {FilePath}", _jsonFilePath);
            throw;
        }
    }
}