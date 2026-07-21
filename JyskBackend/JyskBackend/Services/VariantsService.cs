using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class VariantsService(JyskDbContext context) : IVariantsService
{
    public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(Guid productId)
    {
        return await context.ProductVariants.Where(v => v.ProductId == productId).ToListAsync();
    }

    public async Task<ProductVariant?> CreateVariantAsync(Guid productId, ProductVariant variant)
    {
        var productExists = await context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists) return null;

        variant.ProductId = productId;
        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();
        return variant;
    }

    public async Task<bool> DeleteVariantAsync(Guid productId, int variantId)
    {
        var variant = await context.ProductVariants.FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId);
        if (variant == null) return false;

        context.ProductVariants.Remove(variant);
        await context.SaveChangesAsync();
        return true;
    }
}