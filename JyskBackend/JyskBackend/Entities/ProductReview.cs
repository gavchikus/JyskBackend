using System.ComponentModel.DataAnnotations;

namespace JyskBackend.Entities;

public class ProductReview
{
    public int Id { get; set; }
    public int Rating { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}