using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

/// <summary>
/// Спільна база для «кімнат» і «колекцій»: обидва ресурси — це підбірка товарів
/// з назвою та обкладинкою, різниця лише в URL і сенсі для покупця.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ShowcaseControllerBase(IShowcaseService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ShowcaseShortResponse>), 200)]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ShowcaseDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var item = await service.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ShowcaseShortResponse), 201)]
    public async Task<IActionResult> Create([FromBody] CreateShowcaseRequest req)
    {
        var created = await service.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:int}/products")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddProduct([FromRoute] int id, [FromBody] AddShowcaseProductRequest req)
    {
        var ok = await service.AddProductAsync(id, req.ProductId);
        return ok ? Ok(new { message = "Товар додано" }) : NotFound();
    }

    [HttpDelete("{id:int}/products/{productId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveProduct([FromRoute] int id, [FromRoute] Guid productId)
    {
        var ok = await service.RemoveProductAsync(id, productId);
        return ok ? NoContent() : NotFound();
    }
}

/// <summary>Інспіраційні розділи: «Спальня», «Вітальня», «Балкон».</summary>
[Route("api/rooms")]
public class RoomsController(IRoomsService service) : ShowcaseControllerBase(service);

/// <summary>Добірки за стилем: «Скандинавський», «Лофт», «Мінімалізм».</summary>
[Route("api/collections")]
public class CollectionsController(ICollectionsService service) : ShowcaseControllerBase(service);
