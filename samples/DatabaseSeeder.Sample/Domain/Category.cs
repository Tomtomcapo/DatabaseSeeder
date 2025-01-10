using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SampleBookstore.Domain;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}