using JyskBackend.Entities;
using JyskBackend.Models.Responses;

namespace JyskBackend.Interfaces;

public interface IAuthService
{
    Task<Customer?> RegisterAsync(Customer customer, string password);
    Task<(Customer? Customer, string? Token)> LoginAsync(string email, string password);
    Task<Customer?> GetUserByIdAsync(Guid id);
    Task<Customer?> UpdateProfileAsync(Guid id, UpdateProfileRequest request);
    Task<List<Customer>> GetAllCustomersAsync();
    string GenerateJwtToken(Customer customer);
}
