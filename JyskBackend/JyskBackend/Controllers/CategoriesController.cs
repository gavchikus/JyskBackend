using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoriesService categoriesService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var cats = await categoriesService.GetAllCategoriesAsync();
        var response = cats.Select(c => new CategoryShortResponse(c.Id, c.Name, c.Description, c.ImageUrl, c.ParentCategoryId)).ToList();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        var c = await categoriesService.GetCategoryByIdAsync(id);
        if (c == null) return NotFound();

        var response = new CategoryDetailResponse(
            c.Id, c.Name, c.Description, c.ImageUrl, c.ParentCategoryId,
            c.SubCategories.Select(s => new CategoryShortResponse(s.Id, s.Name, s.Description, s.ImageUrl, s.ParentCategoryId)).ToList()
        );
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req)
    {
        var cat = new Category { Name = req.Name, Description = req.Description, ImageUrl = req.ImageUrl, ParentCategoryId = req.ParentCategoryId };
        var created = await categoriesService.CreateCategoryAsync(cat);
        return CreatedAtAction(nameof(GetCategoryById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] CreateCategoryRequest req)
    {
        var cat = new Category { Name = req.Name, Description = req.Description, ImageUrl = req.ImageUrl, ParentCategoryId = req.ParentCategoryId };
        var updated = await categoriesService.UpdateCategoryAsync(id, cat);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] int id)
    {
        var success = await categoriesService.DeleteCategoryAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}