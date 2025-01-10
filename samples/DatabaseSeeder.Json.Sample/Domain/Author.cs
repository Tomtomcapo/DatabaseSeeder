using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DatabaseSeeder.Json.Sample.Domain;

public class Author
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Biography { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}