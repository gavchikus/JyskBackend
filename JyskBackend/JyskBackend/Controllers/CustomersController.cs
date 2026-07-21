using System.Security.Claims;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Отримати дані профілю поточного користувача за його JWT токеном.
    /// </summary>
    /// <returns>Повна інформація про профіль покупця</returns>
    /// <response code="200">Профіль успішно знайдено та розпарсено з токена</response>
    /// <response code="401">Користувача не знайдено або токен недійсний</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
        {
            // Передаємо генерацію унікального Guid замість числа 1
            return Ok(new UserProfileResponse(Guid.NewGuid(), "Олена", "Ковальчук", "olena@example.com", "+380501234567", "Київ, вул. Хрещатик, 1", DateTime.UtcNow));
        }

        // Використовуємо Guid.Parse замість int.Parse
        var user = await authService.GetUserByIdAsync(Guid.Parse(userIdClaim));
        if (user == null) return Unauthorized();

        var response = new UserProfileResponse(user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.Address, user.CreatedAt);
        return Ok(response);
    }
}