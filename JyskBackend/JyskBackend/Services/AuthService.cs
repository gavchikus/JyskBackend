using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;

namespace JyskBackend.Services;

public class AuthService(JyskDbContext context) : IAuthService
{
    private const string JwtKey = "SuperSecretJyskKey2026_DoNotShareItWithAnyone_MustBeLong";

    public async Task<Customer?> RegisterAsync(Customer customer, string password)
    {
        if (await context.Customers.AnyAsync(c => c.Email == customer.Email))
            return null;

        var hasher = new PasswordHasher<Customer>();
        customer.PasswordHash = hasher.HashPassword(customer, password);
        customer.Role = "Customer";
        customer.CreatedAt = DateTime.UtcNow;

        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    public async Task<(Customer? Customer, string? Token)> LoginAsync(string email, string password)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (customer == null) return (null, null);

        var hasher = new PasswordHasher<Customer>();
        var result = hasher.VerifyHashedPassword(customer, customer.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed) return (null, null);

        var token = GenerateJwtToken(customer);
        return (customer, token);
    }

    // Змінено тип аргументу з int на Guid
    public async Task<Customer?> GetUserByIdAsync(Guid id)
    {
        return await context.Customers.FindAsync(id);
    }

    public string GenerateJwtToken(Customer customer)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(JwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim(ClaimTypes.Role, customer.Role ?? "Customer")
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}