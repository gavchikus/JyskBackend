using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class CategoriesService(JyskDbContext context) : ICategoriesService
{
    public async Task<List<Category>> GetAllCategoriesAsync() => await context.Categories.ToListAsync();

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

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

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category == null) return false;

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }
}