using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JyskBackend.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Реєстрація нового покупця в системі магазину TALO.
    /// </summary>
    /// <param name="req">Об'єкт з даними реєстрації (Ім'я, Email, Пароль тощо)</param>
    /// <returns>JWT токен та інформація про створений профіль</returns>
    /// <response code="201">Користувача успішно створено, токен згенеровано</response>
    /// <response code="409">Помилка конфлікту: користувач з таким Email вже існує</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var customer = new Customer
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email,
            PhoneNumber = req.PhoneNumber
            // Роль призначає сервіс — з тіла запиту вона не приймається.
        };

        var created = await authService.RegisterAsync(customer, req.Password);
        if (created == null) return Conflict(new { message = "Цей Email вже зареєстрований у системі" });

        var token = authService.GenerateJwtToken(created);
        var response = new AuthResponse(token,
            new AuthUserResponse(created.Id, created.FirstName, created.LastName, created.Email, created.Role));

        return Created(string.Empty, response);
    }

    /// <summary>
    /// Автентифікація (Вхід) користувача за логіном та паролем.
    /// </summary>
    /// <param name="req">Модель, що містить Email та пароль</param>
    /// <returns>JWT токен доступу Bearer</returns>
    /// <response code="200">Успішний вхід. Повертає токен для заголовка Authorization</response>
    /// <response code="401">Невірний Email або пароль</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var (customer, token) = await authService.LoginAsync(req.Email, req.Password);
        if (customer == null || token == null) return Unauthorized(new { message = "Неправильний email або пароль" });

        var response = new AuthResponse(token,
            new AuthUserResponse(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.Role));
        return Ok(response);
    }
}
