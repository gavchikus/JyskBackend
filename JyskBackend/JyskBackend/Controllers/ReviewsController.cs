using JyskBackend.Extensions;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/reviews")]
[Produces("application/json")]
public class ReviewsController(IReviewsService reviewsService) : ControllerBase
{
    /// <summary>Відгуки про товар, найновіші зверху.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ReviewResponse>), 200)]
    public async Task<IActionResult> GetReviews([FromRoute] Guid productId) =>
        Ok(await reviewsService.GetReviewsByProductIdAsync(productId));

    /// <summary>
    /// Залишити відгук. Автор береться з JWT: раніше CustomerId приходив у тілі
    /// запиту, і будь-хто міг підписати відгук чужим іменем.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReviewResponse), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateReview([FromRoute] Guid productId, [FromBody] CreateReviewRequest req)
    {
        var customerId = User.GetUserId();
        if (customerId == null) return Unauthorized();

        var created = await reviewsService.CreateReviewAsync(productId, customerId.Value, req);
        return created == null ? NotFound(new { message = "Товар не знайдено" }) : Created(string.Empty, created);
    }
}
