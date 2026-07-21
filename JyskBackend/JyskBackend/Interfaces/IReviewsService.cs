using JyskBackend.Entities;

namespace JyskBackend.Interfaces;

public interface IReviewsService
{
    Task<List<ProductReview>> GetReviewsByProductIdAsync(Guid productId);
    Task<ProductReview?> CreateReviewAsync(Guid productId, ProductReview review);
}