using JyskBackend.Models.Responses;

namespace JyskBackend.Interfaces;

public interface IReviewsService
{
    Task<List<ReviewResponse>> GetReviewsByProductIdAsync(Guid productId);
    Task<ReviewResponse?> CreateReviewAsync(Guid productId, Guid customerId, CreateReviewRequest request);
}
