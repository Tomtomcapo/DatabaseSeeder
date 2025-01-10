using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SampleBookstore.Domain;

public class Book
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public int AuthorId { get; set; }
    
    public virtual Author? Author { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public DateTime PublishedDate { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}