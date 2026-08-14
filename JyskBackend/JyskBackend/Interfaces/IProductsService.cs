using JyskBackend.Entities;
using JyskBackend.Models.Responses;

namespace JyskBackend.Interfaces;

/// <summary>Результат видалення: сутність могла бути відсутня або мати посилання із замовлень.</summary>
public enum DeletionResult
{
    Deleted,
    NotFound,
    Blocked
}

public interface IProductsService
{
    Task<(List<ProductShortResponse> Items, int TotalCount)> GetAllProductsAsync(ProductQuery query);
    Task<ProductDetailResponse?> GetProductDetailAsync(Guid id);
    Task<Product> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(Guid id, Product updatedProduct);
    Task<DeletionResult> DeleteProductAsync(Guid id);
    Task<HomePageProductsResponse> GetHomePageProductsAsync();
    Task<ProductImage?> AddProductImageAsync(Guid productId, string imageUrl, bool isMain);
}
