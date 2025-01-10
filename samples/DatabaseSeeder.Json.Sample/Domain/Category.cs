using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DatabaseSeeder.Json.Sample.Domain;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}