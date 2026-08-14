using JyskBackend.Entities;

namespace JyskBackend.Interfaces;

public interface ICategoriesService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category?> UpdateCategoryAsync(int id, Category updatedCategory);
    Task<DeletionResult> DeleteCategoryAsync(int id);
}
