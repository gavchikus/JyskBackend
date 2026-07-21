using System.ComponentModel.DataAnnotations;

namespace JyskBackend.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    [Required, MaxLength(500)] public string DeliveryAddress { get; set; } = string.Empty;
    [MaxLength(500)] public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}