using JyskBackend.Extensions;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using JyskBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Authorize]
public class OrdersController(IOrdersService ordersService) : ControllerBase
{
    /// <summary>Мої замовлення, найновіші зверху.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderListResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyOrders()
    {
        var customerId = User.GetUserId();
        if (customerId == null) return Unauthorized();

        return Ok(await ordersService.GetMyOrdersAsync(customerId.Value));
    }

    /// <summary>Усі замовлення магазину з фільтром за статусом. Лише для адміністратора.</summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedList<OrderListResponse>), 200)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch { < 1 => 20, > 100 => 100, _ => pageSize };

        var (items, totalCount) = await ordersService.GetAllOrdersAsync(status, page, pageSize);
        return Ok(PaginatedList<OrderListResponse>.From(items, totalCount, page, pageSize));
    }

    /// <summary>Деталі замовлення. Покупець бачить лише власні, адміністратор — будь-яке.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid id)
    {
        var customerId = User.GetUserId();
        if (customerId == null) return Unauthorized();

        var order = await ordersService.GetOrderByIdAsync(id, customerId.Value, User.IsAdmin());
        return order == null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// Оформити замовлення. Ціни беруться з бази (варіант або товар),
    /// залишки на складі списуються в межах однієї транзакції SaveChanges.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDetailResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
    {
        var customerId = User.GetUserId();
        if (customerId == null) return Unauthorized();

        var result = await ordersService.CreateOrderAsync(customerId.Value, req);

        return result.Status switch
        {
            OrderCreationStatus.Created =>
                CreatedAtAction(nameof(GetOrderById), new { id = result.Order!.Id }, result.Order),
            OrderCreationStatus.OutOfStock => Conflict(new { message = result.Message }),
            OrderCreationStatus.ProductNotFound => BadRequest(new { message = result.Message }),
            _ => BadRequest(new { message = result.Message ?? "Не вдалося оформити замовлення" })
        };
    }

    /// <summary>Змінити статус замовлення. Лише для адміністратора.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrderListResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusRequest req)
    {
        if (!OrdersService.AllowedStatuses.Contains(req.Status))
            return BadRequest(new { message = $"Допустимі статуси: {string.Join(", ", OrdersService.AllowedStatuses)}" });

        var updated = await ordersService.UpdateStatusAsync(id, req.Status);
        return updated == null ? NotFound() : Ok(updated);
    }
}
