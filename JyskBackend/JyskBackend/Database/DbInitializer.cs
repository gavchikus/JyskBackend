using JyskBackend.Entities;

namespace JyskBackend.Database;

public static class DbInitializer
{
    public static async Task SeedAsync(JyskDbContext context)
    {
        if (context.Products.Any()) return; 

        var categories = new List<Category>
        {
            new() { Name = "Меблі для спальні" },  // categories[0]
            new() { Name = "Вітальня" },          // categories[1]
            new() { Name = "Офіс та кабінет" }   // categories[2]
        };
        
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new() {
                Id = Guid.NewGuid(), Name = "Ліжко кутове JYSK Vedde", Description = "Комфортне двоспальне ліжко з міцним каркасом", 
                Price = 11500, OldPrice = 14000, Stock = 12, ArticleNumber = "JYSK-001", IsNew = true, CategoryId = categories[0].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Матрац безпружинний GOLD F30", Description = "Високоякісний ортопедичний матрац з ефектом пам'яті", 
                Price = 8900, OldPrice = 11200, Stock = 8, ArticleNumber = "JYSK-005", IsNew = false, CategoryId = categories[0].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1631049307264-da0ec9d70304", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Тумба приліжкова ABETONE", Description = "Компактна тумба з двома шухлядами для спальні", 
                Price = 2100, OldPrice = null, Stock = 25, ArticleNumber = "JYSK-006", IsNew = true, CategoryId = categories[0].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1532323544230-7191fd51bc1b", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Шафа для одягу TARP 2-дверна", Description = "Містка шафа-купе з дзеркалом та висувними полицями", 
                Price = 15400, OldPrice = 18000, Stock = 4, ArticleNumber = "JYSK-007", IsNew = false, CategoryId = categories[0].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1595428774223-ef52624120d2", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Комод VEDDE 4 шухляди", Description = "Стильний комод у кольорі дуб для білизни та речей", 
                Price = 6200, OldPrice = 7500, Stock = 10, ArticleNumber = "JYSK-008", IsNew = false, CategoryId = categories[0].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1558882224-dda166733046", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Подушка анатомічна WELLPUR", Description = "Ортопедична подушка з наповнювачем Memory Foam", 
                Price = 1450, OldPrice = null, Stock = 30, ArticleNumber = "JYSK-009", IsNew = true, CategoryId = categories[0].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1584100936595-c0654b55a2e2", IsMain = true } ]
            },

            new() {
                Id = Guid.NewGuid(), Name = "Диван розкладний MARIBO", Description = "Стильний сірий диван у вітальню з функцією сну", 
                Price = 18900, OldPrice = null, Stock = 5, ArticleNumber = "JYSK-002", IsNew = true, CategoryId = categories[1].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Журнальний столик GANDRUP", Description = "Лаконічний круглий столик із дерев'яною стільницею", 
                Price = 3100, OldPrice = 3800, Stock = 18, ArticleNumber = "JYSK-010", IsNew = false, CategoryId = categories[1].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1533779283484-83494495497f", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Крісло відпочинкове HAVNDAL", Description = "М'яке затишне крісло з ергономічною спинкою", 
                Price = 7200, OldPrice = 8500, Stock = 7, ArticleNumber = "JYSK-011", IsNew = false, CategoryId = categories[1].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Тумба під ТВ HAGEN", Description = "Сучасна тумба під телевізор із закритими полицями", 
                Price = 4800, OldPrice = null, Stock = 14, ArticleNumber = "JYSK-012", IsNew = true, CategoryId = categories[1].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1607400201515-c2c41c07d307", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Стелаж підлоговий BROBY", Description = "Легкий відкритий стелаж для книг, рослин та декору", 
                Price = 2900, OldPrice = 3500, Stock = 22, ArticleNumber = "JYSK-013", IsNew = false, CategoryId = categories[1].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1594620302200-9a762244a156", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Килим тканий BARRIT 160x230", Description = "Безворсовий прямокутний килим нейтрального відтінку", 
                Price = 2600, OldPrice = null, Stock = 15, ArticleNumber = "JYSK-014", IsNew = false, CategoryId = categories[1].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1600121848594-d8644e57abab", IsMain = true } ]
            },

            new() {
                Id = Guid.NewGuid(), Name = "Стіл робочий Loft Desk", Description = "Ергономічний стіл для роботи та навчання", 
                Price = 4200, OldPrice = 5500, Stock = 20, ArticleNumber = "JYSK-003", IsNew = false, CategoryId = categories[2].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1518455027359-f3f8164ba6bd", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Крісло офісне TJELE", Description = "Еко-шкіра, регулювання висоти та нахилу", 
                Price = 6700, OldPrice = 8000, Stock = 8, ArticleNumber = "JYSK-004", IsNew = false, CategoryId = categories[2].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1580481072645-022f9a6d83d0", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Стіл з регулюванням висоти SLANGERUP", Description = "Електричний стіл для роботи стоячи та сидячи", 
                Price = 13500, OldPrice = 16000, Stock = 6, ArticleNumber = "JYSK-015", IsNew = true, CategoryId = categories[2].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Геймерське крісло AGERSTRUP", Description = "Ергономічне геймерське крісло з підтримкою попереку", 
                Price = 7900, OldPrice = 9200, Stock = 11, ArticleNumber = "JYSK-016", IsNew = true, CategoryId = categories[2].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1598300042247-d088f8ab3a91", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Шафа офісна для документів MUSDAL", Description = "Шафа з замком та регульованими по висоті полицями", 
                Price = 5400, OldPrice = null, Stock = 9, ArticleNumber = "JYSK-017", IsNew = false, CategoryId = categories[2].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1505330622279-bf7d7fc918f4", IsMain = true } ]
            },
            new() {
                Id = Guid.NewGuid(), Name = "Настільна лампа LED FREDERIK", Description = "Світлодіодна лампа з регулюванням яскравості та USB-портом", 
                Price = 1100, OldPrice = 1450, Stock = 35, ArticleNumber = "JYSK-018", IsNew = false, CategoryId = categories[2].Id,
                Images = [ new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1534105755980-c5253100a9ab", IsMain = true } ]
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}