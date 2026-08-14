using JyskBackend.Extensions;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Профіль поточного користувача за JWT.
    /// </summary>
    /// <response code="200">Профіль знайдено</response>
    /// <response code="401">Токен відсутній або недійсний</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyProfile()
    {
        // Раніше запит без токена повертав 200 із вигаданим користувачем «Олена
        // Ковальчук» — фронт вважав гостя авторизованим.
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var user = await authService.GetUserByIdAsync(userId.Value);
        if (user == null) return Unauthorized();

        return Ok(ToProfile(user));
    }

    /// <summary>Оновити власний профіль. Email і роль тут не змінюються.</summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest req)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var updated = await authService.UpdateProfileAsync(userId.Value, req);
        return updated == null ? Unauthorized() : Ok(ToProfile(updated));
    }

    /// <summary>Список усіх користувачів. Лише для адміністратора.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<CustomerListResponse>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAllCustomers()
    {
        var customers = await authService.GetAllCustomersAsync();
        return Ok(customers
            .Select(c => new CustomerListResponse(c.Id, c.FirstName, c.LastName, c.Email, c.Role, c.CreatedAt))
            .ToList());
    }

    private static UserProfileResponse ToProfile(Entities.Customer user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.Address, user.CreatedAt);
}
