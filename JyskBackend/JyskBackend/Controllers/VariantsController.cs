using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/variants")]
public class VariantsController(IVariantsService variantsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetVariants([FromRoute] Guid productId)
    {
        var variants = await variantsService.GetVariantsByProductIdAsync(productId);
        var response = variants.Select(v => new VariantResponse(v.Id, v.Label, v.Color, v.Size, v.Price, v.Stock)).ToList();
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVariant([FromRoute] Guid productId, [FromBody] CreateVariantRequest req)
    {
        var variant = new ProductVariant { Color = req.Color, Size = req.Size, Label = req.Label, Price = req.Price, Stock = req.Stock };
        var created = await variantsService.CreateVariantAsync(productId, variant);
        if (created == null) return NotFound("Product not found");
        return Created("", new VariantResponse(created.Id, created.Label, created.Color, created.Size, created.Price, created.Stock));
    }

    [HttpDelete("{variantId:int}")]
    public async Task<IActionResult> DeleteVariant([FromRoute] Guid productId, [FromRoute] int variantId)
    {
        var success = await variantsService.DeleteVariantAsync(productId, variantId);
        if (!success) return NotFound();
        return NoContent();
    }
}