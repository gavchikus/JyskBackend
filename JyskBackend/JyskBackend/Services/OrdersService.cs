using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class OrdersService(JyskDbContext context) : IOrdersService
{
    public static readonly string[] AllowedStatuses =
        ["Pending", "Confirmed", "Shipped", "Delivered", "Cancelled"];

    public async Task<List<OrderListResponse>> GetMyOrdersAsync(Guid customerId) =>
        await context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListResponse(
                o.Id, o.Status, o.TotalAmount, o.DeliveryAddress, o.CreatedAt, o.Items.Count, null))
            .ToListAsync();

    public async Task<OrderDetailResponse?> GetOrderByIdAsync(Guid orderId, Guid customerId, bool isAdmin)
    {
        var order = await context.Orders
            .AsSplitQuery()
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        // Чуже замовлення для звичайного покупця не існує — тому 404, а не 403:
        // так не витікає сам факт наявності замовлення з таким id.
        if (order == null || (!isAdmin && order.CustomerId != customerId)) return null;

        return ToDetail(order);
    }

    public async Task<OrderCreationResult> CreateOrderAsync(Guid customerId, CreateOrderRequest request)
    {
        if (request.Items.Count == 0)
            return new OrderCreationResult(OrderCreationStatus.EmptyCart, null, "Кошик порожній");

        // Однакові позиції з фронта складаємо, щоб не списати склад двічі.
        var wanted = request.Items
            .GroupBy(i => new { i.ProductId, i.VariantId })
            .Select(g => new { g.Key.ProductId, g.Key.VariantId, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        var productIds = wanted.Select(w => w.ProductId).Distinct().ToList();
        var products = await context.Products
            .Include(p => p.Variants)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            DeliveryAddress = request.DeliveryAddress,
            Comment = request.Comment,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0;

        foreach (var line in wanted)
        {
            var product = products.FirstOrDefault(p => p.Id == line.ProductId);
            if (product == null || !product.IsActive)
                return new OrderCreationResult(OrderCreationStatus.ProductNotFound, null,
                    $"Товар {line.ProductId} недоступний");

            ProductVariant? variant = null;
            if (line.VariantId.HasValue)
            {
                variant = product.Variants.FirstOrDefault(v => v.Id == line.VariantId.Value);
                if (variant == null)
                    return new OrderCreationResult(OrderCreationStatus.ProductNotFound, null,
                        $"Варіант {line.VariantId} не належить товару «{product.Name}»");
            }

            var available = variant?.Stock ?? product.Stock;
            if (available < line.Quantity)
                return new OrderCreationResult(OrderCreationStatus.OutOfStock, null,
                    $"«{product.Name}»: у наявності лише {available} шт.");

            // Ціну фіксує бекенд — з варіанта або з товару, ніколи з тіла запиту.
            var unitPrice = variant?.Price ?? product.Price;
            total += unitPrice * line.Quantity;

            if (variant != null) variant.Stock -= line.Quantity;
            else product.Stock -= line.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductVariantId = variant?.Id,
                Quantity = line.Quantity,
                UnitPrice = unitPrice
            });
        }

        order.TotalAmount = total;

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var created = await context.Orders
            .AsSplitQuery()
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant)
            .FirstAsync(o => o.Id == order.Id);

        return new OrderCreationResult(OrderCreationStatus.Created, ToDetail(created), null);
    }

    public async Task<OrderListResponse?> UpdateStatusAsync(Guid orderId, string status)
    {
        var order = await context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return null;

        order.Status = status;
        await context.SaveChangesAsync();

        return new OrderListResponse(order.Id, order.Status, order.TotalAmount, order.DeliveryAddress,
            order.CreatedAt, order.Items.Count, null);
    }

    public async Task<(List<OrderListResponse> Items, int TotalCount)> GetAllOrdersAsync(string? status, int page, int pageSize)
    {
        var query = context.Orders.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.Status == status);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListResponse(
                o.Id, o.Status, o.TotalAmount, o.DeliveryAddress, o.CreatedAt, o.Items.Count,
                o.Customer.FirstName + " " + o.Customer.LastName))
            .ToListAsync();

        return (items, totalCount);
    }

    private static OrderDetailResponse ToDetail(Order order) =>
        new(
            order.Id,
            order.Status,
            order.TotalAmount,
            order.DeliveryAddress,
            order.Comment,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemResponse(
                i.ProductId,
                i.Product?.Name ?? "Товар видалено",
                VariantLabel(i.ProductVariant),
                i.Product?.Images.FirstOrDefault(img => img.IsMain)?.ImageUrl
                    ?? i.Product?.Images.FirstOrDefault()?.ImageUrl,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        );

    /// <summary>Підпис варіанта: власна назва, інакше «розмір, колір», інакше нічого.</summary>
    private static string? VariantLabel(ProductVariant? variant)
    {
        if (variant == null) return null;
        if (!string.IsNullOrWhiteSpace(variant.Label)) return variant.Label;

        var parts = new[] { variant.Size, variant.Color }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        return parts.Length == 0 ? null : string.Join(", ", parts);
    }
}
