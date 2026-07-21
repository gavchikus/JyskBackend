using System.ComponentModel.DataAnnotations;

namespace JyskBackend.Entities;

public class ProductVariant
{
    public int Id { get; set; }
    [MaxLength(50)] public string? Color { get; set; }
    [MaxLength(50)] public string? Size { get; set; }
    [MaxLength(100)] public string? Label { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}