using System.ComponentModel.DataAnnotations;

namespace JyskBackend.Entities;

public class ProductImage
{
    public int Id { get; set; }
    [Required, MaxLength(500)] public string ImageUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int SortOrder { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}