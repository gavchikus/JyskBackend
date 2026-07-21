using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class ReviewsService(JyskDbContext context) : IReviewsService
{
    public async Task<List<ProductReview>> GetReviewsByProductIdAsync(Guid productId)
    {
        return await context.ProductReviews
            .Include(r => r.Customer)
            .Where(r => r.ProductId == productId)
            .ToListAsync();
    }

    public async Task<ProductReview?> CreateReviewAsync(Guid productId, ProductReview review)
    {
        var productExists = await context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists) return null;

        review.ProductId = productId;
        context.ProductReviews.Add(review);
        await context.SaveChangesAsync();
        return review;
    }
}