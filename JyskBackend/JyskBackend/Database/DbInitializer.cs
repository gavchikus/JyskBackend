using JyskBackend.Entities;

namespace JyskBackend.Database;

public static class DbInitializer
{
    public static async Task SeedAsync(JyskDbContext context)
    {
        if (context.Products.Any()) return; // Если база уже не пустая, ничего не делаем

        var categories = new List<Category>
        {
            new() { Name = "Меблі для спальні" },
            new() { Name = "Вітальня" },
            new() { Name = "Офіс та кабінет" }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new() {
                Id = Guid.NewGuid(), Name = "Ліжко кутове JYSK Vedde", Description = "Комфортне двоспальне ліжко", 
                Price = 11500, OldPrice = 14000, Stock = 12, ArticleNumber = "JYSK-001", IsNew = true, CategoryId = categories[0].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Диван розкладний MARIBO", Description = "Стильний сірий диван у вітальню", 
                Price = 18900, OldPrice = null, Stock = 5, ArticleNumber = "JYSK-002", IsNew = true, CategoryId = categories[1].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Стіл робочий Loft Desk", Description = "Ергономічний стіл для роботи", 
                Price = 4200, OldPrice = 5500, Stock = 20, ArticleNumber = "JYSK-003", IsNew = false, CategoryId = categories[2].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1518455027359-f3f8164ba6bd", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Крісло офісне TJELE", Description = "Еко-шкіра, регулювання висоти", 
                Price = 6700, OldPrice = 8000, Stock = 8, ArticleNumber = "JYSK-004", IsNew = false, CategoryId = categories[2].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1580481072645-022f9a6d83d0", IsMain = true } ]
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}