using JyskBackend.Entities;

namespace JyskBackend.Models.Responses;

/// <summary>
/// Єдине місце, де сутність товару перетворюється на короткий DTO.
/// Раніше цей маппінг був продубльований у трьох місцях (список, схожі товари,
/// головна) і встиг розійтися — категорія на головній підставлялася як "Unknown".
/// Тримаємо перетворення в одному методі, щоб такі розбіжності не поверталися.
/// </summary>
public static class ProductMapper
{
    public static ProductShortResponse ToShort(Product p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.OldPrice,
        p.ArticleNumber,
        p.Brand,
        p.Color,
        p.Material,
        p.Dimensions,
        p.IsNew,
        p.CategoryId,
        p.Category?.Name ?? "Unknown",
        p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
            ?? p.Images.FirstOrDefault()?.ImageUrl
            ?? "placeholder.jpg",
        p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0,
        p.Reviews.Count
    );

    public static List<ProductShortResponse> ToShort(IEnumerable<Product> products) =>
        products.Select(ToShort).ToList();
}
