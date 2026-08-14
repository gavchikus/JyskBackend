using System.ComponentModel.DataAnnotations;

namespace JyskBackend.Models.Responses;

// ─── Auth ───────────────────────────────────────────────────────────────────
public record RegisterRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(128)] string Password,
    [Phone, MaxLength(20)] string? PhoneNumber);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, AuthUserResponse Customer);
public record AuthUserResponse(Guid Id, string FirstName, string LastName, string Email, string Role);

// ─── Customers ──────────────────────────────────────────────────────────────
public record UserProfileResponse(Guid Id, string FirstName, string LastName, string Email, string? PhoneNumber, string? Address, DateTime CreatedAt);
public record CustomerListResponse(Guid Id, string FirstName, string LastName, string Email, string Role, DateTime CreatedAt);

public record UpdateProfileRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Phone, MaxLength(20)] string? PhoneNumber,
    [MaxLength(500)] string? Address);

// ─── Products ───────────────────────────────────────────────────────────────
public record ProductShortResponse(
    Guid Id, string Name, string? Description, decimal Price, decimal? OldPrice, string? ArticleNumber,
    string? Brand, string? Color, string? Material, string? Dimensions, bool IsNew,
    int CategoryId, string CategoryName, string? MainImageUrl, double AvgRating, int ReviewCount
);

public record ProductDetailResponse(
    Guid Id, string Name, string? Description, decimal Price, decimal? OldPrice, int Stock,
    string? ArticleNumber, string? Brand, string? Color, string? Material, string? Dimensions,
    bool IsNew, bool IsActive, int CategoryId, string CategoryName,
    List<ProductImageResponse> Images, List<VariantResponse> Variants,
    double AvgRating, int ReviewCount, List<ProductShortResponse> RelatedProducts
);

public record ProductImageResponse(int Id, string ImageUrl, bool IsMain);

public record HomePageProductsResponse(
    List<ProductShortResponse> NewArrivals,
    List<ProductShortResponse> OnSale,
    List<ProductShortResponse> Recommended
);

/// <summary>Фільтри списку товарів. Приходить із query-рядка одним об'єктом.</summary>
public class ProductQuery
{
    public int? CategoryId { get; set; }
    public int? RoomId { get; set; }
    public int? CollectionId { get; set; }
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Color { get; set; }
    public string? Material { get; set; }
    public bool? IsNew { get; set; }
    public bool? OnSale { get; set; }
    public string? SortBy { get; set; }

    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>Номер сторінки, від 1.</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Розмір сторінки, 1..100. Нуль тут раніше давав ділення на нуль у totalPages.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch { < 1 => 20, > 100 => 100, _ => value };
    }
}

public record CreateProductRequest(
    [Required, MaxLength(255)] string Name,
    [MaxLength(2000)] string? Description,
    [Range(0, 10_000_000)] decimal Price,
    [Range(0, 10_000_000)] decimal? OldPrice,
    [Range(0, 1_000_000)] int Stock,
    [MaxLength(100)] string? Brand,
    [MaxLength(100)] string? Material,
    [MaxLength(50)] string? Color,
    [MaxLength(100)] string? Dimensions,
    [MaxLength(50)] string? ArticleNumber,
    bool IsNew,
    [Range(1, int.MaxValue)] int CategoryId,
    bool IsActive = true);

public record AddImageRequest(
    [Required, MaxLength(500)] string ImageUrl,
    bool IsMain);

// ─── Categories ─────────────────────────────────────────────────────────────
public record CategoryShortResponse(int Id, string Name, string? Description, string? ImageUrl, int? ParentCategoryId);
public record CategoryDetailResponse(int Id, string Name, string? Description, string? ImageUrl, int? ParentCategoryId, List<CategoryShortResponse> SubCategories);

public record CreateCategoryRequest(
    [Required, MaxLength(150)] string Name,
    [MaxLength(500)] string? Description,
    [MaxLength(255)] string? ImageUrl,
    int? ParentCategoryId);

// ─── Variants ───────────────────────────────────────────────────────────────
public record VariantResponse(int Id, string? Label, string? Color, string? Size, decimal Price, int Stock);

public record CreateVariantRequest(
    [MaxLength(50)] string? Color,
    [MaxLength(50)] string? Size,
    [MaxLength(100)] string? Label,
    [Range(0, 10_000_000)] decimal Price,
    [Range(0, 1_000_000)] int Stock);

// ─── Reviews ────────────────────────────────────────────────────────────────
public record ReviewResponse(int Id, int Rating, string? Comment, DateTime CreatedAt, string CustomerName);

/// <summary>
/// Автор відгуку береться з JWT, а не з тіла запиту — інакше будь-хто
/// міг би підписати відгук чужим CustomerId.
/// </summary>
public record CreateReviewRequest(
    [Range(1, 5)] int Rating,
    [MaxLength(1000)] string? Comment);

// ─── Rooms / Collections ────────────────────────────────────────────────────
public record ShowcaseShortResponse(int Id, string Name, string? Description, string? CoverImageUrl);
public record ShowcaseProductResponse(Guid Id, string Name, decimal Price, string? MainImageUrl);
public record ShowcaseDetailResponse(int Id, string Name, string? Description, string? CoverImageUrl, List<ShowcaseProductResponse> Products);

public record CreateShowcaseRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [MaxLength(255)] string? CoverImageUrl);

public record AddShowcaseProductRequest([Required] Guid ProductId);

// ─── Orders ─────────────────────────────────────────────────────────────────
public record OrderListResponse(Guid Id, string Status, decimal TotalAmount, string DeliveryAddress, DateTime CreatedAt, int ItemCount, string? CustomerName);
public record OrderItemResponse(Guid ProductId, string ProductName, string? VariantLabel, string? ImageUrl, int Quantity, decimal UnitPrice);
public record OrderDetailResponse(Guid Id, string Status, decimal TotalAmount, string DeliveryAddress, string? Comment, DateTime CreatedAt, List<OrderItemResponse> Items);

public record CreateOrderItemRequest(
    [Required] Guid ProductId,
    int? VariantId,
    [Range(1, 1000)] int Quantity);

public record CreateOrderRequest(
    [Required, MaxLength(500)] string DeliveryAddress,
    [MaxLength(500)] string? Comment,
    [Required, MinLength(1)] List<CreateOrderItemRequest> Items);

public record UpdateOrderStatusRequest([Required] string Status);

// ─── Спільне ────────────────────────────────────────────────────────────────
public record PaginatedList<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages)
{
    public static PaginatedList<T> From(List<T> items, int totalCount, int page, int pageSize)
    {
        var safePageSize = pageSize < 1 ? 1 : pageSize;
        var totalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);
        return new PaginatedList<T>(items, totalCount, page, safePageSize, totalPages);
    }
}
