using System.Security.Claims;

namespace JyskBackend.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Id користувача з токена. Повертає null, якщо клейма немає або він не Guid —
    /// раніше цей розбір падав на Guid.Parse при будь-якому кривому токені.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole("Admin");
}
