using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Extensions;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class ProductsService(JyskDbContext context) : IProductsService
{
    private const int RelatedProductsLimit = 4;
    private const int HomePageBlockSize = 8;

    public async Task<(List<ProductShortResponse> Items, int TotalCount)> GetAllProductsAsync(ProductQuery q)
    {
        // Вимкнені товари не мають потрапляти у видачу — раніше прапорець IsActive
        // існував, але жодного разу не перевірявся.
        var query = context.Products.Where(p => p.IsActive);

        if (q.CategoryId.HasValue) query = query.Where(p => p.CategoryId == q.CategoryId.Value);
        if (q.RoomId.HasValue) query = query.Where(p => p.RoomProducts.Any(rp => rp.RoomId == q.RoomId.Value));
        if (q.CollectionId.HasValue) query = query.Where(p => p.CollectionProducts.Any(cp => cp.CollectionId == q.CollectionId.Value));

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            // Пошук тільки за назвою пропускав очевидні запити на кшталт артикула.
            var term = q.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                (p.ArticleNumber != null && p.ArticleNumber.ToLower().Contains(term)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(term)));
        }

        if (q.MinPrice.HasValue) query = query.Where(p => p.Price >= q.MinPrice.Value);
        if (q.MaxPrice.HasValue) query = query.Where(p => p.Price <= q.MaxPrice.Value);
        if (!string.IsNullOrEmpty(q.Color)) query = query.Where(p => p.Color == q.Color);
        if (!string.IsNullOrEmpty(q.Material)) query = query.Where(p => p.Material == q.Material);
        if (q.IsNew.HasValue) query = query.Where(p => p.IsNew == q.IsNew.Value);
        if (q.OnSale == true) query = query.Where(p => p.OldPrice.HasValue && p.OldPrice > p.Price);

        query = q.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "rating" => query.OrderByDescending(p => p.Reviews.Count == 0 ? 0d : p.Reviews.Average(r => (double)r.Rating)),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .SelectShort()
            .ToListAsync();

        return (items.WithRoundedRating(), totalCount);
    }

    public async Task<ProductDetailResponse?> GetProductDetailAsync(Guid id)
    {
        var product = await context.Products
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        var related = await context.Products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(RelatedProductsLimit)
            .SelectShort()
            .ToListAsync();

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.OldPrice,
            product.Stock,
            product.ArticleNumber,
            product.Brand,
            product.Color,
            product.Material,
            product.Dimensions,
            product.IsNew,
            product.IsActive,
            product.CategoryId,
            product.Category?.Name ?? "Unknown",
            product.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.SortOrder)
                .Select(i => new ProductImageResponse(i.Id, i.ImageUrl, i.IsMain)).ToList(),
            product.Variants.Select(v => new VariantResponse(v.Id, v.Label, v.Color, v.Size, v.Price, v.Stock)).ToList(),
            product.Reviews.Count == 0 ? 0d : Math.Round(product.Reviews.Average(r => (double)r.Rating), 1),
            product.Reviews.Count,
            related.WithRoundedRating()
        );
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
        existing.IsActive = updatedProduct.IsActive;
        existing.CategoryId = updatedProduct.CategoryId;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<DeletionResult> DeleteProductAsync(Guid id)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null) return DeletionResult.NotFound;

        // Каскад по OrderItem знищив би позиції вже оформлених замовлень
        // разом з історією покупок. Такий товар лише знімаємо з продажу.
        if (await context.OrderItems.AnyAsync(oi => oi.ProductId == id))
        {
            product.IsActive = false;
            await context.SaveChangesAsync();
            return DeletionResult.Blocked;
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return DeletionResult.Deleted;
    }

    public async Task<HomePageProductsResponse> GetHomePageProductsAsync()
    {
        var active = context.Products.Where(p => p.IsActive);

        var newArrivals = await active
            .Where(p => p.IsNew)
            .OrderByDescending(p => p.CreatedAt)
            .Take(HomePageBlockSize)
            .SelectShort()
            .ToListAsync();

        var onSale = await active
            .Where(p => p.OldPrice.HasValue && p.OldPrice > p.Price)
            .OrderByDescending(p => p.OldPrice - p.Price)
            .Take(HomePageBlockSize)
            .SelectShort()
            .ToListAsync();

        var recommended = await active
            .OrderByDescending(p => p.Reviews.Count == 0 ? 0d : p.Reviews.Average(r => (double)r.Rating))
            .ThenByDescending(p => p.Reviews.Count)
            .Take(HomePageBlockSize)
            .SelectShort()
            .ToListAsync();

        return new HomePageProductsResponse(
            newArrivals.WithRoundedRating(),
            onSale.WithRoundedRating(),
            recommended.WithRoundedRating());
    }

    public async Task<ProductImage?> AddProductImageAsync(Guid productId, string imageUrl, bool isMain)
    {
        var product = await context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return null;

        // Головне фото має бути одне: нове знімає прапорець зі старого.
        if (isMain)
        {
            foreach (var existing in product.Images.Where(i => i.IsMain))
            {
                existing.IsMain = false;
            }
        }

        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsMain = isMain || product.Images.Count == 0,
            SortOrder = product.Images.Count
        };

        context.ProductImages.Add(image);
        await context.SaveChangesAsync();

        return image;
    }
}
