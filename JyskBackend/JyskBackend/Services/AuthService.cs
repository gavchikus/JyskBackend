using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JyskBackend.Services;

public class AuthService(JyskDbContext context, IConfiguration configuration) : IAuthService
{
    private static readonly PasswordHasher<Customer> Hasher = new();

    // Ключ підпису живе в конфігурації (Development — appsettings.Development.json,
    // Production — змінна оточення Jwt__Key). У коді його більше немає.
    private string JwtKey => configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key не налаштовано.");

    private string Issuer => configuration["Jwt:Issuer"] ?? "JyskBackend";
    private string Audience => configuration["Jwt:Audience"] ?? "JyskFrontend";
    private int ExpiresDays => int.TryParse(configuration["Jwt:ExpiresDays"], out var d) ? d : 7;

    public async Task<Customer?> RegisterAsync(Customer customer, string password)
    {
        var email = customer.Email.Trim().ToLowerInvariant();
        if (await context.Customers.AnyAsync(c => c.Email == email))
            return null;

        customer.Email = email;
        customer.PasswordHash = Hasher.HashPassword(customer, password);
        // Роль з тіла запиту не приймаємо — самопризначити собі Admin неможливо.
        customer.Role = "Customer";
        customer.CreatedAt = DateTime.UtcNow;

        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    public async Task<(Customer? Customer, string? Token)> LoginAsync(string email, string password)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Email == normalized);
        if (customer == null) return (null, null);

        var result = Hasher.VerifyHashedPassword(customer, customer.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed) return (null, null);

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            customer.PasswordHash = Hasher.HashPassword(customer, password);
            await context.SaveChangesAsync();
        }

        return (customer, GenerateJwtToken(customer));
    }

    public async Task<Customer?> GetUserByIdAsync(Guid id) => await context.Customers.FindAsync(id);

    public async Task<Customer?> UpdateProfileAsync(Guid id, UpdateProfileRequest request)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer == null) return null;

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.PhoneNumber = request.PhoneNumber;
        customer.Address = request.Address;

        await context.SaveChangesAsync();
        return customer;
    }

    public async Task<List<Customer>> GetAllCustomersAsync() =>
        await context.Customers.OrderBy(c => c.CreatedAt).ToListAsync();

    public string GenerateJwtToken(Customer customer)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(JwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim(ClaimTypes.Role, customer.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(ExpiresDays),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
