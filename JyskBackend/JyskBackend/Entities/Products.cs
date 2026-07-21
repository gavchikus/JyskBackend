using System.ComponentModel.DataAnnotations;

namespace JyskBackend.Entities;

public class Product
{
    public Guid Id { get; set; }
    [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public int Stock { get; set; }
    [MaxLength(100)] public string? Brand { get; set; }
    [MaxLength(100)] public string? Material { get; set; }
    [MaxLength(50)] public string? Color { get; set; }
    [MaxLength(100)] public string? Dimensions { get; set; }
    [MaxLength(50)] public string? ArticleNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsNew { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<RoomProduct> RoomProducts { get; set; } = new List<RoomProduct>();
    public ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();
}