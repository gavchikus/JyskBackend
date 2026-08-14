using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/variants")]
[Produces("application/json")]
public class VariantsController(IVariantsService variantsService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<VariantResponse>), 200)]
    public async Task<IActionResult> GetVariants([FromRoute] Guid productId)
    {
        var variants = await variantsService.GetVariantsByProductIdAsync(productId);
        return Ok(variants.Select(v => new VariantResponse(v.Id, v.Label, v.Color, v.Size, v.Price, v.Stock)).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(VariantResponse), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateVariant([FromRoute] Guid productId, [FromBody] CreateVariantRequest req)
    {
        var variant = new ProductVariant
        {
            Color = req.Color,
            Size = req.Size,
            Label = req.Label,
            Price = req.Price,
            Stock = req.Stock
        };

        var created = await variantsService.CreateVariantAsync(productId, variant);
        return created == null
            ? NotFound(new { message = "Товар не знайдено" })
            : Created(string.Empty, new VariantResponse(created.Id, created.Label, created.Color, created.Size, created.Price, created.Stock));
    }

    [HttpDelete("{variantId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteVariant([FromRoute] Guid productId, [FromRoute] int variantId)
    {
        var success = await variantsService.DeleteVariantAsync(productId, variantId);
        return success ? NoContent() : NotFound();
    }
}
