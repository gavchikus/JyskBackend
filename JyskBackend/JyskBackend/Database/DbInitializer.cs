using JyskBackend.Entities;

namespace JyskBackend.Database;

public static class DbInitializer
{
    // Базовий шлях Unsplash + параметри масштабування (квадрат 800px, авто-формат).
    private const string U = "https://images.unsplash.com/";
    private const string Q = "?auto=format&fit=crop&w=800&q=80";

    private static ProductImage Img(string photo, bool main = false, int sort = 0) =>
        new() { ImageUrl = U + photo + Q, IsMain = main, SortOrder = sort };

    public static async Task SeedAsync(JyskDbContext context)
    {
        if (context.Products.Any()) return; // Если база уже не пустая, ничего не делаем

        var categories = new List<Category>
        {
            new()
            {
                Name = "Меблі для спальні",
                Description = "Ліжка, шафи, комоди та тумби для затишної спальні",
                ImageUrl = U + "photo-1522771739844-6a9f6d5f14af" + Q
            },
            new()
            {
                Name = "Вітальня",
                Description = "Дивани, крісла, стелажі та декор для вітальні",
                ImageUrl = U + "photo-1493663284031-b7e3aefcae8e" + Q
            },
            new()
            {
                Name = "Офіс та кабінет",
                Description = "Робочі столи та ергономічні крісла для продуктивності",
                ImageUrl = U + "photo-1518455027359-f3f8164ba6bd" + Q
            }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        int bedroom = categories[0].Id;
        int living = categories[1].Id;
        int office = categories[2].Id;

        var products = new List<Product>
        {
            // ─── Спальня ───
            new()
            {
                Id = Guid.NewGuid(), Name = "Ліжко кутове JYSK Vedde", CategoryId = bedroom,
                Description = "Комфортне двоспальне ліжко з м'яким узголів'ям та міцним каркасом.",
                Price = 11500, OldPrice = 14000, Stock = 12, IsNew = true,
                ArticleNumber = "JYSK-001", Brand = "JYSK", Material = "ДСП / тканина", Color = "Сірий", Dimensions = "160×200 см",
                Images = [ Img("photo-1505693416388-ac5ce068fe85", main: true), Img("photo-1616594039964-ae9021a400a0", sort: 1) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Ліжко двоспальне GABEL", CategoryId = bedroom,
                Description = "Класичне ліжко з натурального дерева для здорового сну.",
                Price = 9800, Stock = 9, IsNew = false,
                ArticleNumber = "JYSK-010", Brand = "GABEL", Material = "Масив дуба", Color = "Дуб", Dimensions = "180×200 см",
                Images = [ Img("photo-1522771739844-6a9f6d5f14af", main: true), Img("photo-1560448204-e02f11c3d0e2", sort: 1) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Шафа розсувна VINTERBRO", CategoryId = bedroom,
                Description = "Місткa шафа-купе з дзеркалом і трьома секціями.",
                Price = 15400, OldPrice = 17900, Stock = 6, IsNew = false,
                ArticleNumber = "JYSK-011", Brand = "JYSK", Material = "ЛДСП", Color = "Білий", Dimensions = "200×220 см",
                Images = [ Img("photo-1558997519-83ea9252edf8", main: true) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Тумба приліжкова NORESUND", CategoryId = bedroom,
                Description = "Компактна тумба з двома шухлядами.",
                Price = 2300, Stock = 25, IsNew = true,
                ArticleNumber = "JYSK-012", Brand = "JYSK", Material = "ДСП", Color = "Дуб сонома", Dimensions = "45×40 см",
                Images = [ Img("photo-1532372320572-cda25653a26d", main: true) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Комод HARLEV", CategoryId = bedroom,
                Description = "Просторий комод на шість шухляд для спальні чи передпокою.",
                Price = 6900, OldPrice = 8200, Stock = 10, IsNew = false,
                ArticleNumber = "JYSK-013", Brand = "GABEL", Material = "МДФ", Color = "Графіт", Dimensions = "120×80 см",
                Images = [ Img("photo-1595428774223-ef52624120d2", main: true) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Дзеркало підлогове STUBBERUP", CategoryId = bedroom,
                Description = "Велике підлогове дзеркало в тонкій металевій рамі.",
                Price = 3400, Stock = 14, IsNew = true,
                ArticleNumber = "JYSK-014", Brand = "JYSK", Material = "Метал / скло", Color = "Чорний", Dimensions = "50×170 см",
                Images = [ Img("photo-1618220179428-22790b461013", main: true) ]
            },

            // ─── Вітальня ───
            new()
            {
                Id = Guid.NewGuid(), Name = "Диван розкладний MARIBO", CategoryId = living,
                Description = "Стильний сірий диван з механізмом «єврокнижка» та нішею для білизни.",
                Price = 18900, Stock = 5, IsNew = true,
                ArticleNumber = "JYSK-002", Brand = "MARIBO", Material = "Рогожка", Color = "Сірий", Dimensions = "220×95 см",
                Images = [ Img("photo-1555041469-a586c61ea9bc", main: true), Img("photo-1493663284031-b7e3aefcae8e", sort: 1) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Диван кутовий VEJLBY", CategoryId = living,
                Description = "Просторий кутовий диван для великої родини та гостей.",
                Price = 24500, OldPrice = 28000, Stock = 4, IsNew = false,
                ArticleNumber = "JYSK-015", Brand = "MARIBO", Material = "Велюр", Color = "Синій", Dimensions = "280×180 см",
                Images = [ Img("photo-1550254478-ead40cc54513", main: true), Img("photo-1567016432779-094069958ea5", sort: 1) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Крісло м'яке AABENRAA", CategoryId = living,
                Description = "Затишне крісло для читання з високою спинкою.",
                Price = 7200, Stock = 11, IsNew = true,
                ArticleNumber = "JYSK-016", Brand = "JYSK", Material = "Букле", Color = "Молочний", Dimensions = "80×85 см",
                Images = [ Img("photo-1586023492125-27b2c045efd7", main: true) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Журнальний столик FROSTED", CategoryId = living,
                Description = "Лаконічний журнальний столик зі склом та металевими ніжками.",
                Price = 3900, OldPrice = 4800, Stock = 18, IsNew = false,
                ArticleNumber = "JYSK-017", Brand = "JYSK", Material = "Скло / метал", Color = "Прозорий", Dimensions = "100×50 см",
                Images = [ Img("photo-1499933374294-4584851497cc", main: true) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Стелаж BILLUND", CategoryId = living,
                Description = "Відкритий стелаж на п'ять полиць для книг та декору.",
                Price = 5400, Stock = 15, IsNew = false,
                ArticleNumber = "JYSK-018", Brand = "GABEL", Material = "ЛДСП", Color = "Дуб", Dimensions = "80×180 см",
                Images = [ Img("photo-1594620302200-9a762244a156", main: true), Img("photo-1497366216548-37526070297c", sort: 1) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Килим VISKUM", CategoryId = living,
                Description = "М'який короткий ворс, приємний на дотик і легкий у догляді.",
                Price = 2900, Stock = 30, IsNew = true,
                ArticleNumber = "JYSK-019", Brand = "JYSK", Material = "Поліпропілен", Color = "Бежевий", Dimensions = "160×230 см",
                Images = [ Img("photo-1600166898405-da9535204843", main: true) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Торшер AKSEL", CategoryId = living,
                Description = "Підлоговий світильник з тканинним абажуром і теплим світлом.",
                Price = 1850, OldPrice = 2400, Stock = 22, IsNew = false,
                ArticleNumber = "JYSK-020", Brand = "JYSK", Material = "Метал / тканина", Color = "Чорний", Dimensions = "40×150 см",
                Images = [ Img("photo-1507473885765-e6ed057f782c", main: true) ]
            },

            // ─── Офіс та кабінет ───
            new()
            {
                Id = Guid.NewGuid(), Name = "Стіл робочий Loft Desk", CategoryId = office,
                Description = "Ергономічний стіл у стилі лофт з металевим каркасом.",
                Price = 4200, OldPrice = 5500, Stock = 20, IsNew = false,
                ArticleNumber = "JYSK-003", Brand = "JYSK", Material = "ДСП / метал", Color = "Дуб / чорний", Dimensions = "120×60 см",
                Images = [ Img("photo-1518455027359-f3f8164ba6bd", main: true), Img("photo-1593062096033-9a26b09da705", sort: 1) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Стіл письмовий AUSTIN", CategoryId = office,
                Description = "Компактний письмовий стіл з двома шухлядами для дому.",
                Price = 5600, Stock = 13, IsNew = true,
                ArticleNumber = "JYSK-021", Brand = "GABEL", Material = "МДФ", Color = "Білий", Dimensions = "110×55 см",
                Images = [ Img("photo-1524758631624-e2822e304c36", main: true) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Крісло офісне TJELE", CategoryId = office,
                Description = "Офісне крісло з еко-шкіри з регулюванням висоти та підлокітниками.",
                Price = 6700, OldPrice = 8000, Stock = 8, IsNew = false,
                ArticleNumber = "JYSK-004", Brand = "JYSK", Material = "Еко-шкіра", Color = "Чорний", Dimensions = "60×60 см",
                Images = [ Img("photo-1592078615290-033ee584e267", main: true), Img("photo-1567538096630-e0c55bd6374c", sort: 1) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Крісло робоче SVANE", CategoryId = office,
                Description = "Сітчаста спинка для вентиляції та підтримки спини протягом дня.",
                Price = 4900, Stock = 16, IsNew = true,
                ArticleNumber = "JYSK-022", Brand = "JYSK", Material = "Сітка / пластик", Color = "Сірий", Dimensions = "58×58 см",
                Images = [ Img("photo-1519947486511-46149fa0a254", main: true) ]
            },
            new()
            {
                Id = Guid.NewGuid(), Name = "Стілець ергономічний HERNING", CategoryId = office,
                Description = "Легкий стілець з дерев'яними ніжками для кабінету чи кухні.",
                Price = 3200, Stock = 24, IsNew = false,
                ArticleNumber = "JYSK-023", Brand = "GABEL", Material = "Дерево / тканина", Color = "Гірчичний", Dimensions = "45×80 см",
                Images = [ Img("photo-1503602642458-232111445657", main: true) ]
            }
        };

        // Рознесемо CreatedAt, щоб блок «Новинки» мав осмислений порядок (перші — найновіші).
        var now = DateTime.UtcNow;
        for (int i = 0; i < products.Count; i++)
            products[i].CreatedAt = now.AddMinutes(-i);

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }
}
