using System.Text.Json;

namespace DatabaseSeeder.Json.Providers;

/// <summary>
/// Configuration options for the JSON data provider
/// </summary>
public class JsonDataProviderOptions
{
    /// <summary>
    /// Gets or sets the base directory for JSON files
    /// </summary>
    public string? BaseDirectory { get; set; }

    /// <summary>
    /// Gets or sets the JSON serialization options
    /// </summary>
    public JsonSerializerOptions? SerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets whether to clean existing data before seeding
    /// </summary>
    public bool CleanExistingData { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate JSON schema before seeding
    /// </summary>
    public bool ValidateSchema { get; set; } = true;

    /// <summary>
    /// Gets or sets the file extension for JSON files (default: .json)
    /// </summary>
    public string FileExtension { get; set; } = ".json";
}
