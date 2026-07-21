using System.ComponentModel.DataAnnotations;

namespace JyskBackend.Entities;

public class Room
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(255)] public string? CoverImageUrl { get; set; }
    public ICollection<RoomProduct> RoomProducts { get; set; } = new List<RoomProduct>();
}

public class RoomProduct
{
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

public class Collection
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(255)] public string? CoverImageUrl { get; set; }
    public ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();
}

public class CollectionProduct
{
    public int CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}