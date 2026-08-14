using System.Linq.Expressions;
using JyskBackend.Entities;
using JyskBackend.Models.Responses;

namespace JyskBackend.Extensions;

/// <summary>
/// Один опис перетворення Product → DTO на весь проєкт.
/// Раніше цей маппінг був скопійований у трьох місцях і встиг розійтися:
/// у списку схожих товарів категорія не підвантажувалась і завжди
/// віддавалась як "Unknown".
/// </summary>
public static class ProductMappings
{
    /// <summary>
    /// Проєкція виконується в SQL: середній рейтинг і головне фото рахує база,
    /// тому не доводиться тягнути в пам'ять усі відгуки та зображення товару.
    /// </summary>
    public static readonly Expression<Func<Product, ProductShortResponse>> ToShort = p =>
        new ProductShortResponse(
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
            p.Category.Name,
            p.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault()
                ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
            p.Reviews.Count == 0 ? 0d : p.Reviews.Average(r => (double)r.Rating),
            p.Reviews.Count
        );

    public static IQueryable<ProductShortResponse> SelectShort(this IQueryable<Product> query) =>
        query.Select(ToShort);

    /// <summary>Рейтинг округлюємо вже в пам'яті — SQLite-версії по-різному округлюють half-up.</summary>
    public static ProductShortResponse WithRoundedRating(this ProductShortResponse dto) =>
        dto with { AvgRating = Math.Round(dto.AvgRating, 1) };

    public static List<ProductShortResponse> WithRoundedRating(this IEnumerable<ProductShortResponse> items) =>
        items.Select(WithRoundedRating).ToList();

    public static ShowcaseProductResponse ToShowcaseProduct(this Product p) =>
        new(p.Id, p.Name, p.Price,
            p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl);
}
