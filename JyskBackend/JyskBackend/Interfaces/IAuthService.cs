using JyskBackend.Entities;

namespace JyskBackend.Interfaces;

public interface IAuthService
{
    Task<Customer?> RegisterAsync(Customer customer, string password);
    Task<(Customer? Customer, string? Token)> LoginAsync(string email, string password);
    Task<Customer?> GetUserByIdAsync(Guid id);
    string GenerateJwtToken(Customer customer);
}