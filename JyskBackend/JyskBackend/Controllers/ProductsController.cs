using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController(IProductsService productsService) : ControllerBase
{
    /// <summary>Список товарів із фільтрами, сортуванням та пагінацією.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<ProductShortResponse>), 200)]
    public async Task<IActionResult> GetAllProducts([FromQuery] ProductQuery query)
    {
        var (items, totalCount) = await productsService.GetAllProductsAsync(query);
        return Ok(PaginatedList<ProductShortResponse>.From(items, totalCount, query.Page, query.PageSize));
    }

    /// <summary>Детальна картка товару разом із фото, варіантами та схожими товарами.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProductById([FromRoute] Guid id)
    {
        var response = await productsService.GetProductDetailAsync(id);
        return response == null ? NotFound() : Ok(response);
    }

    /// <summary>Товари для головної сторінки: новинки, акції та рекомендації.</summary>
    [HttpGet("homepage")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HomePageProductsResponse), 200)]
    public async Task<IActionResult> GetHomePageProducts() =>
        Ok(await productsService.GetHomePageProductsAsync());

    /// <summary>Створити товар. Лише для адміністратора.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Product), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest req)
    {
        var created = await productsService.CreateProductAsync(ToEntity(req));
        return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
    }

    /// <summary>Оновити товар. Лише для адміністратора.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] CreateProductRequest req)
    {
        var updated = await productsService.UpdateProductAsync(id, ToEntity(req));
        return updated == null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Видалити товар. Якщо товар уже фігурує у замовленнях, він не видаляється,
    /// а знімається з продажу — інакше каскад знищив би історію покупок.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
    {
        var result = await productsService.DeleteProductAsync(id);
        return result switch
        {
            DeletionResult.Deleted => NoContent(),
            DeletionResult.NotFound => NotFound(),
            _ => Conflict(new { message = "Товар присутній у замовленнях, тому знятий з продажу замість видалення" })
        };
    }

    /// <summary>Додати фото до товару. Лише для адміністратора.</summary>
    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductImageResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddProductImage([FromRoute] Guid id, [FromBody] AddImageRequest req)
    {
        var image = await productsService.AddProductImageAsync(id, req.ImageUrl, req.IsMain);
        return image == null
            ? NotFound()
            : Ok(new ProductImageResponse(image.Id, image.ImageUrl, image.IsMain));
    }

    private static Product ToEntity(CreateProductRequest req) => new()
    {
        Name = req.Name,
        Description = req.Description,
        Price = req.Price,
        OldPrice = req.OldPrice,
        Stock = req.Stock,
        Brand = req.Brand,
        Material = req.Material,
        Color = req.Color,
        Dimensions = req.Dimensions,
        ArticleNumber = req.ArticleNumber,
        IsNew = req.IsNew,
        IsActive = req.IsActive,
        CategoryId = req.CategoryId
    };
}
