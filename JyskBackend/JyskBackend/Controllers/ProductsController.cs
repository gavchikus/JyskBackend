using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using JyskBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController(IProductsService productsService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ProductShortResponse>), 200)]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int? categoryId, [FromQuery] string? search, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
        [FromQuery] string? color, [FromQuery] string? material, [FromQuery] bool? isNew, [FromQuery] bool? onSale,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null)
    {
        var (items, totalCount) = await productsService.GetAllProductsAsync(categoryId, search, minPrice, maxPrice, color, material, isNew, onSale, page, pageSize, sortBy);

        var responseItems = ProductMapper.ToShort(items);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return Ok(new PaginatedList<ProductShortResponse>(responseItems, totalCount, page, pageSize, totalPages));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProductById([FromRoute] Guid id)
    {
        var p = await productsService.GetProductByIdAsync(id);
        if (p == null) return NotFound();

        var relatedEntities = await ((ProductsService)productsService).GetRelatedProductsAsync(p.CategoryId, p.Id, 4);
        var relatedProducts = ProductMapper.ToShort(relatedEntities);

        var response = new ProductDetailResponse(
            p.Id, p.Name, p.Description, p.Price, p.OldPrice, p.Stock, p.ArticleNumber, p.Brand, p.Color, p.Material, p.Dimensions, p.IsNew, p.IsActive, p.CategoryId,
            p.Category?.Name ?? "Unknown",
            p.Images.Select(i => new ProductImageResponse(i.Id, i.ImageUrl, i.IsMain)).ToList(),
            p.Variants.Select(v => new VariantResponse(v.Id, v.Label, v.Color, v.Size, v.Price, v.Stock)).ToList(),
            p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0,
            p.Reviews.Count,
            relatedProducts 
        );

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Product), 201)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest req)
    {
        var product = new Product
        {
            Name = req.Name, Description = req.Description, Price = req.Price, OldPrice = req.OldPrice, Stock = req.Stock,
            Brand = req.Brand, Material = req.Material, Color = req.Color, Dimensions = req.Dimensions, ArticleNumber = req.ArticleNumber,
            IsNew = req.IsNew, CategoryId = req.CategoryId
        };

        var created = await productsService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] CreateProductRequest req)
    {
        var product = new Product
        {
            Name = req.Name, Description = req.Description, Price = req.Price, OldPrice = req.OldPrice, Stock = req.Stock,
            Brand = req.Brand, Material = req.Material, Color = req.Color, Dimensions = req.Dimensions, ArticleNumber = req.ArticleNumber,
            IsNew = req.IsNew, CategoryId = req.CategoryId
        };

        var updated = await productsService.UpdateProductAsync(id, product);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
    {
        var success = await productsService.DeleteProductAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpGet("homepage")]
    [ProducesResponseType(typeof(HomePageProductsResponse), 200)]
    public async Task<IActionResult> GetHomePageProducts()
    {
        var result = await productsService.GetHomePageProductsAsync();
        return Ok(result);
    }

    public record AddImageRequest(string ImageUrl, bool IsMain);

    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddProductImage([FromRoute] Guid id, [FromBody] AddImageRequest req)
    {
        var image = await productsService.AddProductImageAsync(id, req.ImageUrl, req.IsMain);
        if (image == null) return NotFound();

        return Ok(image);
    }
}