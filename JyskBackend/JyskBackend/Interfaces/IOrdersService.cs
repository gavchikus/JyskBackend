using JyskBackend.Models.Responses;

namespace JyskBackend.Interfaces;

/// <summary>Чому саме такий результат оформлення — див. <see cref="OrderCreationStatus"/>.</summary>
public record OrderCreationResult(OrderCreationStatus Status, OrderDetailResponse? Order, string? Message);

public enum OrderCreationStatus
{
    Created,
    ProductNotFound,
    OutOfStock,
    EmptyCart
}

public interface IOrdersService
{
    Task<List<OrderListResponse>> GetMyOrdersAsync(Guid customerId);
    Task<OrderDetailResponse?> GetOrderByIdAsync(Guid orderId, Guid customerId, bool isAdmin);
    Task<OrderCreationResult> CreateOrderAsync(Guid customerId, CreateOrderRequest request);
    Task<OrderListResponse?> UpdateStatusAsync(Guid orderId, string status);
    Task<(List<OrderListResponse> Items, int TotalCount)> GetAllOrdersAsync(string? status, int page, int pageSize);
}
