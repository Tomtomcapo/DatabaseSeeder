using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DatabaseSeeder.Json.Sample.Domain;

public class Book
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public int AuthorId { get; set; }
    
    [JsonIgnore]
    public virtual Author? Author { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public DateTime PublishedDate { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    // Added for JSON deserialization
    [JsonPropertyName("categoryIds")]
    public List<int> CategoryIds { get; set; } = new List<int>();
}