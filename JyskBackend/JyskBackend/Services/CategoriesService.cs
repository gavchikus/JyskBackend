using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class CategoriesService(JyskDbContext context) : ICategoriesService
{
    public async Task<List<Category>> GetAllCategoriesAsync() =>
        await context.Categories.OrderBy(c => c.Name).ToListAsync();

    public async Task<Category?> GetCategoryByIdAsync(int id) =>
        await context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateCategoryAsync(int id, Category updatedCategory)
    {
        var existing = await context.Categories.FindAsync(id);
        if (existing == null) return null;

        existing.Name = updatedCategory.Name;
        existing.Description = updatedCategory.Description;
        existing.ImageUrl = updatedCategory.ImageUrl;
        existing.ParentCategoryId = updatedCategory.ParentCategoryId;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<DeletionResult> DeleteCategoryAsync(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null) return DeletionResult.NotFound;

        // FK на Product і на батьківську категорію стоять із Restrict: без цієї
        // перевірки EF кидав би DbUpdateException і клієнт отримував 500 замість 409.
        var hasDependents = await context.Products.AnyAsync(p => p.CategoryId == id)
                            || await context.Categories.AnyAsync(c => c.ParentCategoryId == id);
        if (hasDependents) return DeletionResult.Blocked;

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return DeletionResult.Deleted;
    }
}
