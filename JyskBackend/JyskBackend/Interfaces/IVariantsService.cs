using JyskBackend.Entities;

namespace JyskBackend.Interfaces;

public interface IVariantsService
{
    Task<List<ProductVariant>> GetVariantsByProductIdAsync(Guid productId);
    Task<ProductVariant?> CreateVariantAsync(Guid productId, ProductVariant variant);
    Task<bool> DeleteVariantAsync(Guid productId, int variantId);
}