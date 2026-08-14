using JyskBackend.Models.Responses;

namespace JyskBackend.Interfaces;

/// <summary>
/// Кімнати та колекції — це та сама «вітрина»: назва, обкладинка й набір товарів.
/// Контракт розділяє їх за URL, тому інтерфейси окремі, а логіка спільна.
/// </summary>
public interface IShowcaseService
{
    Task<List<ShowcaseShortResponse>> GetAllAsync();
    Task<ShowcaseDetailResponse?> GetByIdAsync(int id);
    Task<ShowcaseShortResponse> CreateAsync(CreateShowcaseRequest request);
    Task<bool> AddProductAsync(int showcaseId, Guid productId);
    Task<bool> RemoveProductAsync(int showcaseId, Guid productId);
}

public interface IRoomsService : IShowcaseService;

public interface ICollectionsService : IShowcaseService;
