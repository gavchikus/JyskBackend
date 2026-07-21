using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class ProductsService(JyskDbContext context) : IProductsService
{
    public async Task<(List<Product> Items, int TotalCount)> GetAllProductsAsync(
        int? categoryId, string? search, decimal? minPrice, decimal? maxPrice, 
        string? color, string? material, bool? isNew, bool? onSale, 
        int page, int pageSize, string? sortBy)
    {
        var query = context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .AsQueryable();

        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
        if (!string.IsNullOrEmpty(color)) query = query.Where(p => p.Color == color);
        if (!string.IsNullOrEmpty(material)) query = query.Where(p => p.Material == material);
        if (isNew.HasValue) query = query.Where(p => p.IsNew == isNew.Value);
        
        if (onSale.HasValue && onSale.Value) query = query.Where(p => p.OldPrice.HasValue && p.OldPrice > p.Price);

        
        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "rating" => query.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0),
            _ => query.OrderBy(p => p.Id)
        };

        int totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .ThenInclude(r => r.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateProductAsync(Guid id, Product updatedProduct)
    {
        var existing = await context.Products.FindAsync(id);
        if (existing == null) return null;

        existing.Name = updatedProduct.Name;
        existing.Description = updatedProduct.Description;
        existing.Price = updatedProduct.Price;
        existing.OldPrice = updatedProduct.OldPrice;
        existing.Stock = updatedProduct.Stock;
        existing.Brand = updatedProduct.Brand;
        existing.Material = updatedProduct.Material;
        existing.Color = updatedProduct.Color;
        existing.Dimensions = updatedProduct.Dimensions;
        existing.ArticleNumber = updatedProduct.ArticleNumber;
        existing.IsNew = updatedProduct.IsNew;
        existing.CategoryId = updatedProduct.CategoryId;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null) return false;

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Product>> GetRelatedProductsAsync(int categoryId, Guid currentProductId, int limit = 4)
    {
        return await context.Products
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.CategoryId == categoryId && p.Id != currentProductId)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<HomePageProductsResponse> GetHomePageProductsAsync()
    {
        var newProducts = await context.Products
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.IsNew) 
            .Take(8)
            .ToListAsync();

        var saleProducts = await context.Products
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.OldPrice.HasValue && p.OldPrice > p.Price)
            .Take(8)
            .ToListAsync();

        var recommendedProducts = await context.Products
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0)
            .Take(8)
            .ToListAsync();

        Func<Product, ProductShortResponse> mapToShortDto = p => new ProductShortResponse(
            p.Id,
            p.Name,
            p.Price,
            p.OldPrice,
            p.ArticleNumber,
            p.Brand,
            p.Color,
            p.Material,
            p.Dimensions,
            p.IsNew,
            p.CategoryId,
            p.Category?.Name ?? "Unknown",
            p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl ?? "placeholder.jpg",
            p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0.0,
            p.Reviews.Count
        );

        return new HomePageProductsResponse(
            NewArrivals: newProducts.Select(mapToShortDto).ToList(),
            OnSale: saleProducts.Select(mapToShortDto).ToList(),
            Recommended: recommendedProducts.Select(mapToShortDto).ToList()
        );
    }
    public async Task<ProductImage?> AddProductImageAsync(Guid productId, string imageUrl, bool isMain)
    {
        var product = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return null;

        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsMain = isMain
        };

        context.ProductImages.Add(image);
        await context.SaveChangesAsync();

        return image;
    }
}