using JyskBackend.Entities;
using JyskBackend.Models.Responses;

namespace JyskBackend.Interfaces;

public interface IProductsService
{
    Task<(List<Product> Items, int TotalCount)> GetAllProductsAsync(
        int? categoryId, string? search, decimal? minPrice, decimal? maxPrice, 
        string? color, string? material, bool? isNew, bool? onSale, 
        int page, int pageSize, string? sortBy);

    Task<Product?> GetProductByIdAsync(Guid id);
    Task<Product> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(Guid id, Product updatedProduct);
    Task<bool> DeleteProductAsync(Guid id);
    Task<HomePageProductsResponse> GetHomePageProductsAsync();
    Task<ProductImage?> AddProductImageAsync(Guid productId, string imageUrl, bool isMain);
}