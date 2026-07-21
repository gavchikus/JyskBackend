using System.ComponentModel.DataAnnotations;

namespace JyskBackend.Entities;

public class Category
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(255)] public string? ImageUrl { get; set; }

    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
}