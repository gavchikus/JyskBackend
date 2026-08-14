using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class ReviewsService(JyskDbContext context) : IReviewsService
{
    public async Task<List<ReviewResponse>> GetReviewsByProductIdAsync(Guid productId)
    {
        var reviews = await context.ProductReviews
            .Include(r => r.Customer)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews
            .Select(r => new ReviewResponse(r.Id, r.Rating, r.Comment, r.CreatedAt, DisplayName(r.Customer)))
            .ToList();
    }

    public async Task<ReviewResponse?> CreateReviewAsync(Guid productId, Guid customerId, CreateReviewRequest request)
    {
        var productExists = await context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists) return null;

        var review = new ProductReview
        {
            ProductId = productId,
            CustomerId = customerId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        context.ProductReviews.Add(review);
        await context.SaveChangesAsync();

        var customer = await context.Customers.FindAsync(customerId);
        return new ReviewResponse(review.Id, review.Rating, review.Comment, review.CreatedAt, DisplayName(customer));
    }

    /// <summary>
    /// «Олена К.» — ім'я та перша літера прізвища. Порожнє прізвище раніше
    /// валило весь список відгуків через Substring(0, 1).
    /// </summary>
    private static string DisplayName(Customer? customer)
    {
        if (customer == null) return "Анонім";

        var firstName = customer.FirstName?.Trim();
        if (string.IsNullOrEmpty(firstName)) return "Анонім";

        var lastName = customer.LastName?.Trim();
        return string.IsNullOrEmpty(lastName) ? firstName : $"{firstName} {lastName[0]}.";
    }
}
