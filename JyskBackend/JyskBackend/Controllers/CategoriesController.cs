using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public class CategoriesController(ICategoriesService categoriesService) : ControllerBase
{
    /// <summary>Плоский список усіх категорій разом із підкатегоріями.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CategoryShortResponse>), 200)]
    public async Task<IActionResult> GetAllCategories()
    {
        var cats = await categoriesService.GetAllCategoriesAsync();
        return Ok(cats.Select(ToShort).ToList());
    }

    /// <summary>Категорія з переліком безпосередніх підкатегорій.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDetailResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        var c = await categoriesService.GetCategoryByIdAsync(id);
        if (c == null) return NotFound();

        return Ok(new CategoryDetailResponse(
            c.Id, c.Name, c.Description, c.ImageUrl, c.ParentCategoryId,
            c.SubCategories.Select(ToShort).ToList()));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryShortResponse), 201)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest req)
    {
        var created = await categoriesService.CreateCategoryAsync(ToEntity(req));
        return CreatedAtAction(nameof(GetCategoryById), new { id = created.Id }, ToShort(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryShortResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] CreateCategoryRequest req)
    {
        // Категорія не може бути власним батьком — інакше дерево замикається саме на себе.
        if (req.ParentCategoryId == id)
            return BadRequest(new { message = "Категорія не може бути батьківською для самої себе" });

        var updated = await categoriesService.UpdateCategoryAsync(id, ToEntity(req));
        return updated == null ? NotFound() : Ok(ToShort(updated));
    }

    /// <summary>Видалити категорію. Не спрацює, поки в ній є товари або підкатегорії.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DeleteCategory([FromRoute] int id)
    {
        var result = await categoriesService.DeleteCategoryAsync(id);
        return result switch
        {
            DeletionResult.Deleted => NoContent(),
            DeletionResult.NotFound => NotFound(),
            _ => Conflict(new { message = "У категорії є товари або підкатегорії — спершу перенесіть їх" })
        };
    }

    private static CategoryShortResponse ToShort(Category c) =>
        new(c.Id, c.Name, c.Description, c.ImageUrl, c.ParentCategoryId);

    private static Category ToEntity(CreateCategoryRequest req) => new()
    {
        Name = req.Name,
        Description = req.Description,
        ImageUrl = req.ImageUrl,
        ParentCategoryId = req.ParentCategoryId
    };
}
