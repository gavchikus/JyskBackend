namespace JyskBackend.Models.Responses;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string? PhoneNumber);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, AuthUserResponse Customer);
public record AuthUserResponse(Guid Id, string FirstName, string LastName, string Email, string Role);
public record UserProfileResponse(Guid Id, string FirstName, string LastName, string Email, string? PhoneNumber, string? Address, DateTime CreatedAt);
public record ProductShortResponse(
    Guid Id, string Name, decimal Price, decimal? OldPrice, string? ArticleNumber,
    string? Brand, string? Color, string? Material, string? Dimensions, bool IsNew,
    int CategoryId, string CategoryName, string ImageUrl, double AvgRating, int ReviewCount
);
public record ProductDetailResponse(
    Guid Id, string Name, string Description, decimal Price, decimal? OldPrice, int Stock,
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
public record CategoryShortResponse(int Id, string Name, string? Description, string? ImageUrl, int? ParentCategoryId);
public record CategoryDetailResponse(int Id, string Name, string? Description, string? ImageUrl, int? ParentCategoryId, List<CategoryShortResponse> SubCategories);

public record VariantResponse(int Id, string? Label, string? Color, string? Size, decimal Price, int Stock);
public record ReviewResponse(int Id, int Rating, string? Comment, DateTime CreatedAt, string CustomerName);

public record PaginatedList<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public record CreateProductRequest(string Name, string? Description, decimal Price, decimal? OldPrice, int Stock, string? Brand, string? Material, string? Color, string? Dimensions, string? ArticleNumber, bool IsNew, int CategoryId);
public record CreateCategoryRequest(string Name, string? Description, string? ImageUrl, int? ParentCategoryId);
public record CreateVariantRequest(string? Color, string? Size, string? Label, decimal Price, int Stock);
public record CreateReviewRequest(int Rating, string? Comment, Guid CustomerId); 