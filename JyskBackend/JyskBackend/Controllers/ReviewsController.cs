using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/reviews")]
public class ReviewsController(IReviewsService reviewsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetReviews([FromRoute] Guid productId)
    {
        var reviews = await reviewsService.GetReviewsByProductIdAsync(productId);
        var response = reviews.Select(r => new ReviewResponse(
            r.Id, r.Rating, r.Comment, r.CreatedAt, 
            r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName.Substring(0,1)}." : "Анонім"
        )).ToList();
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromRoute] Guid productId, [FromBody] CreateReviewRequest req)
    {
        var review = new ProductReview { Rating = req.Rating, Comment = req.Comment, CustomerId = req.CustomerId };
        var created = await reviewsService.CreateReviewAsync(productId, review);
        if (created == null) return NotFound("Product not found");
        return Created("", created);
    }
}